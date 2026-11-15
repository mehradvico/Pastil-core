using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Services.ProductSrvs.AiProductMatchSrv.Iface;
using Application.Services.ProductSrvs.ProductImageSuggestionSrv.Dto;
using Application.Services.ProductSrvs.ProductImageSuggestionSrv.Iface;
using Application.Services.Setting.NoticeSrv;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.ProductImageSuggestionSrv
{
    /// <summary>
    /// تصویر پیشنهادیِ فروشگاه برای محصول کاتالوگ. فروشنده می‌فرستد → اعلان ادمین → ادمین تأیید/رد می‌کند.
    /// تأیید: Product.PictureId همان تصویر می‌شود؛ اگر محصول از قبل تصویر داشت فقط با ConfirmReplace=true.
    /// </summary>
    public class ProductImageSuggestionService : IProductImageSuggestionService
    {
        // کد پاسخِ «محصول از قبل تصویر دارد، تأیید جایگزینی لازم است» (پنل با همین کد مودال را نشان می‌دهد)
        public const int CodeProductHasPicture = 9;

        private const long MaxImageBytes = 8 * 1024 * 1024;
        private const int MaxReasonLength = 500;
        private const int MaxPendingPerStore = 300;

        private readonly IDataBaseContext _context;
        private readonly IAiProductMatchFileClient _fileClient;
        private readonly INoticeService _noticeService;
        private readonly ILogger<ProductImageSuggestionService> _logger;

        public ProductImageSuggestionService(
            IDataBaseContext context,
            IAiProductMatchFileClient fileClient,
            INoticeService noticeService,
            ILogger<ProductImageSuggestionService> logger)
        {
            _context = context;
            _fileClient = fileClient;
            _noticeService = noticeService;
            _logger = logger;
        }

        // ================= فروشنده =================

        public async Task<BaseResultDto<ProductImageSuggestionDto>> SubmitAsync(
            long storeId, long? actorUserId, long productId, IFormFile image, string authorizationHeaderValue, CancellationToken cancellationToken)
        {
            if (storeId <= 0 || productId <= 0 || image == null)
                return Fail<ProductImageSuggestionDto>(Resource.Notification.InvalidData);

            var product = await _context.Products.AsNoTracking()
                .Where(x => x.Id == productId && x.Active && !x.Deleted)
                .Select(x => new { x.Id, x.Name })
                .FirstOrDefaultAsync(cancellationToken);
            if (product == null)
                return Fail<ProductImageSuggestionDto>(Resource.Notification.ProductImageSuggestionProductMissing, 3);

            // فقط فروشگاهی که این محصول را (با قیمت/موجودی) دارد می‌تواند برایش تصویر پیشنهاد دهد
            var storeSellsProduct = await _context.ProductItems.AsNoTracking()
                .AnyAsync(x => x.ProductId == productId && x.StoreId == storeId && !x.Deleted, cancellationToken);
            if (!storeSellsProduct)
                return Fail<ProductImageSuggestionDto>(Resource.Notification.ProductImageSuggestionNotInStore);

            if (!await ImageContentValidator.IsAcceptableAsync(image, MaxImageBytes, cancellationToken))
                return Fail<ProductImageSuggestionDto>(Resource.Notification.ProductImageSuggestionPictureRejected);

            var pending = await _context.ProductImageSuggestions.AsTracking()
                .Where(x => x.StoreId == storeId && x.ProductId == productId && x.Status == ProductImageSuggestionStatus.Pending)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (pending == null &&
                await _context.ProductImageSuggestions.CountAsync(x => x.StoreId == storeId && x.Status == ProductImageSuggestionStatus.Pending, cancellationToken) >= MaxPendingPerStore)
                return Fail<ProductImageSuggestionDto>(Resource.Notification.ProductImageSuggestionTooMany);

            var (ok, pictureId, error) = await _fileClient.UploadAsync(image, authorizationHeaderValue, cancellationToken);
            if (!ok || pictureId == null)
            {
                _logger.LogWarning("ProductImageSuggestion upload failed for store {StoreId}, product {ProductId}: {Error}", storeId, productId, error);
                return Fail<ProductImageSuggestionDto>(Resource.Notification.ProductImageSuggestionUploadFailed, 5);
            }

            var now = DateTime.UtcNow;
            var isNew = pending == null;
            if (isNew)
            {
                pending = new ProductImageSuggestion
                {
                    ProductId = productId,
                    StoreId = storeId,
                    Status = ProductImageSuggestionStatus.Pending
                };
                await _context.ProductImageSuggestions.AddAsync(pending, cancellationToken);
            }

            // فروشنده هنوز منتظر بررسی است و عکس بهتری فرستاد: همان ردیف عوض می‌شود، صف ادمین شلوغ نمی‌شود
            pending.PictureId = pictureId.Value;
            pending.CreateDate = now;
            await _context.SaveChangesAsync(cancellationToken);

            if (isNew)
                await NotifyAdminAsync(pending, product.Name, actorUserId);

            var saved = await _context.ProductImageSuggestions.AsNoTracking()
                .Include(x => x.Picture)
                .FirstAsync(x => x.Id == pending.Id, cancellationToken);
            return new BaseResultDto<ProductImageSuggestionDto>(true, ToDto(saved, product.Name));
        }

        public async Task<BaseResultDto<ProductImageSuggestionListDto>> ListAsync(long storeId, long? productId, int pageSize)
        {
            pageSize = Math.Clamp(pageSize <= 0 ? 100 : pageSize, 1, 200);

            var query = _context.ProductImageSuggestions.AsNoTracking().Where(x => x.StoreId == storeId);
            if (productId.HasValue)
                query = query.Where(x => x.ProductId == productId.Value);

            var total = await query.CountAsync();
            var rows = await query
                .Include(x => x.Picture)
                .Include(x => x.Product)
                .OrderByDescending(x => x.Id)
                .Take(pageSize)
                .ToListAsync();

            return new BaseResultDto<ProductImageSuggestionListDto>(true, new ProductImageSuggestionListDto
            {
                Items = rows.Select(x => ToDto(x, x.Product?.Name)).ToList(),
                Total = total
            });
        }

        public async Task<BaseResultDto> DeleteAsync(long storeId, long id)
        {
            var entity = storeId <= 0 || id <= 0
                ? null
                : await _context.ProductImageSuggestions.AsTracking().FirstOrDefaultAsync(x => x.Id == id && x.StoreId == storeId);

            // پیشنهاد فروشندهٔ دیگر هم دقیقاً همین پاسخ را می‌گیرد تا وجودش لو نرود
            if (entity == null)
                return new BaseResultDto(false, Resource.Notification.ProductImageSuggestionNotFound, 3);
            if (entity.Status == ProductImageSuggestionStatus.Approved)
                return new BaseResultDto(false, Resource.Notification.ProductImageSuggestionNotDeletable, 1);

            _context.ProductImageSuggestions.Remove(entity);
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        // ================= ادمین =================

        public async Task<BaseResultDto<ProductImageSuggestionAdminListDto>> SearchAsync(ProductImageSuggestionAdminSearchDto dto)
        {
            dto ??= new ProductImageSuggestionAdminSearchDto();
            var pageNumber = Math.Max(1, dto.PageNumber);
            var pageSize = Math.Clamp(dto.PageSize <= 0 ? 20 : dto.PageSize, 1, 100);

            var statusText = string.IsNullOrWhiteSpace(dto.Status) ? "pending" : dto.Status.Trim().ToLowerInvariant();
            var query = _context.ProductImageSuggestions.AsNoTracking().AsQueryable();
            ProductImageSuggestionStatus? status = null;
            if (statusText != "all")
            {
                if (!Enum.TryParse<ProductImageSuggestionStatus>(statusText, ignoreCase: true, out var parsed) || !Enum.IsDefined(parsed))
                    return Fail<ProductImageSuggestionAdminListDto>(Resource.Notification.InvalidData);
                status = parsed;
            }
            if (status.HasValue)
                query = query.Where(x => x.Status == status.Value);

            if (dto.StoreId.HasValue)
                query = query.Where(x => x.StoreId == dto.StoreId.Value);
            if (dto.ProductId.HasValue)
                query = query.Where(x => x.ProductId == dto.ProductId.Value);

            var q = dto.Q?.Trim();
            if (!string.IsNullOrEmpty(q))
                query = query.Where(x => x.Product.Name.Contains(q) || x.Store.Name.Contains(q));

            var total = await query.CountAsync();

            // صف بررسی: قدیمی‌ترین پیشنهاد اول؛ بقیهٔ حالت‌ها جدیدترین اول
            var ordered = status == ProductImageSuggestionStatus.Pending
                ? query.OrderBy(x => x.CreateDate).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.Id);

            var rows = await ordered
                .Include(x => x.Picture)
                .Include(x => x.Store)
                .Include(x => x.Product).ThenInclude(x => x.Picture)
                .Include(x => x.Product).ThenInclude(x => x.Brand)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var replacedIds = rows.Where(x => x.ReplacedPictureId.HasValue).Select(x => x.ReplacedPictureId.Value).Distinct().ToList();
            var replaced = replacedIds.Count == 0
                ? new Dictionary<long, Picture>()
                : await _context.Pictures.AsNoTracking().Where(x => replacedIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id);

            return new BaseResultDto<ProductImageSuggestionAdminListDto>(true, new ProductImageSuggestionAdminListDto
            {
                Items = rows.Select(x => ToAdminDto(x, replaced)).ToList(),
                Total = total
            });
        }

        public async Task<BaseResultDto<ProductImageSuggestionApproveResultDto>> ApproveAsync(long adminUserId, ProductImageSuggestionApproveDto dto)
        {
            if (dto == null || dto.Id <= 0)
                return Fail<ProductImageSuggestionApproveResultDto>(Resource.Notification.InvalidData);

            var suggestion = await _context.ProductImageSuggestions.AsNoTracking()
                .Where(x => x.Id == dto.Id)
                .Select(x => new { x.Id, x.Status, x.ProductId, x.PictureId })
                .FirstOrDefaultAsync();
            if (suggestion == null)
                return Fail<ProductImageSuggestionApproveResultDto>(Resource.Notification.ProductImageSuggestionNotFound, 3);
            if (suggestion.Status != ProductImageSuggestionStatus.Pending)
                return Fail<ProductImageSuggestionApproveResultDto>(Resource.Notification.ProductImageSuggestionNotPending);

            var product = await _context.Products.AsNoTracking()
                .Where(x => x.Id == suggestion.ProductId && !x.Deleted)
                .Select(x => new { x.Id, x.PictureId })
                .FirstOrDefaultAsync();
            if (product == null)
                return Fail<ProductImageSuggestionApproveResultDto>(Resource.Notification.ProductImageSuggestionProductMissing, 3);

            var hadPicture = product.PictureId.HasValue;
            if (hadPicture && !dto.ConfirmReplace)
                return Fail<ProductImageSuggestionApproveResultDto>(Resource.Notification.ProductImageSuggestionProductHasPicture, CodeProductHasPicture);

            // رزرو اتمیک: فقط یک ادمین می‌تواند یک پیشنهاد را به «تأییدشده» ببرد
            var now = DateTime.UtcNow;
            var replacedPictureId = hadPicture ? product.PictureId : null;
            var claimed = await _context.ProductImageSuggestions
                .Where(x => x.Id == suggestion.Id && x.Status == ProductImageSuggestionStatus.Pending)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, ProductImageSuggestionStatus.Approved)
                    .SetProperty(x => x.ReviewedDate, now)
                    .SetProperty(x => x.ReviewedByUserId, adminUserId)
                    .SetProperty(x => x.ReplacedPictureId, replacedPictureId));
            if (claimed == 0)
                return Fail<ProductImageSuggestionApproveResultDto>(Resource.Notification.ProductImageSuggestionNotPending);

            // اگر تأیید جایگزینی نداریم، فقط وقتی می‌نویسیم که محصول هنوز تصویر ندارد؛ اگر بین بررسی و نوشتن
            // کسی تصویر گذاشته باشد، چیزی بی‌اجازه عوض نمی‌شود.
            var confirmReplace = dto.ConfirmReplace;
            var pictureId = suggestion.PictureId;
            var changed = await _context.Products
                .Where(x => x.Id == product.Id && !x.Deleted && (confirmReplace || x.PictureId == null))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.PictureId, (long?)pictureId)
                    .SetProperty(x => x.UpdateDate, DateTime.Now));
            if (changed == 0)
            {
                await RevertClaimAsync(suggestion.Id);
                var nowHasPicture = await _context.Products.AsNoTracking().AnyAsync(x => x.Id == product.Id && !x.Deleted && x.PictureId != null);
                return nowHasPicture
                    ? Fail<ProductImageSuggestionApproveResultDto>(Resource.Notification.ProductImageSuggestionProductHasPicture, CodeProductHasPicture)
                    : Fail<ProductImageSuggestionApproveResultDto>(Resource.Notification.ProductImageSuggestionProductMissing, 3);
            }

            return new BaseResultDto<ProductImageSuggestionApproveResultDto>(true, new ProductImageSuggestionApproveResultDto
            {
                ProductId = product.Id,
                Replaced = hadPicture
            });
        }

        public async Task<BaseResultDto<ProductImageSuggestionAdminDto>> RejectAsync(long adminUserId, ProductImageSuggestionRejectDto dto)
        {
            var reason = dto?.Reason?.Trim();
            if (dto == null || dto.Id <= 0 || string.IsNullOrWhiteSpace(reason) || reason.Length > MaxReasonLength)
                return Fail<ProductImageSuggestionAdminDto>(Resource.Notification.ProductImageSuggestionReasonRequired);

            var now = DateTime.UtcNow;
            var rejected = await _context.ProductImageSuggestions
                .Where(x => x.Id == dto.Id && x.Status == ProductImageSuggestionStatus.Pending)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, ProductImageSuggestionStatus.Rejected)
                    .SetProperty(x => x.RejectionReason, reason)
                    .SetProperty(x => x.ReviewedDate, now)
                    .SetProperty(x => x.ReviewedByUserId, adminUserId));
            if (rejected == 0)
            {
                var exists = await _context.ProductImageSuggestions.AsNoTracking().AnyAsync(x => x.Id == dto.Id);
                return Fail<ProductImageSuggestionAdminDto>(
                    exists ? Resource.Notification.ProductImageSuggestionNotPending : Resource.Notification.ProductImageSuggestionNotFound,
                    exists ? 1 : 3);
            }

            var row = await _context.ProductImageSuggestions.AsNoTracking()
                .Include(x => x.Picture).Include(x => x.Store)
                .Include(x => x.Product).ThenInclude(x => x.Picture)
                .Include(x => x.Product).ThenInclude(x => x.Brand)
                .FirstAsync(x => x.Id == dto.Id);
            return new BaseResultDto<ProductImageSuggestionAdminDto>(true, ToAdminDto(row, new Dictionary<long, Picture>()));
        }

        // ================= کمکی =================

        private async Task RevertClaimAsync(long id)
        {
            await _context.ProductImageSuggestions
                .Where(x => x.Id == id && x.Status == ProductImageSuggestionStatus.Approved)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, ProductImageSuggestionStatus.Pending)
                    .SetProperty(x => x.ReviewedDate, (DateTime?)null)
                    .SetProperty(x => x.ReviewedByUserId, (long?)null)
                    .SetProperty(x => x.ReplacedPictureId, (long?)null));
        }

        // اعلان زنگولهٔ پنل؛ خطای اعلان هیچ‌وقت ثبت پیشنهاد را خراب نمی‌کند
        private async Task NotifyAdminAsync(ProductImageSuggestion suggestion, string productName, long? actorUserId)
        {
            try
            {
                var storeName = await _context.Stores.AsNoTracking()
                    .Where(x => x.Id == suggestion.StoreId).Select(x => x.Name).FirstOrDefaultAsync();
                var result = await _noticeService.CreateAsync(new NoticeCreateDto
                {
                    Label = NoticeTypeLabels.ProductImageSuggestionSubmitted,
                    ActorUserId = actorUserId,
                    ReferenceType = "ProductImageSuggestion",
                    ReferenceId = suggestion.Id,
                    DeduplicationKey = $"{NoticeTypeLabels.ProductImageSuggestionSubmitted}:{suggestion.Id}",
                    Metadata = new Dictionary<string, string>
                    {
                        { "storeName", storeName ?? string.Empty },
                        { "productName", productName ?? string.Empty },
                        { "productId", suggestion.ProductId.ToString() }
                    }
                });
                if (result != null && !result.IsSuccess)
                    _logger.LogWarning("Admin notice for ProductImageSuggestion {Id} was not created (is the NoticeType row seeded?).", suggestion.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin notice for ProductImageSuggestion {Id} failed.", suggestion.Id);
            }
        }

        private static string PictureUrl(Picture picture)
            => picture == null ? null : $"{picture.Url}/{picture.Name}";

        private static DateTime Utc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

        private static void Fill(ProductImageSuggestionDto dto, ProductImageSuggestion entity, string productName)
        {
            dto.Id = entity.Id;
            dto.ProductId = entity.ProductId;
            dto.ProductName = productName;
            dto.PictureUrl = PictureUrl(entity.Picture);
            dto.Status = entity.Status.ToString().ToLowerInvariant();
            dto.RejectionReason = entity.Status == ProductImageSuggestionStatus.Rejected ? entity.RejectionReason : null;
            dto.CreatedAtUtc = Utc(entity.CreateDate);
            dto.ReviewedAtUtc = entity.ReviewedDate.HasValue ? Utc(entity.ReviewedDate.Value) : null;
        }

        private static ProductImageSuggestionDto ToDto(ProductImageSuggestion entity, string productName)
        {
            var dto = new ProductImageSuggestionDto();
            Fill(dto, entity, productName);
            return dto;
        }

        private static ProductImageSuggestionAdminDto ToAdminDto(ProductImageSuggestion entity, IReadOnlyDictionary<long, Picture> replaced)
        {
            var dto = new ProductImageSuggestionAdminDto
            {
                StoreId = entity.StoreId,
                StoreName = entity.Store?.Name,
                BrandName = entity.Product?.Brand?.Name,
                ProductHasPicture = entity.Product?.PictureId != null,
                ProductPictureUrl = PictureUrl(entity.Product?.Picture),
                ReplacedPictureUrl = entity.ReplacedPictureId.HasValue && replaced.TryGetValue(entity.ReplacedPictureId.Value, out var old)
                    ? PictureUrl(old)
                    : null
            };
            Fill(dto, entity, entity.Product?.Name);
            return dto;
        }

        private static BaseResultDto<T> Fail<T>(string message, int code = 1)
            => new(false, message, default, code);
    }
}
