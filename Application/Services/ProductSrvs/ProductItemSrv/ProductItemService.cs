using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Service;
using Application.Services.ProductSrvs.ProductItemSrv.Dto;
using Application.Services.ProductSrvs.ProductItemSrv.Iface;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Iface;
using Application.Services.ProductSrvs.ProductStockAlertSrv.Iface;
using Application.Services.ProductSrvs.VarietyItemSrv.Dto;
using Application.Services.ProductSrvs.VarietySrv.Dto;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Application.Services.ProductSrvs.ProductItemSrv
{
    public class ProductItemService : CommonSrv<ProductItem, ProductItemDto>, IProductItemService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IProductService _productService;
        private readonly IProductStockAlertService _productStockAlertService;
        public ProductItemService(IDataBaseContext _context, IMapper mapper, IProductService productService, IProductStockAlertService productStockAlertService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._productService = productService;
            this._productStockAlertService = productStockAlertService;
        }

        public ProductItemSearchDto SearchDto(ProductItemInputDto dto)
        {

            var model = _context.ProductItems.Include(s => s.Store).Include(s => s.VarietyItem).Include(s => s.VarietyItem2).AsQueryable();
            if (dto.ProductId.HasValue)
            {
                model = model.Where(s => s.ProductId.Equals(dto.ProductId));
            }
            if (dto.StoreId.HasValue)
            {
                model = model.Where(s => s.StoreId.Equals(dto.StoreId));
            }
            model = model.OrderByDescending(s => s.ProductId).ThenByDescending(s => s.Quantity);
            return new ProductItemSearchDto(dto, model, mapper);

        }
        public void InsertOrUpdate(ProductItemDto productItem)
        {
            var item = _context.ProductItems.AsTracking().FirstOrDefault(s => s.ProductId == productItem.ProductId && s.VarietyItemId == productItem.VarietyItemId && s.VarietyItem2Id == productItem.VarietyItem2Id && s.StoreId == productItem.StoreId && s.Deleted == false);
            if (item != null)
            {
                item.BasePrice = productItem.BasePrice;
                item.Price = productItem.BasePrice;
                item.Quantity = productItem.Quantity;
                item.Active = productItem.Active;
                _context.ProductItems.Update(item);
            }
            else
            {
                if (productItem.BasePrice > 0 && productItem.Quantity > 0)
                {
                    item = mapper.Map<ProductItem>(productItem);
                    item.SystemActive = true;
                    _context.ProductItems.Add(item);
                }
            }
            _context.SaveChanges();
        }
        public void InsertOrUpdate(List<ProductItemDto> productItemListUpdate)
        {
            foreach (var item in productItemListUpdate)
            {
                InsertOrUpdate(item);
            }
        }
        public async Task<BaseResultDto> InsertOrUpdateAsync(ProductItemListUpdateDto productItemListUpdate)
        {
            try
            {
                productItemListUpdate.ProductItems.ForEach(s => s.StoreId = productItemListUpdate.StoreId);
                productItemListUpdate.ProductItems.ForEach(s => s.ProductId = productItemListUpdate.ProductId);

                var submittedKeys = productItemListUpdate.ProductItems
                    .Select(item => BuildInventoryKey(item.VarietyItemId, item.VarietyItem2Id))
                    .ToHashSet();
                var previousItems = await _context.ProductItems
                    .AsNoTracking()
                    .Where(item => item.ProductId == productItemListUpdate.ProductId && item.StoreId == productItemListUpdate.StoreId && !item.Deleted)
                    .ToListAsync();

                InsertOrUpdate(productItemListUpdate.ProductItems);

                var currentItems = await _context.ProductItems
                    .AsNoTracking()
                    .Include(item => item.Store)
                    .Where(item => item.ProductId == productItemListUpdate.ProductId && item.StoreId == productItemListUpdate.StoreId && !item.Deleted)
                    .ToListAsync();
                var previousByKey = previousItems
                    .GroupBy(item => BuildInventoryKey(item.VarietyItemId, item.VarietyItem2Id))
                    .ToDictionary(group => group.Key, group => group.First());
                var restockedItems = currentItems
                    .Where(item => submittedKeys.Contains(BuildInventoryKey(item.VarietyItemId, item.VarietyItem2Id)) &&
                                   item.Quantity > 0 && item.Active && item.SystemActive && item.Store != null && item.Store.Active &&
                                   (!previousByKey.TryGetValue(BuildInventoryKey(item.VarietyItemId, item.VarietyItem2Id), out var previous) || previous.Quantity <= 0))
                    .GroupBy(item => new { item.ProductId, item.StoreId })
                    .Select(group => group.Key)
                    .ToList();

                foreach (var restockedItem in restockedItems)
                    await _productStockAlertService.NotifyRestockedAsync(restockedItem.ProductId, restockedItem.StoreId);

                await _productService.UpdateProductPriceAsync(ProductUpdateTypeEnum.Product, Id: productItemListUpdate.ProductId.ToString());
                return new BaseResultDto(true);

            }
            catch
            {
                return new BaseResultDto(false);
            }
        }

        private static string BuildInventoryKey(long? varietyItemId, long? varietyItem2Id)
            => $"{varietyItemId?.ToString() ?? "null"}|{varietyItem2Id?.ToString() ?? "null"}";
        public async Task<BaseResultDto> GetInsertOrUpdateListAsync(ProductItemListRequestDto productItemListrequest)
        {
            ProductItemListUpdateDto productItemListUpdate = new ProductItemListUpdateDto();
            productItemListUpdate.ProductId = productItemListrequest.ProductId;
            productItemListUpdate.StoreId = productItemListrequest.StoreId;
            var product = await _context.Products.AsTracking().Include(s => s.Variety).ThenInclude(s => s.VarietyItems.Where(s => s.Deleted == false)).Include(s => s.Variety2).ThenInclude(s => s.VarietyItems.Where(s => s.Deleted == false)).Include(s => s.ProductItems.Where(p => p.Deleted == false && p.StoreId == productItemListUpdate.StoreId)).ThenInclude(s => s.Store).FirstOrDefaultAsync(s => s.Id == productItemListUpdate.ProductId);
            productItemListUpdate.ProductItems = new List<ProductItemDto>();
            productItemListUpdate.Product = mapper.Map<ProductMinVDto>(product);
            var newItem = new ProductItemDto()
            {
                ProductId = productItemListUpdate.ProductId,
                Active = true,
                StoreId = productItemListUpdate.StoreId,
            };
            if (product.Variety == null && product.Variety2 == null)
            {
                var item = product.ProductItems.FirstOrDefault(s => s.VarietyItemId == null);
                if (item != null)
                    mapper.Map(item, newItem);
                productItemListUpdate.ProductItems.Add(newItem);
            }
            else
            {
                var varietyItems = new List<VarietyItem>();
                if (product.Variety != null)
                {
                    varietyItems = product.Variety.VarietyItems.ToList();
                }
                else
                {
                    varietyItems.Add(null);
                }
                var varietyItems2 = new List<VarietyItem>();
                if (product.Variety2 != null)
                {
                    varietyItems2 = product.Variety2.VarietyItems.ToList();
                }
                else
                {
                    varietyItems2.Add(null);
                }
                foreach (var varietyItem in varietyItems)
                {
                    long? vi1 = null;
                    if (varietyItem != null)
                        vi1 = varietyItem.Id;
                    foreach (var varietyItem2 in varietyItems2)
                    {
                        long? vi2 = null;
                        if (varietyItem2 != null)
                            vi2 = varietyItem2.Id;
                        newItem = new ProductItemDto();
                        var item = product.ProductItems.FirstOrDefault(s => s.VarietyItemId == vi1 && s.VarietyItem2Id == vi2);
                        if (item != null)
                        {
                            mapper.Map(item, newItem);
                        }
                        else
                        {
                            newItem.VarietyItem = mapper.Map<VarietyItemVDto>(varietyItem);
                            newItem.VarietyItem2 = mapper.Map<VarietyItemVDto>(varietyItem2);
                            newItem.VarietyItemId = varietyItem?.Id;
                            newItem.VarietyItem2Id = varietyItem2?.Id;
                            newItem.StoreId = productItemListUpdate.StoreId;
                            newItem.Active = true;

                        }
                        productItemListUpdate.ProductItems.Add(newItem);
                    }
                }
            }
            return new BaseResultDto<ProductItemListUpdateDto>(true, productItemListUpdate);
        }


        public async Task<BaseResultDto<ProductItemVDto>> IsSalable(long productItemId, int count)
        {
            var item = await _context.ProductItems.FirstOrDefaultAsync(x => x.Id == productItemId);
            if (item != null)
            {
                if (item.SystemActive)
                {
                    if (item.Quantity >= count)
                    {
                        return new BaseResultDto<ProductItemVDto>(true, data: mapper.Map<ProductItemVDto>(item));
                    }
                    else
                    {
                        return new BaseResultDto<ProductItemVDto>(false, val: Resource.Notification.InsufficientInventory, null);
                    }
                }
            }
            return new BaseResultDto<ProductItemVDto>(false, val: Resource.Notification.TheProductUnavailable, null);

        }

        public async Task<BaseResultDto> GetVarietyAsync(long productId)
        {
            var product = await _context.Products.AsTracking().Include(s => s.Variety).Include(s => s.Variety2)/*.ThenInclude(s => s.VarietyItems.Where(s => s.Deleted == false))*/.Include(s => s.ProductItems.OrderByDescending(o => o.SystemActive).ThenBy(s => s.Price)).ThenInclude(s => s.Store).FirstOrDefaultAsync(s => s.Id == productId && s.Status.Label == ProductStatusEnum.ProductStatus_Available.ToString() && s.Active && s.Deleted == false);
            if (product != null && product.ProductItems.Any())
            {
                var result = new ProductItemVarietyVDto();
                result.Variety = mapper.Map<VarietyShowVDto>(product.Variety);
                var group = product.ProductItems.GroupBy(g => g.VarietyItem);
                foreach (var item in group)
                {

                    result.VarietyItems.Add(mapper.Map<VarietyItemShowVDto>(item));
                }
                return new BaseResultDto<ProductItemVarietyVDto>(true, result);
            }
            return new BaseResultDto(false);

        }
        public async Task<BaseResultDto> GetVariety2Async(long productId, long? varietyItem1Id, long? varietyItem2Id)
        {
            var product = await _context.Products.Include(s => s.Variety).Include(s => s.Variety2).FirstOrDefaultAsync(s => s.Id == productId && s.Status.Label == ProductStatusEnum.ProductStatus_Available.ToString() && s.Active && s.Deleted == false);
            if (product != null)
            {
                var result = new ProductItemVariety2VDto();
                result.Variety1 = mapper.Map<VarietyShowVDto>(product.Variety);
                result.Variety2 = mapper.Map<VarietyShowVDto>(product.Variety2);
                result.VarietyItem1Id = varietyItem1Id;
                result.VarietyItem2Id = varietyItem2Id;
                var productItems = _context.ProductItems
                    .Include(s => s.VarietyItem)
                    .Include(s => s.VarietyItem2)
                    .Include(s => s.Store)
                    .Include(s => s.DiscountGroup)
                    .Where(s => !s.Deleted && s.ProductId == productId && s.SystemActive)
                    .OrderByDescending(o => o.SystemActive)
                    .ThenBy(o => o.Price);
                if (await productItems.AnyAsync())
                {
                    if (result.Variety1 != null)
                    {
                        var group = productItems.GroupBy(g => g.VarietyItem);
                        result.VarietyItems1 = mapper.Map<List<VarietyItemMinVDto>>(group.Select(s => s.Key).ToList());
                        if (result.VarietyItem1Id == null)
                            result.VarietyItem1Id = result.VarietyItems1.First().Id;
                        if (result.Variety2 != null)
                        {
                            if (result.VarietyItem2Id == null)
                            {
                                result.VarietyItems2 = mapper.Map<List<VarietyItemMinVDto>>(group.ToList().First().ToList().Select(s => s.VarietyItem2).ToList());
                                result.VarietyItem2Id = result.VarietyItems2.First().Id;
                            }
                            else
                                result.VarietyItems2 = mapper.Map<List<VarietyItemMinVDto>>(group.ToList().First(g => g.Key.Id == varietyItem1Id).ToList().Select(s => s.VarietyItem2).DistinctBy(d => d.Id).ToList());
                        }
                    }
                    result.ProductItems = mapper.Map<List<ProductItemShowVDto>>(productItems.Where(s => s.VarietyItemId == result.VarietyItem1Id && s.VarietyItem2Id == result.VarietyItem2Id));
                    return new BaseResultDto<ProductItemVariety2VDto>(true, result);
                }
            }

            return new BaseResultDto(false);

        }

        public async Task<BaseResultDto<ProductItemForProductDto>> GetForProductVDto(long productId)
        {

            var allitems = await _context.Products.IgnoreAutoIncludes().Include(s => s.Variety).Include(s => s.Variety2).Include(s => s.ProductItems.Where(s => s.ProductId == productId && s.Active && s.SystemActive && s.Deleted == false)).ThenInclude(s => s.Store).ThenInclude(s => s.Type).Include(s => s.ProductItems).ThenInclude(s => s.VarietyItem).Include(s => s.ProductItems).ThenInclude(s => s.VarietyItem2).FirstOrDefaultAsync(s => s.Id == productId);
            return new BaseResultDto<ProductItemForProductDto>(true, mapper.Map<ProductItemForProductDto>(allitems));
        }
    }
}
