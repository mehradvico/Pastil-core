using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.ProductSrvs.AiProductMatchSrv.Iface;
using Application.Services.ProductSrvs.MissingProductSrv.Dto;
using Application.Services.ProductSrvs.MissingProductSrv.Iface;
using Application.Services.ProductSrvs.ProductItemSrv.Dto;
using Application.Services.ProductSrvs.ProductItemSrv.Iface;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Iface;
using Entities.Entities;
using Entities.Entities.CommonField;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.MissingProductSrv
{
    public class MissingProductService : IMissingProductService
    {
        private const int MaxBatchItems = 100;
        private const int MaxNameLength = 200;
        private const int MaxShortTextLength = 100;
        private const int MaxDescriptionLength = 2000;
        private const int MaxReasonLength = 500;
        private const long MaxImageBytes = 8 * 1024 * 1024;
        private const int SimilarProductCount = 5;
        private const int MaxPicturesPerRequest = 5;

        private const string MsgNotFound = "درخواست یافت نشد.";
        private const string MsgInvalid = "اطلاعات ارسال‌شده معتبر نیست.";
        private const string MsgNotEditable = "فقط پیش‌نویس یا درخواست ردشده قابل ویرایش است.";
        private const string MsgDuplicateName = "این نام قبلاً برای فروشگاه شما ثبت شده است.";
        private const string MsgPictureRequired = "برای ارسال، تصویر محصول لازم است.";
        private const string MsgNameRequired = "نام محصول لازم است.";
        private const string MsgAlreadySubmitted = "این درخواست قبلاً ارسال شده است.";
        private const string MsgNotSubmitted = "فقط درخواست ارسال‌شده قابل بررسی است.";
        private const string MsgPictureRejected = "تصویر معتبر نیست؛ فقط jpg، png یا webp تا ۸ مگابایت.";
        private const string MsgPictureUploadFailed = "آپلود تصویر ناموفق بود؛ دوباره تلاش کنید.";
        private const string MsgTooManyPictures = "حداکثر ۵ تصویر برای هر محصول مجاز است؛ یکی را حذف کنید.";
        private const string MsgPictureNotFound = "این تصویر برای درخواست پیدا نشد.";

        private readonly IDataBaseContext _context;
        private readonly IProductService _productService;
        private readonly IProductItemService _productItemService;
        private readonly IAiProductMatchFileClient _fileClient;
        private readonly ILogger<MissingProductService> _logger;

        public MissingProductService(
            IDataBaseContext context,
            IProductService productService,
            IProductItemService productItemService,
            IAiProductMatchFileClient fileClient,
            ILogger<MissingProductService> logger)
        {
            _context = context;
            _productService = productService;
            _productItemService = productItemService;
            _fileClient = fileClient;
            _logger = logger;
        }

        // ================= فروشنده =================

        public async Task<BaseResultDto<MissingProductBatchResultDto>> CreateBatchAsync(long storeId, MissingProductBatchInputDto dto)
        {
            if (storeId <= 0 || dto?.Items == null || dto.Items.Count == 0 || dto.Items.Count > MaxBatchItems)
                return Fail<MissingProductBatchResultDto>(MsgInvalid);

            var now = DateTime.UtcNow;
            var candidates = new Dictionary<string, MissingProduct>();
            var alreadyKnown = 0;

            foreach (var item in dto.Items)
            {
                var name = item?.Name?.Trim();
                if (string.IsNullOrWhiteSpace(name) || name.Length > MaxNameLength)
                    continue;

                var key = SearchNormalizeHelper.NormalizeNoSpace(name);
                if (string.IsNullOrEmpty(key))
                    continue;

                // تکراری داخل همان درخواست هم «قبلاً شناخته‌شده» حساب می‌شود
                if (candidates.ContainsKey(key))
                {
                    alreadyKnown++;
                    continue;
                }

                candidates[key] = new MissingProduct
                {
                    StoreId = storeId,
                    Name = name,
                    NormalizedName = key,
                    Brand = Trim(item.Brand, MaxShortTextLength),
                    PackageSize = Trim(item.PackageSize, MaxShortTextLength),
                    Source = Trim(item.Source, 30) ?? "shelf",
                    Status = MissingProductStatus.Draft,
                    CreateDate = now,
                    UpdateDate = now
                };
            }

            if (candidates.Count == 0)
                return new BaseResultDto<MissingProductBatchResultDto>(true, new MissingProductBatchResultDto { Created = 0, AlreadyKnown = alreadyKnown });

            var keys = candidates.Keys.ToList();
            var existingKeys = await _context.MissingProducts
                .Where(x => x.StoreId == storeId && keys.Contains(x.NormalizedName))
                .Select(x => x.NormalizedName)
                .ToListAsync();

            var toInsert = candidates.Where(x => !existingKeys.Contains(x.Key)).Select(x => x.Value).ToList();
            alreadyKnown += candidates.Count - toInsert.Count;

            var created = 0;
            if (toInsert.Count > 0)
            {
                try
                {
                    await _context.MissingProducts.AddRangeAsync(toInsert);
                    await _context.SaveChangesAsync();
                    created = toInsert.Count;
                }
                catch (DbUpdateException ex) when (IsDuplicateKey(ex))
                {
                    // درخواست هم‌زمان دیگری بین چک و ثبت همین نام را ساخته؛ تک‌به‌تک دوباره تلاش می‌کنیم
                    foreach (var entity in toInsert)
                        _context.Entry(entity).State = EntityState.Detached;

                    foreach (var entity in toInsert)
                    {
                        try
                        {
                            entity.Id = 0;
                            await _context.MissingProducts.AddAsync(entity);
                            await _context.SaveChangesAsync();
                            created++;
                        }
                        catch (DbUpdateException inner) when (IsDuplicateKey(inner))
                        {
                            _context.Entry(entity).State = EntityState.Detached;
                            alreadyKnown++;
                        }
                    }
                }
            }

            return new BaseResultDto<MissingProductBatchResultDto>(true, new MissingProductBatchResultDto { Created = created, AlreadyKnown = alreadyKnown });
        }

        public async Task<BaseResultDto<MissingProductListDto>> ListAsync(long storeId, int pageSize)
        {
            pageSize = Math.Clamp(pageSize <= 0 ? 200 : pageSize, 1, 200);

            var query = _context.MissingProducts.Where(x => x.StoreId == storeId);
            var total = await query.CountAsync();
            var items = await query
                .Include(x => x.Picture)
                .OrderByDescending(x => x.Id)
                .Take(pageSize)
                .ToListAsync();

            return new BaseResultDto<MissingProductListDto>(true, new MissingProductListDto
            {
                Items = items.Select(ToDto).ToList(),
                Total = total
            });
        }

        public async Task<BaseResultDto<MissingProductDto>> UpdateAsync(long storeId, MissingProductUpdateDto dto)
        {
            var entity = await FindOwnAsync(storeId, dto?.Id ?? 0, includePicture: true);
            if (entity == null)
                return Fail<MissingProductDto>(MsgNotFound, 3);

            if (entity.Status != MissingProductStatus.Draft && entity.Status != MissingProductStatus.Rejected)
                return Fail<MissingProductDto>(MsgNotEditable);

            var name = dto.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                return Fail<MissingProductDto>(MsgNameRequired);
            if (name.Length > MaxNameLength
                || (dto.Brand?.Trim().Length ?? 0) > MaxShortTextLength
                || (dto.PackageSize?.Trim().Length ?? 0) > MaxShortTextLength
                || (dto.Description?.Trim().Length ?? 0) > MaxDescriptionLength
                || dto.Price < 0 || dto.Quantity < 0)
                return Fail<MissingProductDto>(MsgInvalid);

            var key = SearchNormalizeHelper.NormalizeNoSpace(name);
            if (string.IsNullOrEmpty(key))
                return Fail<MissingProductDto>(MsgNameRequired);

            if (key != entity.NormalizedName &&
                await _context.MissingProducts.AnyAsync(x => x.StoreId == storeId && x.NormalizedName == key && x.Id != entity.Id))
                return Fail<MissingProductDto>(MsgDuplicateName);

            entity.Name = name;
            entity.NormalizedName = key;
            entity.Brand = Trim(dto.Brand, MaxShortTextLength);
            entity.PackageSize = Trim(dto.PackageSize, MaxShortTextLength);
            entity.Description = Trim(dto.Description, MaxDescriptionLength);
            entity.Price = dto.Price;
            entity.Quantity = dto.Quantity;
            MarkEdited(entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsDuplicateKey(ex))
            {
                return Fail<MissingProductDto>(MsgDuplicateName);
            }

            return new BaseResultDto<MissingProductDto>(true, ToDto(entity));
        }

        // افزودن یک تصویر به فهرست (نه جایگزینی): فروشنده تا ۵ تصویر از محصول می‌گذارد و
        // ادمین با همان‌ها تصمیم می‌گیرد؛ بعد از تأیید همین‌ها ProductPictureهای محصول کاتالوگ می‌شوند.
        public async Task<BaseResultDto<MissingProductDto>> AddPictureAsync(
            long storeId, long id, IFormFile image, string authorizationHeaderValue, CancellationToken cancellationToken)
        {
            var entity = await FindOwnAsync(storeId, id, includePicture: true);
            if (entity == null)
                return Fail<MissingProductDto>(MsgNotFound, 3);

            if (entity.Status != MissingProductStatus.Draft && entity.Status != MissingProductStatus.Rejected)
                return Fail<MissingProductDto>(MsgNotEditable);

            if (entity.Pictures.Count >= MaxPicturesPerRequest)
                return Fail<MissingProductDto>(MsgTooManyPictures);

            if (!await ImageContentValidator.IsAcceptableAsync(image, MaxImageBytes, cancellationToken))
                return Fail<MissingProductDto>(MsgPictureRejected);

            var (ok, pictureId, error) = await _fileClient.UploadAsync(image, authorizationHeaderValue, cancellationToken);
            if (!ok || pictureId == null)
            {
                _logger.LogWarning("MissingProduct picture upload failed for store {StoreId}, item {Id}: {Error}", storeId, id, error);
                return Fail<MissingProductDto>(MsgPictureUploadFailed, 5);
            }

            entity.Pictures.Add(new MissingProductPicture
            {
                MissingProductId = entity.Id,
                PictureId = pictureId.Value,
                SortOrder = entity.Pictures.Count == 0 ? 0 : entity.Pictures.Max(x => x.SortOrder) + 1
            });
            SyncCover(entity);
            MarkEdited(entity);
            await _context.SaveChangesAsync();

            return new BaseResultDto<MissingProductDto>(true, ToDto(await FindOwnAsync(storeId, id, includePicture: true)));
        }

        public async Task<BaseResultDto<MissingProductDto>> RemovePictureAsync(long storeId, long id, long pictureId)
        {
            var entity = await FindOwnAsync(storeId, id, includePicture: true);
            if (entity == null)
                return Fail<MissingProductDto>(MsgNotFound, 3);

            if (entity.Status != MissingProductStatus.Draft && entity.Status != MissingProductStatus.Rejected)
                return Fail<MissingProductDto>(MsgNotEditable);

            var row = entity.Pictures.FirstOrDefault(x => x.PictureId == pictureId);
            if (row == null)
                return Fail<MissingProductDto>(MsgPictureNotFound, 3);

            entity.Pictures.Remove(row);
            _context.MissingProductPictures.Remove(row);
            SyncCover(entity);
            MarkEdited(entity);
            await _context.SaveChangesAsync();

            return new BaseResultDto<MissingProductDto>(true, ToDto(await FindOwnAsync(storeId, id, includePicture: true)));
        }

        // PictureId روی خود رکورد فقط «کاور» است و همیشه اولین تصویر فهرست؛ نگه داشتنش یعنی همهٔ
        // کوئری‌ها/ایندکس‌ها/شرط‌های موجود (از جمله «تصویر دارد یا نه») بدون تغییر کار می‌کنند.
        private static void SyncCover(MissingProduct entity)
            => entity.PictureId = entity.Pictures.OrderBy(x => x.SortOrder).ThenBy(x => x.Id).FirstOrDefault()?.PictureId;

        public async Task<BaseResultDto<MissingProductDto>> SubmitAsync(long storeId, long id)
        {
            var entity = await FindOwnAsync(storeId, id, includePicture: true);
            if (entity == null)
                return Fail<MissingProductDto>(MsgNotFound, 3);

            if (entity.Status == MissingProductStatus.Submitted)
                return Fail<MissingProductDto>(MsgAlreadySubmitted);
            if (entity.Status != MissingProductStatus.Draft)
                return Fail<MissingProductDto>(MsgNotEditable);

            if (string.IsNullOrWhiteSpace(entity.Name))
                return Fail<MissingProductDto>(MsgNameRequired);
            // کاور همیشه اولین تصویر فهرست است، پس خالی‌بودنش یعنی هیچ تصویری آپلود نشده.
            if (entity.PictureId == null)
                return Fail<MissingProductDto>(MsgPictureRequired);

            var now = DateTime.UtcNow;
            entity.Status = MissingProductStatus.Submitted;
            entity.SubmittedDate = now;
            entity.UpdateDate = now;
            entity.RejectionReason = null;
            await _context.SaveChangesAsync();

            return new BaseResultDto<MissingProductDto>(true, ToDto(entity));
        }

        public async Task<BaseResultDto> DeleteAsync(long storeId, long id)
        {
            var entity = await FindOwnAsync(storeId, id, includePicture: false);
            // رکورد فروشندهٔ دیگر هم دقیقاً همین پاسخ را می‌گیرد تا وجودش لو نرود
            if (entity == null)
                return new BaseResultDto(false, MsgNotFound, 3);

            if (entity.Status != MissingProductStatus.Draft && entity.Status != MissingProductStatus.Rejected)
                return new BaseResultDto(false, MsgNotEditable, 1);

            _context.MissingProducts.Remove(entity);
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        // ================= ادمین =================

        public async Task<BaseResultDto<MissingProductAdminListDto>> SearchAsync(MissingProductAdminSearchDto dto)
        {
            dto ??= new MissingProductAdminSearchDto();
            var pageNumber = Math.Max(1, dto.PageNumber);
            var pageSize = Math.Clamp(dto.PageSize <= 0 ? 20 : dto.PageSize, 1, 100);

            var query = _context.MissingProducts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                if (!TryParseStatus(dto.Status, out var status))
                    return Fail<MissingProductAdminListDto>(MsgInvalid);
                query = query.Where(x => x.Status == status);
            }
            else
            {
                // پیش‌نویس‌ها خصوصی فروشنده‌اند؛ مگر صراحتاً status=draft خواسته شود
                query = query.Where(x => x.Status != MissingProductStatus.Draft);
            }

            if (dto.StoreId.HasValue)
                query = query.Where(x => x.StoreId == dto.StoreId.Value);

            var q = dto.Q?.Trim();
            if (!string.IsNullOrEmpty(q))
                query = query.Where(x => x.Name.Contains(q) || (x.Brand != null && x.Brand.Contains(q)));

            var total = await query.CountAsync();

            // صف بررسی: قدیمی‌ترین ارسال‌شده اول؛ بقیهٔ حالت‌ها جدیدترین اول
            var ordered = string.Equals(dto.Status, "submitted", StringComparison.OrdinalIgnoreCase)
                ? query.OrderBy(x => x.SubmittedDate).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.Id);

            var items = await ordered
                .Include(x => x.Picture)
                .Include(x => x.Pictures).ThenInclude(x => x.Picture)
                .Include(x => x.Store)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BaseResultDto<MissingProductAdminListDto>(true, new MissingProductAdminListDto
            {
                Items = items.Select(ToAdminDto).ToList(),
                Total = total
            });
        }

        public async Task<BaseResultDto<MissingProductAdminDto>> GetAsync(long id, CancellationToken cancellationToken)
        {
            var entity = await _context.MissingProducts
                .Include(x => x.Picture)
                .Include(x => x.Pictures).ThenInclude(x => x.Picture)
                .Include(x => x.Store)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity == null)
                return Fail<MissingProductAdminDto>(MsgNotFound, 3);

            var dto = ToAdminDto(entity);
            dto.SimilarProducts = await FindSimilarProductsAsync(entity.Name, cancellationToken);
            return new BaseResultDto<MissingProductAdminDto>(true, dto);
        }

        public async Task<BaseResultDto<MissingProductApproveResultDto>> ApproveAsync(long adminUserId, MissingProductApproveDto dto)
        {
            if (dto == null || dto.Id <= 0 || dto.CategoryId <= 0)
                return Fail<MissingProductApproveResultDto>(MsgInvalid);

            string slug;
            try
            {
                // همان اعتبارسنجی Label سایر محصولات: فقط حروف انگلیسی/عدد
                slug = SlugNormalizer.Normalize(dto.ProductLabel);
            }
            catch (ValidationException ex)
            {
                return Fail<MissingProductApproveResultDto>(ex.Message);
            }

            if (!await _context.Categories.AnyAsync(x => x.Id == dto.CategoryId))
                return Fail<MissingProductApproveResultDto>("دسته‌بندی انتخاب‌شده معتبر نیست.");
            if (dto.BrandId.HasValue && !await _context.Brands.AnyAsync(x => x.Id == dto.BrandId.Value))
                return Fail<MissingProductApproveResultDto>("برند انتخاب‌شده معتبر نیست.");

            if (dto.PictureId.HasValue && !await _context.Pictures.AnyAsync(x => x.Id == dto.PictureId.Value))
                return Fail<MissingProductApproveResultDto>(MsgInvalid);
            if (dto.Price is < 0 || dto.Quantity is < 0)
                return Fail<MissingProductApproveResultDto>(MsgInvalid);

            var entity = await _context.MissingProducts.AsTracking().FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (entity == null)
                return Fail<MissingProductApproveResultDto>(MsgNotFound, 3);
            if (entity.Status != MissingProductStatus.Submitted)
                return Fail<MissingProductApproveResultDto>(MsgNotSubmitted);

            var officialName = string.IsNullOrWhiteSpace(dto.Name) ? entity.Name : dto.Name.Trim();
            if (officialName.Length > MaxNameLength)
                return Fail<MissingProductApproveResultDto>(MsgInvalid);

            // رزرو اتمیک: فقط یک ادمین می‌تواند یک درخواست را به «تأییدشده» ببرد؛ از ساخت دوبارهٔ محصول جلوگیری می‌کند
            var now = DateTime.UtcNow;
            var claimed = await _context.MissingProducts
                .Where(x => x.Id == entity.Id && x.Status == MissingProductStatus.Submitted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, MissingProductStatus.Approved)
                    .SetProperty(x => x.ReviewedDate, now)
                    .SetProperty(x => x.ReviewedByUserId, adminUserId)
                    .SetProperty(x => x.UpdateDate, now));
            if (claimed == 0)
                return Fail<MissingProductApproveResultDto>(MsgNotSubmitted);

            var productDto = new ProductDto
            {
                Name = officialName,
                ProductLabel = dto.ProductLabel.Trim(),
                SecondName = dto.SecondName?.Trim(),
                Summary = dto.Summary?.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? entity.Description : dto.Description.Trim(),
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                PictureId = dto.PictureId ?? entity.PictureId,
                StatusId = (long)ProductStatusEnum.ProductStatus_Available,
                TypeId = (long)ProductTypeEnum.ProductType_Product,
                Active = true,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now
            };

            var inserted = await _productService.InsertAsyncDto(productDto);
            if (!inserted.IsSuccess || inserted.Data == null || inserted.Data.Id <= 0)
            {
                await RevertClaimAsync(entity.Id);
                _logger.LogWarning("MissingProduct {Id} approval failed while creating the catalog product (slug {Slug}).", entity.Id, slug);
                return new BaseResultDto<MissingProductApproveResultDto>(false, inserted.Messages ?? new List<Tuple<string, string>>(), null, 1);
            }

            var productId = inserted.Data.Id;
            await _context.MissingProducts
                .Where(x => x.Id == entity.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.ProductId, productId));

            // تصاویر فروشنده (تا ۵ تا) به گالری محصول کاتالوگ منتقل می‌شوند؛ کاور از قبل به‌عنوان
            // PictureId روی خود محصول نشسته. شکست این مرحله نباید تأیید را باطل کند.
            try
            {
                var galleryPictureIds = await _context.MissingProductPictures
                    .Where(x => x.MissingProductId == entity.Id)
                    .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
                    .Select(x => x.PictureId)
                    .ToListAsync();

                if (galleryPictureIds.Count > 0)
                {
                    await _context.ProductPictures.AddRangeAsync(galleryPictureIds
                        .Select(pictureId => new ProductPicture { ProductId = productId, PictureId = pictureId }));
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MissingProduct {Id}: copying pictures to product {ProductId} failed.", entity.Id, productId);
            }

            // محصول همان لحظه به فروشگاه درخواست‌دهنده هم اضافه می‌شود (اگر قیمت داده بود)؛ شکستش تأیید را باطل نمی‌کند
            long? productItemId = null;
            var itemPrice = dto.Price ?? entity.Price;
            var itemQuantity = dto.Quantity ?? entity.Quantity;
            if (itemPrice is > 0)
            {
                try
                {
                    var itemResult = await _productItemService.InsertOrUpdateAsync(new ProductItemListUpdateDto
                    {
                        StoreId = entity.StoreId,
                        ProductId = productId,
                        ProductItems = new List<ProductItemDto>
                        {
                            new() { BasePrice = itemPrice.Value, Quantity = itemQuantity ?? 0, Active = true, Warranty = "" }
                        }
                    });

                    if (itemResult.IsSuccess)
                    {
                        productItemId = await _context.ProductItems
                            .Where(x => x.ProductId == productId && x.StoreId == entity.StoreId && !x.Deleted)
                            .Select(x => (long?)x.Id)
                            .FirstOrDefaultAsync();
                        await _context.MissingProducts
                            .Where(x => x.Id == entity.Id)
                            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ProductItemId, productItemId));
                    }
                    else
                    {
                        _logger.LogWarning("MissingProduct {Id}: catalog product {ProductId} created but store item was not.", entity.Id, productId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "MissingProduct {Id}: creating the store item failed after the product {ProductId} was created.", entity.Id, productId);
                }
            }

            return new BaseResultDto<MissingProductApproveResultDto>(true, new MissingProductApproveResultDto
            {
                ProductId = productId,
                StoreItemCreated = productItemId.HasValue,
                ProductItemId = productItemId
            });
        }

        public async Task<BaseResultDto<MissingProductAdminDto>> RejectAsync(long adminUserId, MissingProductRejectDto dto)
        {
            var reason = dto?.Reason?.Trim();
            if (dto == null || dto.Id <= 0 || string.IsNullOrWhiteSpace(reason) || reason.Length > MaxReasonLength)
                return Fail<MissingProductAdminDto>("دلیل رد لازم است (حداکثر ۵۰۰ کاراکتر).");

            var entity = await _context.MissingProducts.AsTracking()
                .Include(x => x.Picture).Include(x => x.Store)
                .FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (entity == null)
                return Fail<MissingProductAdminDto>(MsgNotFound, 3);
            if (entity.Status != MissingProductStatus.Submitted)
                return Fail<MissingProductAdminDto>(MsgNotSubmitted);

            var now = DateTime.UtcNow;
            entity.Status = MissingProductStatus.Rejected;
            entity.RejectionReason = reason;
            entity.ReviewedDate = now;
            entity.ReviewedByUserId = adminUserId;
            entity.UpdateDate = now;
            await _context.SaveChangesAsync();

            return new BaseResultDto<MissingProductAdminDto>(true, ToAdminDto(entity));
        }

        // ================= کمکی =================

        private async Task<MissingProduct> FindOwnAsync(long storeId, long id, bool includePicture)
        {
            if (storeId <= 0 || id <= 0)
                return null;

            var query = _context.MissingProducts.AsTracking().Where(x => x.Id == id && x.StoreId == storeId);
            if (includePicture)
                query = query.Include(x => x.Picture).Include(x => x.Pictures).ThenInclude(x => x.Picture);
            return await query.FirstOrDefaultAsync();
        }

        // هر ویرایشی روی رکورد ردشده آن را به پیش‌نویس برمی‌گرداند تا دوباره قابل ارسال شود
        private static void MarkEdited(MissingProduct entity)
        {
            entity.UpdateDate = DateTime.UtcNow;
            if (entity.Status == MissingProductStatus.Rejected)
            {
                entity.Status = MissingProductStatus.Draft;
                entity.RejectionReason = null;
            }
        }

        private async Task RevertClaimAsync(long id)
        {
            await _context.MissingProducts
                .Where(x => x.Id == id && x.Status == MissingProductStatus.Approved && x.ProductId == null)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, MissingProductStatus.Submitted)
                    .SetProperty(x => x.ReviewedDate, (DateTime?)null)
                    .SetProperty(x => x.ReviewedByUserId, (long?)null));
        }

        private async Task<List<MissingProductSimilarProductDto>> FindSimilarProductsAsync(string name, CancellationToken cancellationToken)
        {
            try
            {
                var request = new SearchRequestDto
                {
                    Q = name,
                    ProductCount = SimilarProductCount,
                    BrandCount = 0, CategoryCount = 0, FeatureCount = 0, CompanionCount = 0,
                    AssistanceCount = 0, StoreCount = 0, PansionCount = 0, PackageCount = 0,
                    TotalCount = SimilarProductCount,
                    EnableFuzzy = true
                };
                request.Q = SearchNormalizeHelper.Normalize(request.Q);
                request.ClampCounts();
                request.SearchTerms = SearchNormalizeHelper.BuildTerms(request.Q, request.EnableFuzzy);
                if (string.IsNullOrWhiteSpace(request.Q) || request.Q.Length < 2)
                    return new List<MissingProductSimilarProductDto>();

                var ids = (await _productService.SearchCatalogProductIdsAsync(request, cancellationToken) ?? new List<long>())
                    .Distinct().Take(SimilarProductCount).ToList();
                if (ids.Count == 0)
                    return new List<MissingProductSimilarProductDto>();

                var products = await _context.Products
                    .Include(p => p.Brand).Include(p => p.Picture)
                    .Where(p => ids.Contains(p.Id) && p.Active && !p.Deleted)
                    .ToListAsync(cancellationToken);

                return ids.Select(id => products.FirstOrDefault(p => p.Id == id))
                    .Where(p => p != null)
                    .Select(p => new MissingProductSimilarProductDto
                    {
                        ProductId = p.Id,
                        Name = p.Name,
                        BrandName = p.Brand?.Name,
                        PictureUrl = PictureUrl(p.Picture)
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                // فهرست «مشابه‌ها» فقط کمک‌بررسی است؛ خطایش نباید نمایش خود درخواست را خراب کند
                _logger.LogWarning(ex, "MissingProduct similar-product lookup failed.");
                return new List<MissingProductSimilarProductDto>();
            }
        }

        // بررسی واقعی محتوا (نه فقط پسوند): امضای بایت‌های ابتدای فایل باید jpg/png/webp باشد
        private static bool IsDuplicateKey(DbUpdateException exception)
            => exception.InnerException is SqlException { Number: 2601 or 2627 };

        private static string Trim(string value, int maxLength)
        {
            var trimmed = value?.Trim();
            if (string.IsNullOrEmpty(trimmed))
                return null;
            return trimmed.Length > maxLength ? trimmed[..maxLength] : trimmed;
        }

        private static bool TryParseStatus(string value, out MissingProductStatus status)
            => Enum.TryParse(value?.Trim(), ignoreCase: true, out status) && Enum.IsDefined(status);

        private static string PictureUrl(Picture picture)
            => picture == null ? null : $"{picture.Url}/{picture.Name}";

        private static DateTime Utc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

        private static MissingProductDto ToDto(MissingProduct entity)
        {
            var dto = new MissingProductDto();
            FillDto(dto, entity);
            return dto;
        }

        private static MissingProductAdminDto ToAdminDto(MissingProduct entity)
        {
            var dto = new MissingProductAdminDto
            {
                StoreId = entity.StoreId,
                StoreName = entity.Store?.Name,
                ProductItemId = entity.ProductItemId,
                SubmittedAtUtc = entity.SubmittedDate.HasValue ? Utc(entity.SubmittedDate.Value) : null,
                ReviewedAtUtc = entity.ReviewedDate.HasValue ? Utc(entity.ReviewedDate.Value) : null
            };
            FillDto(dto, entity);
            return dto;
        }

        private static void FillDto(MissingProductDto dto, MissingProduct entity)
        {
            dto.Id = entity.Id;
            dto.Name = entity.Name;
            dto.Brand = entity.Brand;
            dto.PackageSize = entity.PackageSize;
            dto.Description = entity.Description;
            dto.Price = entity.Price;
            dto.Quantity = entity.Quantity;
            dto.PictureUrl = PictureUrl(entity.Picture);
            dto.Pictures = entity.Pictures == null
                ? new List<MissingProductPictureDto>()
                : entity.Pictures
                    .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
                    .Select(x => new MissingProductPictureDto { PictureId = x.PictureId, PictureUrl = PictureUrl(x.Picture) })
                    .ToList();
            dto.Status = entity.Status.ToString().ToLowerInvariant();
            dto.RejectionReason = entity.Status == MissingProductStatus.Rejected ? entity.RejectionReason : null;
            dto.Source = entity.Source;
            dto.CreatedAtUtc = Utc(entity.CreateDate);
            dto.ProductId = entity.ProductId;
        }

        private static BaseResultDto<T> Fail<T>(string message, int code = 1)
            => new(false, message, default, code);
    }
}
