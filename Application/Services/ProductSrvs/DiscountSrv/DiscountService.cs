using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.ProductSrvs.DiscountSrv.Dto;
using Application.Services.ProductSrvs.DiscountSrv.IFace;
using Application.Services.ProductSrvs.ProductSrv.Iface;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.StoreSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.DiscountSrv
{
    public class DiscountService : IDiscountService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICodeService _codeService;
        private readonly IProductService _productService;
        private readonly IStoreService _storeService;
        public DiscountService(IDataBaseContext _context, IMapper mapper, IStoreService storeService, ICodeService codeService, IProductService productService)
        {
            this._context = _context;
            this.mapper = mapper;
            this._codeService = codeService;
            this._productService = productService;
            this._storeService = storeService;
        }

        public async Task SyncExpiredAsync()
        {
            var expireds = _context.Discounts.AsTracking().Include(s => s.Type).Where(s => s.Active && s.EndDate.HasValue && s.EndDate.Value < DateTime.Now.Date).ToList();

            if (expireds.Any())
            {
                foreach (var discount in expireds)
                {
                    await UpdateDiscountsAsync(discount);
                }
                expireds.ForEach(s => s.Active = false);
                _context.Discounts.UpdateRange(expireds);
                await _context.SaveChangesAsync();
            }
        }
        private async Task UpdateDiscountsAsync(Discount discount)
        {
            var updateType = ProductUpdateTypeEnum.Store;
            var updateId = discount.StoreId;

            var type = discount.Type;
            if (type != null)
            {
                if (type.Label == DiscountTypeEnum.DiscountType_Category.ToString())
                {
                    updateType = ProductUpdateTypeEnum.Category;
                    updateId = discount.CategoryId.Value;
                }
                if (type.Label == DiscountTypeEnum.DiscountType_Brand.ToString())
                {

                    updateType = ProductUpdateTypeEnum.Brand;
                    updateId = discount.BrandId.Value;
                }
                if (type.Label == DiscountTypeEnum.DiscountType_Product.ToString())
                {

                    updateType = ProductUpdateTypeEnum.Product;
                    updateId = discount.ProductId.Value;
                }
                if (type.Label == DiscountTypeEnum.DiscountType_ProductItem.ToString())
                {
                    updateType = ProductUpdateTypeEnum.ProductItem;
                    updateId = discount.ProductItemId.Value;
                }
            }
            await _productService.UpdateProductPriceAsync(updateType, Id: updateId.ToString());
            await SetStoreMaxDiscountAsync(discount.StoreId);
        }
        public async Task<BaseResultDto> ActiveAsync(DiscountDto discount)
        {
            var item = await _context.Discounts.Include(s => s.Type).FirstOrDefaultAsync(s => s.Id == discount.Id && s.StoreId == discount.StoreId);
            if (item != null)
            {
                item.Active = discount.Active;
                _context.Discounts.Update(item);
                await _context.SaveChangesAsync();
                await UpdateDiscountsAsync(item);

                return new BaseResultDto(true);
            }
            return new BaseResultDto(false);
        }

        public async Task<BaseResultDto> DeleteAsync(DiscountDto discount)
        {
            var item = await _context.Discounts.Include(s => s.Type).FirstOrDefaultAsync(s => s.Id == discount.Id && s.StoreId == discount.StoreId);
            if (item != null)
            {
                item.Deleted = true;
                _context.Discounts.Update(item);
                await _context.SaveChangesAsync();

                await UpdateDiscountsAsync(item);

                return new BaseResultDto(true);
            }
            return new BaseResultDto(false);
        }

        public async Task<BaseResultDto> FindAsyncDto(long id)
        {
            var Discount = await _context.Discounts.Include(s => s.Type).Include(s => s.Store).Include(s => s.Category).Include(s => s.Brand).Include(s => s.Product).Include(s => s.Product).FirstOrDefaultAsync(s => s.Id.Equals(id));
            if (Discount != null)
            {
                return new BaseResultDto<DiscountDto>(true, mapper.Map<DiscountDto>(Discount));
            }
            return new BaseResultDto(false, val: Resource.Notification.ResourceNotFind);
        }


        public async Task<BaseResultDto> InsertAsync(DiscountDto discount)
        {
            try
            {
                if (discount.EndDate != null)
                    discount.EndDate = discount.EndDate.Value.Date;
                if (discount.Percent < 1 || discount.Percent > 99)
                {
                    return new BaseResultDto(false, val1: Resource.Notification.ThePercentageRangeNotCorrect, val2: nameof(discount.Percent));
                }
                else if (discount.EndDate < DateTime.Now.Date)
                {
                    return new BaseResultDto(false, val1: Resource.Notification.TheDateNotCorrect, val2: nameof(discount.EndDate));
                }
                var updateType = ProductUpdateTypeEnum.Store;
                var updateId = discount.StoreId;

                var type = await _codeService.GetByIdAsync(discount.TypeId);
                if (type != null)
                {
                    if (type.Label == DiscountTypeEnum.DiscountType_Category.ToString())
                    {

                        if (!discount.CategoryId.HasValue)
                        {
                            return new BaseResultDto<DiscountDto>(isSuccess: false, val: string.Format(Resource.Pattern.PleaseSelectT1, Resource.Field.Category), data: discount);
                        }
                        discount.BrandId = null;
                        discount.ProductId = null;
                        discount.ProductItemId = null;
                        updateType = ProductUpdateTypeEnum.Category;
                        updateId = discount.CategoryId.Value;
                    }
                    if (type.Label == DiscountTypeEnum.DiscountType_Brand.ToString())
                    {

                        if (!discount.BrandId.HasValue)
                        {
                            return new BaseResultDto<DiscountDto>(isSuccess: false, val: string.Format(Resource.Pattern.PleaseSelectT1, Resource.Field.Brand), data: discount);
                        }
                        discount.CategoryId = null;
                        discount.ProductId = null;
                        discount.ProductItemId = null;
                        updateType = ProductUpdateTypeEnum.Brand;
                        updateId = discount.BrandId.Value;
                    }
                    if (type.Label == DiscountTypeEnum.DiscountType_Product.ToString())
                    {

                        if (!discount.ProductId.HasValue)
                        {
                            return new BaseResultDto<DiscountDto>(isSuccess: false, val: string.Format(Resource.Pattern.PleaseSelectT1, Resource.Field.Product), data: discount);
                        }
                        discount.CategoryId = null;
                        discount.BrandId = null;
                        discount.ProductItemId = null;
                        updateType = ProductUpdateTypeEnum.Product;
                        updateId = discount.ProductId.Value;
                    }
                    if (type.Label == DiscountTypeEnum.DiscountType_ProductItem.ToString())
                    {

                        if (!discount.ProductItemId.HasValue)
                        {
                            return new BaseResultDto<DiscountDto>(isSuccess: false, val: string.Format(Resource.Pattern.PleaseSelectT1, Resource.Field.ProductItem), data: discount);
                        }
                        discount.CategoryId = null;
                        discount.BrandId = null;
                        discount.ProductId = null;
                        updateType = ProductUpdateTypeEnum.ProductItem;
                        updateId = discount.ProductItemId.Value;
                    }
                }
                var item = mapper.Map<Discount>(discount);
                await _context.Discounts.AddAsync(item);
                await _context.SaveChangesAsync();
                await _productService.UpdateProductPriceAsync(updateType, Id: updateId.ToString());

                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto<DiscountDto>(isSuccess: false, val: ex.Message, data: discount);
            }
        }

        public DiscountSearchDto Search(DiscountInputDto searchDto)
        {
            var query = _context.Discounts.Include(s => s.Product).ThenInclude(s => s.Category).Include(s => s.ProductItem).ThenInclude(s => s.Product).ThenInclude(s => s.Category).Include(s => s.DiscountGroup).AsQueryable();
            if (searchDto.Available == true)
            {
                query = query.Where(s => s.Deleted == false && s.Active && (s.EndDate == null || (s.EndDate.HasValue && s.EndDate > DateTime.Now)));
            }
            if (searchDto.StoreId.HasValue)
            {
                query = query.Where(s => s.StoreId == searchDto.StoreId);
            }
            if (searchDto.CategoryId.HasValue)
            {
                query = query.Where(s => s.CategoryId == searchDto.CategoryId);
            }
            if (searchDto.BrandId.HasValue)
            {
                query = query.Where(s => s.BrandId == searchDto.BrandId);
            }
            if (searchDto.ProductId.HasValue)
            {
                var product = _context.Products.Include(s => s.Category).Include(s => s.Categories).Include(s => s.ProductItems.Where(s => searchDto.StoreId.HasValue ? s.StoreId == searchDto.StoreId : true)).AsTracking().FirstOrDefault(s => s.Id == searchDto.ProductId);
                if (product != null)
                {
                    var categoryIds = product.Categories.Select(s => s.Id).ToList();
                    var itemIds = product.ProductItems.Select(s => s.Id).ToList();
                    query = query.Where(s => s.ProductId == searchDto.ProductId || (s.CategoryId.HasValue && categoryIds.Any(a => a.Equals(s.CategoryId))) || (s.BrandId.HasValue && s.BrandId == product.BrandId) || (searchDto.StoreId.HasValue && s.StoreId == searchDto.StoreId.Value) || (s.ProductItemId.HasValue && itemIds.Any(a => a.Equals(s.ProductItemId.Value))));

                }
                else
                {
                    query = query.Where(s => s.ProductId == searchDto.ProductId);

                }

            }
            if (searchDto.ProductItemId.HasValue)
            {
                var productItem = _context.ProductItems.Include(s => s.Product).ThenInclude(s => s.Category).Include(s => s.Product).ThenInclude(s => s.Categories).FirstOrDefault(s => s.Id == searchDto.ProductItemId);
                if (productItem != null)
                {
                    var categoryIds = productItem.Product.Categories.Select(s => s.Id).ToList();

                    query = query.Where(s => s.ProductItemId == searchDto.ProductItemId || s.ProductId == productItem.ProductId || (s.CategoryId.HasValue && categoryIds.Any(a => a == s.CategoryId)) || (s.BrandId.HasValue && s.BrandId == productItem.Product.BrandId) || (searchDto.StoreId.HasValue ? s.StoreId == searchDto.StoreId : true));

                }
                else
                {
                    query = query.Where(s => s.ProductItemId == searchDto.ProductItemId);

                }
            }
            if (searchDto.DiscountType.HasValue)
            {
                query = query.Where(s => s.Type.Label == searchDto.DiscountType.ToString());
            }
            if (searchDto.SortBy != Common.Enumerable.SortEnum.Default)
            {
                switch (searchDto.SortBy)
                {
                    case Common.Enumerable.SortEnum.Default:
                        {
                            query = query.OrderByDescending(s => s.Id);

                            break;
                        }
                    case Common.Enumerable.SortEnum.New:
                        {
                            query = query.OrderByDescending(s => s.Id);
                            break;
                        }
                    case Common.Enumerable.SortEnum.Old:
                        {
                            query = query.OrderBy(s => s.Id);
                            break;
                        }
                    case Common.Enumerable.SortEnum.Inexpensive:
                        {
                            query = query.OrderBy(s => s.Percent);
                            break;
                        }
                    case Common.Enumerable.SortEnum.Expensive:
                        {
                            query = query.OrderByDescending(s => s.Percent);
                            break;
                        }

                    default:
                        break;
                }
            }
            return new DiscountSearchDto(searchDto, query, mapper);
        }

        private async Task SetStoreMaxDiscountAsync(long storeId)
        {
            var maxDiscount = _context.Discounts.AsTracking().Where(s => s.StoreId == storeId && s.Active && s.Deleted == false && (s.EndDate == null || s.EndDate >= DateTime.Now.Date));
            if (maxDiscount.Any())
            {
                await _storeService.SetMaxDiscountAsync(storeId: storeId, maxDiscount.Max(s => s.Percent));
            }
        }
    }
}
