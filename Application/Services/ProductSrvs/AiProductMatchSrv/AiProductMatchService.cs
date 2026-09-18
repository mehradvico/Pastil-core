using Application.Common.Dto.Result;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.ProductSrvs.AiProductMatchSrv.Dto;
using Application.Services.ProductSrvs.AiProductMatchSrv.Iface;
using Application.Services.ProductSrvs.ProductSrv.Iface;
using Application.Common.Helpers;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.AiProductMatchSrv
{
    public class AiProductMatchService : IAiProductMatchService
    {
        private const string IssueNotFoundInCatalog = "در کاتالوگ پاستیل پیدا نشد";
        private const string IssueNoAutoMatch = "امکان تطبیق خودکار وجود نداشت؛ لطفاً به‌صورت دستی انتخاب کنید";
        private const string IssueMultipleCloseMatches = "چند محصول مشابه یافت شد؛ لطفاً به‌صورت دستی انتخاب کنید";
        private const double MinimumSuggestedMatchConfidence = 0.60;
        // فاصله‌ی امتیاز کاندید اول و دوم؛ کمتر از این یعنی مدل واقعاً بین دو محصول شبیه‌هم مردد بوده.
        private const double AmbiguityMarginThreshold = 0.08;

        private static readonly JsonSerializerOptions RowsJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IDataBaseContext _context;
        private readonly IProductService _productService;
        private readonly IAiProductMatchGeminiClient _geminiClient;
        private readonly IAiProductMatchFileClient _fileClient;
        private readonly AiProductMatchOptions _options;
        private readonly ILogger<AiProductMatchService> _logger;

        public AiProductMatchService(
            IDataBaseContext context,
            IProductService productService,
            IAiProductMatchGeminiClient geminiClient,
            IAiProductMatchFileClient fileClient,
            IOptions<AiProductMatchOptions> options,
            ILogger<AiProductMatchService> logger)
        {
            _context = context;
            _productService = productService;
            _geminiClient = geminiClient;
            _fileClient = fileClient;
            _options = options.Value;
            _logger = logger;
        }

        public Task<BaseResultDto<AiProductMatchStatusDto>> GetStatusAsync()
        {
            var geminiAvailable = _geminiClient.IsAvailable(out var providerName);
            var available = _options.Enabled && geminiAvailable;
            return Task.FromResult(new BaseResultDto<AiProductMatchStatusDto>(
                true,
                new AiProductMatchStatusDto { Available = available, Provider = available ? providerName : null }));
        }

        public async Task<BaseResultDto<AiProductMatchAnalyzeResultDto>> AnalyzeAsync(
            long storeId,
            AiProductMatchAnalyzeInputDto dto,
            string authorizationHeaderValue,
            CancellationToken cancellationToken,
            Action<int, int> onBatchProgress = null)
        {
            if (!_options.Enabled || !_geminiClient.IsAvailable(out _))
                return Fail(Resource.Notification.AiProductMatchServiceUnavailable, 6);

            var sourceType = dto.SourceType?.Trim().ToLowerInvariant();
            var validSourceTypes = new[] { "shelf", "excel", "sepidar", "veterinary" };
            if (string.IsNullOrWhiteSpace(sourceType) || !validSourceTypes.Contains(sourceType))
                return Fail(Resource.Notification.AiProductMatchInvalidSourceType, 1);

            // veterinary/sepidar دو مسیر ورودی دارند: rowsJson (وقتی Bridge توانسته جدول را متنی بخواند،
            // مسیر قبلی و بدون تغییر) یا images (وقتی نتوانسته و به‌جایش از جدول اسکرین‌شات گرفته).
            var isTableCaptureSource = sourceType == "veterinary" || sourceType == "sepidar";

            var hasImages = dto.Images != null && dto.Images.Count > 0;
            var hasRowsJson = !string.IsNullOrWhiteSpace(dto.RowsJson);
            if (!hasImages && !hasRowsJson)
                return Fail(Resource.Notification.AiProductMatchInvalidInput, 1);

            if (isTableCaptureSource && hasImages && hasRowsJson)
                return Fail(Resource.Notification.AiProductMatchInvalidInput, 1);

            var isImageTableCapture = isTableCaptureSource && hasImages;
            var maxImages = isImageTableCapture ? _options.MaxTableImagesPerRequest : _options.MaxImagesPerRequest;

            if (hasImages && dto.Images.Count > maxImages)
                return Fail(Resource.Notification.AiProductMatchInvalidInput, 1);

            if (hasImages && dto.Images.Any(image => image == null || image.Length <= 0 || image.Length > _options.MaxImageSizeBytes))
                return Fail(Resource.Notification.AiProductMatchInvalidInput, 1);

            List<AiProductMatchRowInputDto> parsedRows = new();
            if (hasRowsJson)
            {
                try
                {
                    parsedRows = JsonSerializer.Deserialize<List<AiProductMatchRowInputDto>>(dto.RowsJson, RowsJsonOptions) ?? new();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "AiProductMatch failed to parse RowsJson for store {StoreId}.", storeId);
                    return Fail(Resource.Notification.AiProductMatchInvalidInput, 1);
                }
            }

            // آپلود تصاویر (اگر وجود دارد) به سرویس File — طبق تصمیم ۰.۶، برای نگهداری دائمی جهت بازبینی بعدی
            // موازی: هر آپلود یک HTTP Call مستقل به File Service است؛ با چند عکس، این توالی خودش چند ثانیه از
            // بودجه‌ی زمانی کلاینت را می‌بلعید (طبق لاگ واقعی: ~۳-۴ ثانیه به‌ازای هر عکس، پشت‌سرهم).
            var uploadedImages = new List<(IFormFile Image, long? PictureId)>();
            if (hasImages)
            {
                var uploadTasks = dto.Images.Select(async image =>
                {
                    var (ok, pictureId, error) = await _fileClient.UploadAsync(image, authorizationHeaderValue, cancellationToken);
                    if (!ok)
                        _logger.LogWarning("AiProductMatch failed to persist an uploaded image for store {StoreId}: {Error}", storeId, error);
                    return (Image: image, PictureId: ok ? pictureId : (long?)null);
                });
                uploadedImages.AddRange(await Task.WhenAll(uploadTasks));
            }

            var workingRows = new List<AiProductMatchWorkingRow>();

            if (sourceType == "shelf")
            {
                // موازی: هر عکس یک Call مستقل به Gemini/GapGPT/AvalAI است (تا ~RequestTimeoutSeconds هرکدام)؛
                // اجرای پشت‌سرهم برای چند عکس، بودجه‌ی زمانی کلاینت را چند برابر می‌کند بدون هیچ اشتراک state.
                var completedImages = 0;
                var totalImages = uploadedImages.Count;
                var extractionTasks = uploadedImages.Select(async (uploaded, index) =>
                {
                    var imageIndex = index + 1;
                    var (image, pictureId) = uploaded;
                    try
                    {
                        byte[] bytes;
                        await using (var stream = image.OpenReadStream())
                        await using (var memory = new System.IO.MemoryStream())
                        {
                            await stream.CopyToAsync(memory, cancellationToken);
                            bytes = memory.ToArray();
                        }

                        var extraction = await _geminiClient.GenerateJsonAsync(
                            AiProductMatchPrompts.ShelfExtractionSystemInstruction,
                            AiProductMatchPrompts.ShelfExtractionUserText,
                            new List<(string, byte[])> { (string.IsNullOrWhiteSpace(image.ContentType) ? "image/jpeg" : image.ContentType, bytes) },
                            cancellationToken);

                        var rowsForImage = new List<AiProductMatchWorkingRow>();
                        if (!extraction.IsSuccess)
                        {
                            _logger.LogWarning("AiProductMatch shelf extraction failed for image {ImageIndex}, store {StoreId}: {Error}", imageIndex, storeId, extraction.ErrorCode);
                            return rowsForImage;
                        }

                        var extractedRows = AiProductMatchGeminiResponseParser.ParseShelfExtraction(extraction.RawJson);
                        var itemIndex = 0;
                        foreach (var extracted in extractedRows)
                        {
                            itemIndex++;
                            rowsForImage.Add(new AiProductMatchWorkingRow
                            {
                                RowId = $"shelf-{imageIndex}-{itemIndex}",
                                Name = extracted.DetectedName,
                                Brand = extracted.Brand,
                                AnimalType = extracted.AnimalType,
                                PackageSizeValue = extracted.PackageSizeValue,
                                PackageSizeUnit = extracted.PackageSizeUnit,
                                Price = extracted.PriceGuess,
                                Quantity = extracted.QuantityGuess,
                                Unit = extracted.Unit,
                                SourcePictureId = pictureId
                            });
                        }
                        return rowsForImage;
                    }
                    finally
                    {
                        var completed = Interlocked.Increment(ref completedImages);
                        onBatchProgress?.Invoke(completed, totalImages);
                    }
                });

                foreach (var rowsForImage in await Task.WhenAll(extractionTasks))
                    workingRows.AddRange(rowsForImage);

                // چند عکس هم‌پوشان از یک قفسه می‌توانند همان SKU را دوباره برگردانند؛ این یک محصول است، نه چند ردیف.
                workingRows = AiProductMatchMatchingHelper.DeduplicateRows(workingRows);

                if (workingRows.Count == 0)
                    return Fail(Resource.Notification.AiProductMatchUnanalyzable, 3);
            }
            else if (isImageTableCapture)
            {
                workingRows = await ExtractTableCaptureRowsAsync(uploadedImages, storeId, cancellationToken, onBatchProgress);

                // اسکرول صفحه‌به‌صفحه یعنی یک ردیف می‌تواند در دو اسکرین‌شات پشت‌سرهم دیده شود.
                workingRows = AiProductMatchMatchingHelper.DeduplicateRows(workingRows);

                if (workingRows.Count == 0)
                    return Fail(Resource.Notification.AiProductMatchUnanalyzable, 3);
            }
            else
            {
                var rowIndex = 0;
                foreach (var row in parsedRows)
                {
                    if (string.IsNullOrWhiteSpace(row.Name))
                        continue;

                    rowIndex++;
                    workingRows.Add(new AiProductMatchWorkingRow
                    {
                        RowId = string.IsNullOrWhiteSpace(row.RowId) ? $"row-{rowIndex}" : row.RowId,
                        Name = row.Name,
                        ExternalCode = row.ExternalCode,
                        Price = row.Price,
                        Quantity = row.Quantity,
                        Unit = row.Unit
                    });
                }

                if (workingRows.Count == 0)
                    return Fail(Resource.Notification.AiProductMatchInvalidInput, 1);
            }

            var candidatesByRow = await FindCandidatesForAllRowsAsync(workingRows, storeId, cancellationToken);

            var ranked = new List<AiProductMatchRankedRowResult>();
            if (candidatesByRow.Values.Any(list => list.Count > 0))
            {
                var matchUserText = AiProductMatchPrompts.BuildMatchUserText(dto.Currency, workingRows, candidatesByRow);
                var matchResult = await _geminiClient.GenerateJsonAsync(
                    AiProductMatchPrompts.MatchSystemInstruction,
                    matchUserText,
                    Array.Empty<(string, byte[])>(),
                    cancellationToken);

                if (matchResult.IsSuccess)
                {
                    ranked = AiProductMatchGeminiResponseParser.ParseMatchResults(matchResult.RawJson);
                }
                else
                {
                    _logger.LogWarning("AiProductMatch matching stage failed for store {StoreId}: {Error}", storeId, matchResult.ErrorCode);
                }
            }

            var items = new List<AiProductMatchResultItemDto>();
            foreach (var row in workingRows)
            {
                var candidates = candidatesByRow[row.RowId];
                var item = new AiProductMatchResultItemDto
                {
                    RowId = row.RowId,
                    DetectedName = row.Name,
                    ExternalCode = row.ExternalCode,
                    Price = row.Price,
                    Quantity = row.Quantity,
                    SourcePictureId = row.SourcePictureId
                };

                // سلول ناخوانا (قیمت/تعداد/...) هنگام استخراج از عکس — نه رد کل ردیف، فقط هشدار به فروشنده.
                if (!string.IsNullOrWhiteSpace(row.ExtractionIssue))
                    item.Issues.Add(row.ExtractionIssue);

                // PackageIndex فقط یک اشاره‌ی داخلی به packageهای واقعی همین کاندید است؛ هیچ شناسه‌ای از
                // مدل گرفته نمی‌شود. اگر مدل درباره‌ی تنوع مطمئن نباشد، ProductItemId خالی می‌ماند تا
                // فروشنده از فهرست packages انتخاب کند، نه این‌که اولین/موجودترین تنوع اشتباه انتخاب شود.
                var suggestedPackagesByProductId = new Dictionary<long, AiProductMatchPackageDto>();

                if (candidates.Count == 0)
                {
                    item.Issues.Add(IssueNotFoundInCatalog);
                    items.Add(item);
                    continue;
                }

                var rowRanked = ranked.FirstOrDefault(r => r.RowId == row.RowId);
                if (rowRanked != null)
                {
                    var usedCandidateIndexes = new HashSet<int>();
                    foreach (var rankedCandidate in rowRanked.Ranked)
                    {
                        // محافظت در برابر Hallucination: هر اندیسی خارج از بازه‌ی واقعی کاندیدها نادیده گرفته می‌شود
                        if (rankedCandidate.Index < 0 || rankedCandidate.Index >= candidates.Count ||
                            !usedCandidateIndexes.Add(rankedCandidate.Index))
                            continue;

                        if (!double.IsFinite(rankedCandidate.Confidence))
                            continue;

                        var candidate = candidates[rankedCandidate.Index];
                        // امتیاز نهایی فقط عدد خام مدل نیست: شباهت نام نرمال‌شده هم وزن دارد، چون مدل روی
                        // اسم‌های شبیه‌هم (طعم/سایز متفاوت از یک برند) اغلب بیش‌ازحد مطمئن است.
                        var confidence = AiProductMatchMatchingHelper.ComputeCompositeConfidence(
                            rankedCandidate.Confidence, row.Name, row.Brand, candidate.Name, candidate.BrandName);
                        if (confidence < MinimumSuggestedMatchConfidence)
                            continue;

                        item.Matches.Add(new AiProductMatchCandidateDto
                        {
                            ProductId = candidate.ProductId,
                            Name = candidate.Name,
                            Confidence = confidence,
                            ProductItems = candidate.Packages
                        });

                        var suggestedPackage = FindSuggestedPackage(candidate.Packages, rankedCandidate.PackageIndex);
                        if (suggestedPackage != null)
                            suggestedPackagesByProductId[candidate.ProductId] = suggestedPackage;

                        if (item.Matches.Count == 3)
                            break;
                    }
                }

                var orderedMatches = item.Matches.OrderByDescending(match => match.Confidence).ToList();
                item.Matches = orderedMatches;
                var best = orderedMatches.FirstOrDefault();
                var second = orderedMatches.Skip(1).FirstOrDefault();
                // فاصله‌ی امتیاز کم بین دو کاندید برتر یعنی تصمیم مطمئنی وجود ندارد؛ به‌جای حدس، به فروشنده واگذار می‌شود.
                var isAmbiguous = best != null && AiProductMatchMatchingHelper.IsAmbiguous(
                    best.Confidence, second?.Confidence, MinimumSuggestedMatchConfidence, AmbiguityMarginThreshold);

                if (best != null && !isAmbiguous && best.Confidence >= _options.AutoSelectConfidenceThreshold)
                {
                    item.ProductId = best.ProductId;
                    item.ProductName = best.Name;
                    item.Confidence = best.Confidence;
                    suggestedPackagesByProductId.TryGetValue(best.ProductId, out var bestPackage);
                    // تک-تنوعی بودن تنها حالت امن برای انتخاب خودکار بدون شواهد تنوع از مدل است.
                    bestPackage ??= best.ProductItems.Count == 1 ? best.ProductItems[0] : null;
                    item.ProductItemId = bestPackage?.ProductItemId;
                }
                else
                {
                    item.Issues.Add(isAmbiguous ? IssueMultipleCloseMatches : IssueNoAutoMatch);
                }

                items.Add(item);
            }

            return new BaseResultDto<AiProductMatchAnalyzeResultDto>(true, new AiProductMatchAnalyzeResultDto { Items = items });
        }

        // اسکرین‌شات‌های جدول نرم‌افزار انبار را در دسته‌های چندتایی (نه یکی‌یکی مثل قفسه) به مدل
        // می‌فرستد: با تا ۴۰ صفحه، ۴۰ فراخوانی موازی جدا هم هزینه‌ی تکرار system prompt را ۴۰ برابر
        // می‌کند و هم ریسک Rate-Limit سمت Provider را بالا می‌برد. SourcePictureId عمداً خالی می‌ماند
        // چون هر دسته چند تصویر دارد و نمی‌شود یک ردیف را با قطعیت به یکی از آن‌ها نسبت داد — مستند
        // فرانت هم این فیلد را برای veterinary/sepidar لازم نداشت.
        private async Task<List<AiProductMatchWorkingRow>> ExtractTableCaptureRowsAsync(
            List<(IFormFile Image, long? PictureId)> uploadedImages,
            long storeId,
            CancellationToken cancellationToken,
            Action<int, int> onBatchProgress = null)
        {
            var batchSize = Math.Max(1, _options.TableImagesPerVisionCall);
            var batches = uploadedImages
                .Select((uploaded, index) => (uploaded, index))
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.uploaded).ToList())
                .ToList();

            var completedBatches = 0;
            var totalBatches = batches.Count;

            var extractionTasks = batches.Select(async (batch, batchIndex) =>
            {
                try
                {
                    var images = new List<(string MimeType, byte[] Bytes)>();
                    foreach (var (image, _) in batch)
                    {
                        byte[] bytes;
                        await using (var stream = image.OpenReadStream())
                        await using (var memory = new System.IO.MemoryStream())
                        {
                            await stream.CopyToAsync(memory, cancellationToken);
                            bytes = memory.ToArray();
                        }
                        images.Add((string.IsNullOrWhiteSpace(image.ContentType) ? "image/png" : image.ContentType, bytes));
                    }

                    var extraction = await _geminiClient.GenerateJsonAsync(
                        AiProductMatchPrompts.TableCaptureExtractionSystemInstruction,
                        AiProductMatchPrompts.TableCaptureExtractionUserText,
                        images,
                        cancellationToken);

                    var rowsForBatch = new List<AiProductMatchWorkingRow>();
                    if (!extraction.IsSuccess)
                    {
                        _logger.LogWarning("AiProductMatch table-capture extraction failed for batch {BatchIndex}, store {StoreId}: {Error}", batchIndex + 1, storeId, extraction.ErrorCode);
                        return rowsForBatch;
                    }

                    var extractedRows = AiProductMatchGeminiResponseParser.ParseTableCaptureExtraction(extraction.RawJson);
                    var itemIndex = 0;
                    foreach (var extracted in extractedRows)
                    {
                        itemIndex++;
                        rowsForBatch.Add(new AiProductMatchWorkingRow
                        {
                            RowId = $"table-{batchIndex + 1}-{itemIndex}",
                            Name = extracted.DetectedName,
                            Brand = extracted.Brand,
                            AnimalType = extracted.AnimalType,
                            PackageSizeValue = extracted.PackageSizeValue,
                            PackageSizeUnit = extracted.PackageSizeUnit,
                            Price = extracted.Price,
                            Quantity = extracted.Quantity,
                            ExternalCode = extracted.ExternalCode,
                            ExtractionIssue = extracted.ExtractionIssue
                        });
                    }
                    return rowsForBatch;
                }
                finally
                {
                    var completed = Interlocked.Increment(ref completedBatches);
                    onBatchProgress?.Invoke(completed, totalBatches);
                }
            });

            var result = new List<AiProductMatchWorkingRow>();
            foreach (var rowsForBatch in await Task.WhenAll(extractionTasks))
                result.AddRange(rowsForBatch);
            return result;
        }

        // موازی‌سازی جست‌وجوی کاندید برای همه‌ی ردیف‌ها هم‌زمان، در دو فاز:
        // فاز ۱) هر ردیف یک Query مستقل روی Dapper/SqlConnection خودش می‌زند (Thread-safe، چون هرکدام
        //        Connection جدای خودش را باز می‌کند) — قبلاً این حلقه پشت‌سرهم (Sequential) اجرا می‌شد و با
        //        چند ردیف (مثلاً ۷ آیتم تشخیص‌داده‌شده از عکس)، مجموع Timeout ها به‌سرعت جمع می‌شد.
        // فاز ۲) هیدریت (Products/ProductItems) روی _context یک‌بار و برای اجتماع همه‌ی شناسه‌ها انجام
        //        می‌شود — چون DbContext را نمی‌توان هم‌زمان از چند Task صدا زد (Thread-unsafe)، و این کار
        //        تعداد Round-trip های EF را هم از ۲×تعداد‌ردیف به فقط ۲ کاهش می‌دهد.
        private async Task<Dictionary<string, List<AiProductMatchCandidateProduct>>> FindCandidatesForAllRowsAsync(
            List<AiProductMatchWorkingRow> rows,
            long storeId,
            CancellationToken cancellationToken)
        {
            var searchTasks = rows.Select(async row => new
            {
                row.RowId,
                ProductIds = await SearchProductIdsAsync(row.Name, cancellationToken)
            });
            var searchResults = await Task.WhenAll(searchTasks);

            var allProductIds = searchResults.SelectMany(r => r.ProductIds).Distinct().ToList();
            var (productById, itemsByProductId) = await LoadCandidateProductsAsync(allProductIds, cancellationToken);

            var candidatesByRow = new Dictionary<string, List<AiProductMatchCandidateProduct>>();
            foreach (var searchResult in searchResults)
                candidatesByRow[searchResult.RowId] = BuildCandidates(searchResult.ProductIds, productById, itemsByProductId, storeId);

            return candidatesByRow;
        }

        private async Task<List<long>> SearchProductIdsAsync(string rawName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(rawName))
                return new List<long>();

            var request = new SearchRequestDto
            {
                Q = rawName,
                ProductCount = _options.CandidateShortlistSize,
                BrandCount = 0,
                CategoryCount = 0,
                FeatureCount = 0,
                CompanionCount = 0,
                AssistanceCount = 0,
                StoreCount = 0,
                PansionCount = 0,
                PackageCount = 0,
                TotalCount = _options.CandidateShortlistSize,
                EnableFuzzy = true
            };
            // همان دو خط دقیق SearchService.SearchAsync قبل از فراخوانی SearchMinAsync
            request.Q = SearchNormalizeHelper.Normalize(request.Q);
            request.ClampCounts();
            request.SearchTerms = SearchNormalizeHelper.BuildTerms(request.Q, request.EnableFuzzy);

            if (string.IsNullOrWhiteSpace(request.Q) || request.Q.Length < 2)
                return new List<long>();

            try
            {
                // عمداً SearchMinAsync نیست: اون متد برای جستجوی مشتری طراحی شده و فقط محصولاتی که
                // حداقل یک فروشگاه فعال با موجودی>۰ دارند برمی‌گردونه. اینجا دقیقاً برعکسش لازمه —
                // محصولاتی که فروشنده‌ی فعلی (یا هیچ فروشگاهی) هنوز براشون موجودی ثبت نکرده، چون
                // کل هدف این فیچر همینه: پیدا کردن محصول کاتالوگ برای ساختن اولین ProductItem آن.
                var found = await _productService.SearchCatalogProductIdsAsync(request, cancellationToken);
                return found == null ? new List<long>() : found.Distinct().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AiProductMatch candidate search failed for raw name '{RawName}'.", rawName);
                return new List<long>();
            }
        }

        private async Task<(Dictionary<long, Product> ProductById, Dictionary<long, List<ProductItem>> ItemsByProductId)> LoadCandidateProductsAsync(
            List<long> productIds,
            CancellationToken cancellationToken)
        {
            if (productIds.Count == 0)
                return (new Dictionary<long, Product>(), new Dictionary<long, List<ProductItem>>());

            var products = await _context.Products
                .Include(p => p.Brand)
                .Where(p => productIds.Contains(p.Id) && p.Active && !p.Deleted)
                .ToListAsync(cancellationToken);

            var productById = products.ToDictionary(p => p.Id);

            var allItems = await _context.ProductItems
                .Include(pi => pi.VarietyItem)
                .Include(pi => pi.VarietyItem2)
                .Where(pi => productIds.Contains(pi.ProductId) && !pi.Deleted)
                .ToListAsync(cancellationToken);

            var itemsByProductId = allItems.GroupBy(pi => pi.ProductId).ToDictionary(g => g.Key, g => g.ToList());

            return (productById, itemsByProductId);
        }

        private static List<AiProductMatchCandidateProduct> BuildCandidates(
            List<long> productIds,
            Dictionary<long, Product> productById,
            Dictionary<long, List<ProductItem>> itemsByProductId,
            long storeId)
        {
            var result = new List<AiProductMatchCandidateProduct>();
            foreach (var productId in productIds)
            {
                if (!productById.TryGetValue(productId, out var product))
                    continue;

                itemsByProductId.TryGetValue(productId, out var itemsForProduct);
                result.Add(new AiProductMatchCandidateProduct
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    BrandName = product.Brand?.Name,
                    CodeValue = product.CodeValue,
                    Packages = BuildPackages(itemsForProduct ?? new List<ProductItem>(), storeId)
                });
            }

            return result;
        }

        private static AiProductMatchPackageDto FindSuggestedPackage(
            List<AiProductMatchPackageDto> packages,
            int? packageIndex)
        {
            if (!packageIndex.HasValue || packageIndex.Value < 0 || packageIndex.Value >= packages.Count)
                return null;

            return packages[packageIndex.Value];
        }

        private static List<AiProductMatchPackageDto> BuildPackages(List<ProductItem> productItems, long storeId)
        {
            var result = new List<AiProductMatchPackageDto>();

            var groups = productItems.GroupBy(pi => (pi.VarietyItemId, pi.VarietyItem2Id));
            foreach (var group in groups)
            {
                var varietyItemName = group.Select(pi => pi.VarietyItem?.Name).FirstOrDefault(n => n != null);
                var varietyItem2Name = group.Select(pi => pi.VarietyItem2?.Name).FirstOrDefault(n => n != null);
                var storeItem = group.FirstOrDefault(pi => pi.StoreId == storeId);

                string label;
                if (group.Key.VarietyItemId == null && group.Key.VarietyItem2Id == null)
                    label = null;
                else if (group.Key.VarietyItem2Id == null)
                    label = varietyItemName;
                else
                    label = $"{varietyItemName} / {varietyItem2Name}";

                result.Add(new AiProductMatchPackageDto
                {
                    ProductItemId = storeItem?.Id,
                    Label = label,
                    VarietyItemId = group.Key.VarietyItemId,
                    VarietyItem2Id = group.Key.VarietyItem2Id,
                    ExistsForCurrentStore = storeItem != null
                });
            }

            return result;
        }

        private static BaseResultDto<AiProductMatchAnalyzeResultDto> Fail(string message, int code)
            => new(false, message, null, code);
    }
}
