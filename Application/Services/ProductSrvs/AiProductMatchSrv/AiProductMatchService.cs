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
            CancellationToken cancellationToken)
        {
            if (!_options.Enabled || !_geminiClient.IsAvailable(out _))
                return Fail(Resource.Notification.AiProductMatchServiceUnavailable, 6);

            var sourceType = dto.SourceType?.Trim().ToLowerInvariant();
            var validSourceTypes = new[] { "shelf", "excel", "sepidar", "veterinary" };
            if (string.IsNullOrWhiteSpace(sourceType) || !validSourceTypes.Contains(sourceType))
                return Fail(Resource.Notification.AiProductMatchInvalidSourceType, 1);

            var hasImages = dto.Images != null && dto.Images.Count > 0;
            var hasRowsJson = !string.IsNullOrWhiteSpace(dto.RowsJson);
            if (!hasImages && !hasRowsJson)
                return Fail(Resource.Notification.AiProductMatchInvalidInput, 1);

            if (hasImages && dto.Images.Count > _options.MaxImagesPerRequest)
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
            var uploadedImages = new List<(IFormFile Image, long? PictureId)>();
            if (hasImages)
            {
                foreach (var image in dto.Images)
                {
                    var (ok, pictureId, error) = await _fileClient.UploadAsync(image, authorizationHeaderValue, cancellationToken);
                    if (!ok)
                        _logger.LogWarning("AiProductMatch failed to persist an uploaded image for store {StoreId}: {Error}", storeId, error);
                    uploadedImages.Add((image, ok ? pictureId : null));
                }
            }

            var workingRows = new List<AiProductMatchWorkingRow>();

            if (sourceType == "shelf")
            {
                var imageIndex = 0;
                foreach (var (image, pictureId) in uploadedImages)
                {
                    imageIndex++;
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

                    if (!extraction.IsSuccess)
                    {
                        _logger.LogWarning("AiProductMatch shelf extraction failed for image {ImageIndex}, store {StoreId}: {Error}", imageIndex, storeId, extraction.ErrorCode);
                        continue;
                    }

                    var extractedRows = AiProductMatchGeminiResponseParser.ParseShelfExtraction(extraction.RawJson);
                    var itemIndex = 0;
                    foreach (var extracted in extractedRows)
                    {
                        itemIndex++;
                        workingRows.Add(new AiProductMatchWorkingRow
                        {
                            RowId = $"shelf-{imageIndex}-{itemIndex}",
                            Name = extracted.DetectedName,
                            Price = extracted.PriceGuess,
                            Quantity = extracted.QuantityGuess,
                            Unit = extracted.Unit,
                            SourcePictureId = pictureId
                        });
                    }
                }

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

            var candidatesByRow = new Dictionary<string, List<AiProductMatchCandidateProduct>>();
            foreach (var row in workingRows)
                candidatesByRow[row.RowId] = await FindCandidatesAsync(row.Name, storeId, cancellationToken);

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

                if (candidates.Count == 0)
                {
                    item.Issues.Add(IssueNotFoundInCatalog);
                    items.Add(item);
                    continue;
                }

                var rowRanked = ranked.FirstOrDefault(r => r.RowId == row.RowId);
                if (rowRanked != null)
                {
                    foreach (var rankedCandidate in rowRanked.Ranked)
                    {
                        // محافظت در برابر Hallucination: هر اندیسی خارج از بازه‌ی واقعی کاندیدها نادیده گرفته می‌شود
                        if (rankedCandidate.Index < 0 || rankedCandidate.Index >= candidates.Count)
                            continue;

                        var candidate = candidates[rankedCandidate.Index];
                        item.Matches.Add(new AiProductMatchCandidateDto
                        {
                            ProductId = candidate.ProductId,
                            Name = candidate.Name,
                            Confidence = rankedCandidate.Confidence,
                            ProductItems = candidate.Packages
                        });
                    }
                }

                var best = item.Matches.OrderByDescending(match => match.Confidence).FirstOrDefault();
                if (best != null && best.Confidence >= _options.AutoSelectConfidenceThreshold)
                {
                    item.ProductId = best.ProductId;
                    item.ProductName = best.Name;
                    item.Confidence = best.Confidence;
                    var bestPackage = best.ProductItems.FirstOrDefault(p => p.ExistsForCurrentStore) ?? best.ProductItems.FirstOrDefault();
                    item.ProductItemId = bestPackage?.ProductItemId;
                }
                else
                {
                    item.Issues.Add(IssueNoAutoMatch);
                }

                items.Add(item);
            }

            return new BaseResultDto<AiProductMatchAnalyzeResultDto>(true, new AiProductMatchAnalyzeResultDto { Items = items });
        }

        private async Task<List<AiProductMatchCandidateProduct>> FindCandidatesAsync(string rawName, long storeId, CancellationToken cancellationToken)
        {
            var result = new List<AiProductMatchCandidateProduct>();
            if (string.IsNullOrWhiteSpace(rawName))
                return result;

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
                return result;

            List<Application.Services.ProductSrvs.ProductSrv.Dto.SearchProductDto> found;
            try
            {
                found = await _productService.SearchMinAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AiProductMatch candidate search failed for raw name '{RawName}'.", rawName);
                return result;
            }

            if (found == null || found.Count == 0)
                return result;

            var productIds = found.Select(f => f.Id).Distinct().ToList();

            var products = await _context.Products
                .Include(p => p.Brand)
                .Where(p => productIds.Contains(p.Id) && p.Active && !p.Deleted)
                .ToListAsync(cancellationToken);

            if (products.Count == 0)
                return result;

            var productById = products.ToDictionary(p => p.Id);

            var allItems = await _context.ProductItems
                .Include(pi => pi.VarietyItem)
                .Include(pi => pi.VarietyItem2)
                .Where(pi => productIds.Contains(pi.ProductId) && !pi.Deleted)
                .ToListAsync(cancellationToken);

            foreach (var match in found)
            {
                if (!productById.TryGetValue(match.Id, out var product))
                    continue;

                var itemsForProduct = allItems.Where(pi => pi.ProductId == product.Id).ToList();
                result.Add(new AiProductMatchCandidateProduct
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    BrandName = product.Brand?.Name,
                    CodeValue = product.CodeValue,
                    Packages = BuildPackages(itemsForProduct, storeId)
                });
            }

            return result;
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
