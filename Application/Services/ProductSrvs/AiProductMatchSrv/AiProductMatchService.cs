using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.AiProductMatchSrv.Dto;
using Application.Services.ProductSrvs.AiProductMatchSrv.Iface;
using Application.Services.ProductSrvs.ProductSrv.Dto;
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
        private readonly AiProductMatchOptions _options;
        private readonly ILogger<AiProductMatchService> _logger;

        public AiProductMatchService(
            IDataBaseContext context,
            IProductService productService,
            IAiProductMatchGeminiClient geminiClient,
            IOptions<AiProductMatchOptions> options,
            ILogger<AiProductMatchService> logger)
        {
            _context = context;
            _productService = productService;
            _geminiClient = geminiClient;
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

            // بایت‌ها یک‌بار به حافظه خوانده می‌شود (استریم فرم HTTP برای خواندن موازی امن نیست) و بعد از
            // پایان تحلیل دور ریخته می‌شود: عکس قفسه فقط ورودی تحلیل است و هیچ‌جا ذخیره نمی‌شود.
            var bufferedImages = hasImages ? await BufferImagesAsync(dto.Images, cancellationToken) : new List<BufferedImage>();

            // زمان‌بندی مرحله‌به‌مرحله: وقتی همه‌ی ردیف‌ها CatalogMatchFailed می‌شوند، تنها راه تشخیص
            // این‌که کدام مرحله بودجه را خورده، همین است (بدون دسترسی به Provider/دیتابیس از بیرون).
            var stageWatch = System.Diagnostics.Stopwatch.StartNew();
            var extractionMs = 0L;
            var searchMs = 0L;
            var matchMs = 0L;

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
                                BoundingBoxes = AiProductMatchMatchingHelper.NormalizeBoxes(extracted.Boxes)
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
            extractionMs = stageWatch.ElapsedMilliseconds;

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

            searchMs = stageWatch.ElapsedMilliseconds - extractionMs;

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

            matchMs = stageWatch.ElapsedMilliseconds - extractionMs - searchMs;

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
            _logger.LogInformation(
                "AiProductMatch store {StoreId} timing: extraction={ExtractionMs}ms search={SearchMs}ms match={MatchMs}ms budget={BudgetSec}s rows={Rows} searchFailed={SearchFailed} matchBatchesFailed={MatchFailed}",
                storeId, extractionMs, searchMs, matchMs, _options.TotalBudgetSeconds, workingRows.Count, searchFailedRowIds.Count, failedMatchRowIds.Count);

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
        // می‌کند و هم ریسک Rate-Limit سمت Provider را بالا می‌برد.
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

        // یک کوئری برای همهٔ ردیف‌ها، نه یکی به‌ازای هر ردیف.
        //
        // قبلاً هر ردیفِ تشخیص‌داده‌شده از عکس Query جدای خودش را موازی می‌زد. چون Name/SecondName/
        // ProductLabel از نوع nvarchar(max) اند، هر Query یک اسکن کاملِ رشته‌ای روی Products است؛ با ۶
        // محصول در یک قفسه یعنی ۶ اسکن هم‌زمان که روی سرور واقعی هر شش‌تا به commandTimeout می‌خوردند
        // («Execution Timeout Expired») ⇒ صفر کاندید ⇒ همهٔ ردیف‌ها CatalogMatchFailed («نامشخص»).
        //
        // حالا کلیدواژه‌های همهٔ ردیف‌ها یکی می‌شوند و یک فهرست کوتاه مشترک گرفته می‌شود؛ تخصیص کاندید
        // به هر ردیف در حافظه و با همان شباهت نامی انجام می‌شود که مرحلهٔ تطبیق هم از آن استفاده می‌کند.
        // ردیف‌های یک قفسه کلمات مشترک زیادی دارند، پس تعداد کلیدواژهٔ یکتا تقریباً ثابت می‌ماند و
        // هزینهٔ دیتابیس عملاً به اندازهٔ «یک ردیف» می‌رسد، نه N ردیف.
        private async Task<(Dictionary<string, List<AiProductMatchCandidateProduct>> CandidatesByRow, HashSet<string> SearchFailedRowIds)> FindCandidatesForAllRowsAsync(
            List<AiProductMatchWorkingRow> rows,
            long storeId,
            CancellationToken cancellationToken)
        {
            var candidatesByRow = rows.ToDictionary(row => row.RowId, _ => new List<AiProductMatchCandidateProduct>());

            var searchableRows = rows.Where(row => !string.IsNullOrWhiteSpace(row.Name)).ToList();
            var terms = BuildSearchTerms(searchableRows);
            if (terms.Count == 0)
                return (candidatesByRow, new HashSet<string>());

            List<CatalogShortlistItemDto> shortlist;
            try
            {
                // فهرست کوتاه سخاوتمندانه‌تر از سهمیهٔ یک ردیف گرفته می‌شود، چون بین همهٔ ردیف‌ها مشترک است.
                var shortlistSize = Math.Clamp(searchableRows.Count * _options.CandidateShortlistSize, 60, 400);
                shortlist = await _productService.SearchCatalogShortlistAsync(terms, shortlistSize, cancellationToken, storeId)
                            ?? new List<CatalogShortlistItemDto>();
            }
            catch (Exception ex)
            {
                // خطا (نه «نتیجه‌ای نبود») — این دو حالت برای اپ کاملاً فرق دارند: اولی «تلاش دوباره»،
                // دومی «در کاتالوگ نیست». پیام خطا داخل همین خط است تا grep AiProductMatch کافی باشد.
                _logger.LogError(ex, "AiProductMatch catalog shortlist failed for store {StoreId} ({Terms} terms, {Rows} rows): {ErrorType}: {ErrorMessage}",
                    storeId, terms.Count, searchableRows.Count, ex.GetType().Name, ex.Message);
                return (candidatesByRow, rows.Select(row => row.RowId).ToHashSet());
            }

            if (shortlist.Count == 0)
                return (candidatesByRow, new HashSet<string>());

            // هر ردیف بهترین‌های همین فهرست مشترک را برمی‌دارد؛ شباهت صفر یعنی هیچ کلمهٔ مشترکی نیست.
            var idsByRow = new Dictionary<string, List<long>>();
            foreach (var row in searchableRows)
            {
                idsByRow[row.RowId] = shortlist
                    .Select(item => new
                    {
                        item.Id,
                        Similarity = AiProductMatchMatchingHelper.ComputeNameSimilarity(row.Name, item.Name, item.BrandName)
                    })
                    .Where(scored => scored.Similarity > 0)
                    .OrderByDescending(scored => scored.Similarity)
                    .Take(Math.Max(1, _options.CandidateShortlistSize))
                    .Select(scored => scored.Id)
                    .ToList();
            }

            // هیدریت (Products/ProductItems) یک‌بار برای اجتماع همهٔ شناسه‌ها: DbContext هم‌زمان‌پذیر
            // نیست و این کار تعداد Round-trip های EF را هم به ۲ می‌رساند.
            var allProductIds = idsByRow.Values.SelectMany(ids => ids).Distinct().ToList();
            var (productById, itemsByProductId) = await LoadCandidateProductsAsync(allProductIds, cancellationToken);

            foreach (var pair in idsByRow)
                candidatesByRow[pair.Key] = BuildCandidates(pair.Value, productById, itemsByProductId, storeId);

            return (candidatesByRow, new HashSet<string>());
        }

        // همان نرمال‌سازی/ساخت واژه‌های جست‌وجوی مشتری، ولی برای همهٔ ردیف‌ها یکجا و بدون fuzzy:
        // fuzzy هر توکن ۴+ حرفی را به دوحرفی‌ها («ال»، «رو»، «کن») هم می‌شکند که برای یک عبارت
        // تک‌کلمه‌ای تایپ‌شده مفید است، ولی ورودی اینجا نام کامل ۸-۱۲ کلمه‌ای از OCR است: آن دوحرفی‌ها
        // LIKE '%..%' را روی تقریباً کل Products صادق می‌کنند و فقط اسکن را سنگین‌تر می‌کنند.
        private static List<string> BuildSearchTerms(IEnumerable<AiProductMatchWorkingRow> rows)
        {
            var terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows)
            {
                var normalized = SearchNormalizeHelper.Normalize(row.Name);
                if (string.IsNullOrWhiteSpace(normalized) || normalized.Length < 2)
                    continue;

                // فقط خودِ کلمه‌ها؛ نه عبارت کامل و نه نسخهٔ بدون‌فاصله (هیچ‌کدام در LIKE کاتالوگ جور نمی‌شوند).
                foreach (var token in normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    if (token.Length >= 2)
                        terms.Add(token);
            }
            return terms.ToList();
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
