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
        private const string IssueCatalogMatchIncomplete = "تطبیق با کاتالوگ کامل نشد؛ لطفاً دوباره تلاش کنید";
        private const double MinimumSuggestedMatchConfidence = 0.60;

        private sealed record BufferedImage(byte[] Bytes, string ContentType, string FileName, string Name);
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

            // بودجهٔ زمانی کل تحلیل: اپ سقف HTTP حدود ۶۰ ثانیه دارد و باید نتیجهٔ ناقص بگیرد، نه Timeout.
            // استخراج از عکس بخشی از بودجه را می‌گیرد و بقیه برای جست‌وجوی کاتالوگ + تطبیق رزرو می‌ماند.
            var totalBudget = TimeSpan.FromSeconds(Math.Max(5, _options.TotalBudgetSeconds));
            using var overallCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            overallCts.CancelAfter(totalBudget);
            using var extractionCts = CancellationTokenSource.CreateLinkedTokenSource(overallCts.Token);
            extractionCts.CancelAfter(totalBudget - TimeSpan.FromSeconds(Math.Clamp(_options.MatchStageReserveSeconds, 0, (int)totalBudget.TotalSeconds - 1)));
            var overallToken = overallCts.Token;
            var extractionToken = extractionCts.Token;

            // بایت‌ها یک‌بار به حافظه خوانده می‌شود: آپلود به سرویس File (~۳-۴ ثانیه) هم‌زمان با استخراج اجرا
            // می‌شود و بایت‌های حافظه‌ای برخلاف استریم فرم HTTP، هم‌زمان‌خوانی امن و بعد از پایان درخواست هم معتبرند.
            var bufferedImages = hasImages ? await BufferImagesAsync(dto.Images, cancellationToken) : new List<BufferedImage>();
            var uploadTask = hasImages
                ? UploadImagesAsync(bufferedImages, storeId, authorizationHeaderValue)
                : Task.FromResult(Array.Empty<long?>());

            var workingRows = new List<AiProductMatchWorkingRow>();
            var extractionFailures = 0;

            if (sourceType == "shelf")
            {
                // موازی: هر عکس یک Call مستقل به Provider است (تا ~RequestTimeoutSeconds هرکدام)؛
                // اجرای پشت‌سرهم برای چند عکس، بودجه‌ی زمانی کلاینت را چند برابر می‌کند بدون هیچ اشتراک state.
                var completedImages = 0;
                var totalImages = bufferedImages.Count;
                var extractionTasks = bufferedImages.Select(async (image, index) =>
                {
                    var imageIndex = index + 1;
                    try
                    {
                        var extraction = await CallModelAsync(
                            AiProductMatchPrompts.ShelfExtractionSystemInstruction,
                            AiProductMatchPrompts.ShelfExtractionUserText,
                            new List<(string MimeType, byte[] Bytes)> { (string.IsNullOrWhiteSpace(image.ContentType) ? "image/jpeg" : image.ContentType, image.Bytes) },
                            extractionToken,
                            cancellationToken);

                        var rowsForImage = new List<AiProductMatchWorkingRow>();
                        if (!extraction.IsSuccess)
                        {
                            _logger.LogWarning("AiProductMatch shelf extraction failed for image {ImageIndex}, store {StoreId}: {Error}", imageIndex, storeId, extraction.ErrorCode);
                            Interlocked.Increment(ref extractionFailures);
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
                                NameConfidence = extracted.NameConfidence.HasValue && double.IsFinite(extracted.NameConfidence.Value)
                                    ? Math.Clamp(extracted.NameConfidence.Value, 0, 1)
                                    : null,
                                BoundingBoxes = AiProductMatchMatchingHelper.NormalizeBoxes(extracted.Boxes),
                                SourceImageIndex = index
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

                // آپلود معمولاً همین حالا تمام شده؛ اگر نه، بیش از چند ثانیه منتظرش نمی‌مانیم (PictureId فقط برای بازبینی است).
                long?[] pictureIds = Array.Empty<long?>();
                try
                {
                    pictureIds = await uploadTask.WaitAsync(TimeSpan.FromSeconds(8), cancellationToken);
                }
                catch (TimeoutException)
                {
                    _logger.LogWarning("AiProductMatch image upload did not finish in time for store {StoreId}; SourcePictureId left empty.", storeId);
                }
                foreach (var row in workingRows)
                    if (row.SourceImageIndex is int imageSlot && imageSlot < pictureIds.Length)
                        row.SourcePictureId = pictureIds[imageSlot];

                if (workingRows.Count == 0)
                    return extractionFailures > 0
                        ? Fail(Resource.Notification.AiProductMatchGatewayError, 5)
                        : Fail(Resource.Notification.AiProductMatchUnanalyzable, 3);
            }
            else if (isImageTableCapture)
            {
                var (tableRows, tableFailures) = await ExtractTableCaptureRowsAsync(bufferedImages, storeId, extractionToken, cancellationToken, onBatchProgress);
                workingRows = tableRows;
                extractionFailures += tableFailures;

                // اسکرول صفحه‌به‌صفحه یعنی یک ردیف می‌تواند در دو اسکرین‌شات پشت‌سرهم دیده شود.
                workingRows = AiProductMatchMatchingHelper.DeduplicateRows(workingRows);

                if (workingRows.Count == 0)
                    return extractionFailures > 0
                        ? Fail(Resource.Notification.AiProductMatchGatewayError, 5)
                        : Fail(Resource.Notification.AiProductMatchUnanalyzable, 3);
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

            // خطا/اتمام بودجه در جست‌وجوی کاتالوگ یا تطبیق نباید با «واقعاً در کاتالوگ نیست» یکی شود:
            // اپ ردیف با matches خالی را «ناموجود» گزارش می‌کند، پس این ردیف‌ها CatalogMatchFailed می‌گیرند.
            Dictionary<string, List<AiProductMatchCandidateProduct>> candidatesByRow;
            HashSet<string> searchFailedRowIds;
            try
            {
                (candidatesByRow, searchFailedRowIds) = await FindCandidatesForAllRowsAsync(workingRows, storeId, overallToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "AiProductMatch candidate stage failed for store {StoreId}.", storeId);
                candidatesByRow = workingRows.ToDictionary(row => row.RowId, _ => new List<AiProductMatchCandidateProduct>());
                searchFailedRowIds = workingRows.Select(row => row.RowId).ToHashSet();
            }

            var ranked = new List<AiProductMatchRankedRowResult>();
            var failedMatchRowIds = new HashSet<string>();
            var rowsWithCandidates = workingRows.Where(row => candidatesByRow[row.RowId].Count > 0).ToList();
            if (rowsWithCandidates.Count > 0)
            {
                // دسته‌های کوچک و موازی: هر دسته یک فراخوانی مستقل به مدل است، پس کندی/شکست یک دسته فقط ردیف‌های
                // همان دسته را CatalogMatchFailed می‌کند، نه کل نتیجه را.
                var batches = rowsWithCandidates.Chunk(Math.Max(1, _options.MatchRowsPerCall)).ToList();
                var batchResults = await Task.WhenAll(batches.Select(async batch =>
                {
                    var matchUserText = AiProductMatchPrompts.BuildMatchUserText(dto.Currency, batch, candidatesByRow);
                    var matchResult = await CallModelAsync(
                        AiProductMatchPrompts.MatchSystemInstruction,
                        matchUserText,
                        Array.Empty<(string MimeType, byte[] Bytes)>(),
                        overallToken,
                        cancellationToken);
                    return (Batch: batch, Result: matchResult, PromptChars: matchUserText.Length);
                }));

                foreach (var (batch, matchResult, promptChars) in batchResults)
                {
                    if (matchResult.IsSuccess)
                    {
                        ranked.AddRange(AiProductMatchGeminiResponseParser.ParseMatchResults(matchResult.RawJson));
                    }
                    else
                    {
                        foreach (var failedRow in batch)
                            failedMatchRowIds.Add(failedRow.RowId);
                        _logger.LogWarning("AiProductMatch matching batch ({Rows} rows, {PromptChars} chars) failed for store {StoreId}: {Error}",
                            batch.Length, promptChars, storeId, matchResult.ErrorCode);
                    }
                }
            }

            var items = new List<AiProductMatchResultItemDto>();
            var locallyRankedRowIds = new HashSet<string>();
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
                    SourcePictureId = row.SourcePictureId,
                    Brand = string.IsNullOrWhiteSpace(row.Brand) ? null : row.Brand.Trim(),
                    PackageSize = AiProductMatchMatchingHelper.FormatPackageSize(row.PackageSizeValue, row.PackageSizeUnit),
                    BoundingBoxes = row.BoundingBoxes
                };

                // برای آیتم بدون تطبیق (یا تطبیق‌نشده به‌خاطر خطا)، Confidence یعنی اطمینان از خواندن نام؛ اپ فقط
                // ردیف‌های ≥ ۰.۶ را به فهرست ناموجودها می‌فرستد. برای آیتم تطبیق‌خورده، پایین‌تر مقدار تطبیق جایگزین می‌شود.
                item.Confidence = row.NameConfidence ?? 0;

                // سلول ناخوانا (قیمت/تعداد/...) هنگام استخراج از عکس — نه رد کل ردیف، فقط هشدار به فروشنده.
                if (!string.IsNullOrWhiteSpace(row.ExtractionIssue))
                    item.Issues.Add(row.ExtractionIssue);

                // PackageIndex فقط یک اشاره‌ی داخلی به packageهای واقعی همین کاندید است؛ هیچ شناسه‌ای از
                // مدل گرفته نمی‌شود. اگر مدل درباره‌ی تنوع مطمئن نباشد، ProductItemId خالی می‌ماند تا
                // فروشنده از فهرست packages انتخاب کند، نه این‌که اولین/موجودترین تنوع اشتباه انتخاب شود.
                var suggestedPackagesByProductId = new Dictionary<long, AiProductMatchPackageDto>();

                var rowRanked = ranked.FirstOrDefault(r => r.RowId == row.RowId);

                // مدل برای این ردیف نتیجه‌ای نداد (دسته‌اش شکست خورد، بودجه‌ی زمانی تمام شد، یا پاسخ ناقص بود).
                // تا قبل از این، ردیف مستقیم «نامشخص» می‌شد حتی وقتی محصولِ درست بین کاندیدهای همین‌جا بود؛
                // حالا با شباهت نام محلی (بدون هیچ فراخوانی اضافه) دست‌کم پیشنهاد به فروشنده داده می‌شود.
                var locallyRanked = false;
                if (rowRanked == null && candidates.Count > 0 && !searchFailedRowIds.Contains(row.RowId))
                {
                    var localRanking = AiProductMatchMatchingHelper.RankLocally(
                        row.RowId, row.Name, row.Brand, candidates, MinimumSuggestedMatchConfidence);
                    if (localRanking.Ranked.Count > 0)
                    {
                        rowRanked = localRanking;
                        locallyRanked = true;
                        locallyRankedRowIds.Add(row.RowId);
                    }
                }

                // «تطبیق کامل نشد» فقط وقتی که نه مدل و نه رتبه‌بندی محلی هیچ چیزی برای این ردیف نداشتند.
                var catalogMatchFailed = searchFailedRowIds.Contains(row.RowId)
                    || (candidates.Count > 0 && rowRanked == null);
                if (catalogMatchFailed)
                {
                    item.CatalogMatchFailed = true;
                    item.Issues.Add(IssueCatalogMatchIncomplete);
                    items.Add(item);
                    continue;
                }

                if (candidates.Count == 0)
                {
                    item.Issues.Add(IssueNotFoundInCatalog);
                    items.Add(item);
                    continue;
                }

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

                // رتبه‌بندی محلی هرگز انتخاب خودکار نمی‌کند: بدون تأیید مدل روی نوع حیوان/برند/سایز،
                // دو سایز مختلفِ یک محصول نمره‌ی تقریباً یکسان می‌گیرند و انتخاب خودکار یعنی موجودی غلط.
                if (best != null && !isAmbiguous && !locallyRanked && best.Confidence >= _options.AutoSelectConfidenceThreshold)
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

            // یک خط خلاصه برای هر ردیف تا علت «تطبیق نخورد» از لاگ سرور معلوم باشد (بدون کاندید / مدل ردش کرد / ناموفق / مبهم).
            foreach (var item in items)
            {
                var rowCandidates = candidatesByRow.TryGetValue(item.RowId, out var list) ? list.Count : 0;
                var rankedByModel = ranked.FirstOrDefault(r => r.RowId == item.RowId)?.Ranked.Count ?? 0;
                _logger.LogInformation(
                    "AiProductMatch store {StoreId} row {RowId}: candidates={Candidates} rankedByModel={Ranked} modelBatchFailed={BatchFailed} localFallback={Local} matches={Matches} autoSelected={Auto} catalogMatchFailed={Failed} issues={Issues}",
                    storeId, item.RowId, rowCandidates, rankedByModel, failedMatchRowIds.Contains(item.RowId), locallyRankedRowIds.Contains(item.RowId),
                    item.Matches.Count, item.ProductId.HasValue, item.CatalogMatchFailed, string.Join(" | ", item.Issues));
            }

            return new BaseResultDto<AiProductMatchAnalyzeResultDto>(true, new AiProductMatchAnalyzeResultDto { Items = items });
        }

        // اسکرین‌شات‌های جدول نرم‌افزار انبار را در دسته‌های چندتایی (نه یکی‌یکی مثل قفسه) به مدل
        // می‌فرستد: با تا ۴۰ صفحه، ۴۰ فراخوانی موازی جدا هم هزینه‌ی تکرار system prompt را ۴۰ برابر
        // می‌کند و هم ریسک Rate-Limit سمت Provider را بالا می‌برد. SourcePictureId عمداً خالی می‌ماند
        // چون هر دسته چند تصویر دارد و نمی‌شود یک ردیف را با قطعیت به یکی از آن‌ها نسبت داد — مستند
        // فرانت هم این فیلد را برای veterinary/sepidar لازم نداشت.
        private async Task<(List<AiProductMatchWorkingRow> Rows, int Failures)> ExtractTableCaptureRowsAsync(
            List<BufferedImage> bufferedImages,
            long storeId,
            CancellationToken budgetToken,
            CancellationToken callerToken,
            Action<int, int> onBatchProgress = null)
        {
            var batchSize = Math.Max(1, _options.TableImagesPerVisionCall);
            var batches = bufferedImages
                .Select((image, index) => (image, index))
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.image).ToList())
                .ToList();

            var completedBatches = 0;
            var failedBatches = 0;
            var totalBatches = batches.Count;

            var extractionTasks = batches.Select(async (batch, batchIndex) =>
            {
                try
                {
                    var images = batch
                        .Select(image => (MimeType: string.IsNullOrWhiteSpace(image.ContentType) ? "image/png" : image.ContentType, image.Bytes))
                        .ToList();

                    var extraction = await CallModelAsync(
                        AiProductMatchPrompts.TableCaptureExtractionSystemInstruction,
                        AiProductMatchPrompts.TableCaptureExtractionUserText,
                        images,
                        budgetToken,
                        callerToken);

                    var rowsForBatch = new List<AiProductMatchWorkingRow>();
                    if (!extraction.IsSuccess)
                    {
                        _logger.LogWarning("AiProductMatch table-capture extraction failed for batch {BatchIndex}, store {StoreId}: {Error}", batchIndex + 1, storeId, extraction.ErrorCode);
                        Interlocked.Increment(ref failedBatches);
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
            return (result, failedBatches);
        }

        private static async Task<List<BufferedImage>> BufferImagesAsync(List<IFormFile> source, CancellationToken cancellationToken)
        {
            var result = new List<BufferedImage>();
            foreach (var image in source)
            {
                await using var stream = image.OpenReadStream();
                using var memory = new System.IO.MemoryStream();
                await stream.CopyToAsync(memory, cancellationToken);
                result.Add(new BufferedImage(memory.ToArray(), image.ContentType, image.FileName, image.Name));
            }
            return result;
        }

        // هر آپلود روی FormFileِ حافظه‌ایِ مستقل خودش انجام می‌شود؛ شکست آپلود هرگز تحلیل را رد نمی‌کند.
        private async Task<long?[]> UploadImagesAsync(List<BufferedImage> images, long storeId, string authorizationHeaderValue)
        {
            var uploads = images.Select(async image =>
            {
                try
                {
                    var file = new FormFile(new System.IO.MemoryStream(image.Bytes), 0, image.Bytes.Length, image.Name, image.FileName)
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = image.ContentType
                    };
                    var (ok, pictureId, error) = await _fileClient.UploadAsync(file, authorizationHeaderValue, CancellationToken.None);
                    if (!ok)
                        _logger.LogWarning("AiProductMatch failed to persist an uploaded image for store {StoreId}: {Error}", storeId, error);
                    return ok ? pictureId : null;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "AiProductMatch image upload threw for store {StoreId}.", storeId);
                    return (long?)null;
                }
            });
            return await Task.WhenAll(uploads);
        }

        // تمام‌شدن بودجهٔ زمانی (budgetToken) را به شکست عادیِ Provider تبدیل می‌کند؛ فقط قطع‌شدن واقعیِ کلاینت
        // (callerToken) Exception می‌دهد.
        private async Task<AiProductMatchGeminiCallResult> CallModelAsync(
            string systemInstruction,
            string userText,
            IReadOnlyList<(string MimeType, byte[] Bytes)> images,
            CancellationToken budgetToken,
            CancellationToken callerToken)
        {
            try
            {
                return await _geminiClient.GenerateJsonAsync(systemInstruction, userText, images, budgetToken);
            }
            catch (OperationCanceledException) when (!callerToken.IsCancellationRequested)
            {
                return AiProductMatchGeminiCallResult.Failure("budget_exhausted");
            }
        }

        // موازی‌سازی جست‌وجوی کاندید برای همه‌ی ردیف‌ها هم‌زمان، در دو فاز:
        // فاز ۱) هر ردیف یک Query مستقل روی Dapper/SqlConnection خودش می‌زند (Thread-safe، چون هرکدام
        //        Connection جدای خودش را باز می‌کند) — قبلاً این حلقه پشت‌سرهم (Sequential) اجرا می‌شد و با
        //        چند ردیف (مثلاً ۷ آیتم تشخیص‌داده‌شده از عکس)، مجموع Timeout ها به‌سرعت جمع می‌شد.
        // فاز ۲) هیدریت (Products/ProductItems) روی _context یک‌بار و برای اجتماع همه‌ی شناسه‌ها انجام
        //        می‌شود — چون DbContext را نمی‌توان هم‌زمان از چند Task صدا زد (Thread-unsafe)، و این کار
        //        تعداد Round-trip های EF را هم از ۲×تعداد‌ردیف به فقط ۲ کاهش می‌دهد.
        private async Task<(Dictionary<string, List<AiProductMatchCandidateProduct>> CandidatesByRow, HashSet<string> SearchFailedRowIds)> FindCandidatesForAllRowsAsync(
            List<AiProductMatchWorkingRow> rows,
            long storeId,
            CancellationToken cancellationToken)
        {
            var searchTasks = rows.Select(async row =>
            {
                var (productIds, failed) = await SearchProductIdsAsync(row.Name, storeId, cancellationToken);
                return new { row.RowId, ProductIds = productIds, Failed = failed };
            });
            var searchResults = await Task.WhenAll(searchTasks);

            var allProductIds = searchResults.SelectMany(r => r.ProductIds).Distinct().ToList();
            var (productById, itemsByProductId) = await LoadCandidateProductsAsync(allProductIds, cancellationToken);

            var candidatesByRow = new Dictionary<string, List<AiProductMatchCandidateProduct>>();
            foreach (var searchResult in searchResults)
                candidatesByRow[searchResult.RowId] = BuildCandidates(searchResult.ProductIds, productById, itemsByProductId, storeId);

            var failedRowIds = searchResults.Where(r => r.Failed).Select(r => r.RowId).ToHashSet();
            return (candidatesByRow, failedRowIds);
        }

        // Failed=true یعنی جست‌وجو خطا داد (نه این‌که نتیجه‌ای نبوده) — این دو حالت برای اپ کاملاً فرق دارند.
        private async Task<(List<long> ProductIds, bool Failed)> SearchProductIdsAsync(string rawName, long storeId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(rawName))
                return (new List<long>(), false);

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
                // عمداً خاموش (بر خلاف جست‌وجوی مشتری): fuzzy هر توکن ۴+ حرفی را به دنباله‌ای از
                // دوحرفی‌ها («ال»، «رو»، «کن») هم تبدیل می‌کند. برای یک عبارت تک‌کلمه‌ایِ تایپ‌شده مفید
                // است، ولی ورودی اینجا یک نام کامل ۸-۱۲ کلمه‌ای از OCR است: آن دوحرفی‌ها هم سهمیه‌ی
                // ۲۰تایی SearchTerms را از کلمات واقعی می‌گیرند، هم LIKE '%..%' را روی تقریباً کل
                // Products صادق می‌کنند (Scan سنگین‌تر + ریسک commandTimeout=8s) و هم با امتیاز
                // آشغال، کاندید درست را از Top N بیرون می‌اندازند.
                EnableFuzzy = false
            };
            // همان دو خط دقیق SearchService.SearchAsync قبل از فراخوانی SearchMinAsync
            request.Q = SearchNormalizeHelper.Normalize(request.Q);
            request.ClampCounts();
            request.SearchTerms = SearchNormalizeHelper.BuildTerms(request.Q, request.EnableFuzzy);

            if (string.IsNullOrWhiteSpace(request.Q) || request.Q.Length < 2)
                return (new List<long>(), false);

            try
            {
                // عمداً SearchMinAsync نیست: اون متد برای جستجوی مشتری طراحی شده و فقط محصولاتی که
                // حداقل یک فروشگاه فعال با موجودی>۰ دارند برمی‌گردونه. اینجا دقیقاً برعکسش لازمه —
                // محصولاتی که فروشنده‌ی فعلی (یا هیچ فروشگاهی) هنوز براشون موجودی ثبت نکرده، چون
                // کل هدف این فیچر همینه: پیدا کردن محصول کاتالوگ برای ساختن اولین ProductItem آن.
                // پیش‌نویسِ (تأییدنشده‌ی) فروشگاه‌های دیگر به این فروشنده پیشنهاد داده نمی‌شود؛ بقیه‌ی وضعیت‌ها (موجود/ناموجود/…) بله.
                var found = await _productService.SearchCatalogProductIdsAsync(request, cancellationToken, storeId);
                // SearchCatalogProductIdsAsync عمداً بیش‌ازحد می‌آورد (تا ۶۰)؛ فقط بهترین‌ها (به ترتیب امتیاز) به مدل می‌روند.
                return (found == null ? new List<long>() : found.Distinct().Take(Math.Max(1, _options.CandidateShortlistSize)).ToList(), false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AiProductMatch candidate search failed for raw name '{RawName}'.", rawName);
                return (new List<long>(), true);
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
