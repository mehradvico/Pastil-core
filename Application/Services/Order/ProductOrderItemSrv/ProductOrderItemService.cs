using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.Order.ProductOrderItemSrv.Dto;
using Application.Services.Order.ProductOrderItemSrv.Iface;
using Application.Services.Order.ProductOrderStoreSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Order.ProductOrderItemItemSrv
{
    public class ProductOrderItemService : CommonSrv<ProductOrderItem, ProductOrderItemDto>, IProductOrderItemService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IProductOrderStoreService _productOrderStoreService;

        public ProductOrderItemService(IDataBaseContext _context, IProductOrderStoreService productOrderStoreService, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._productOrderStoreService = productOrderStoreService;
        }
        public override BaseResultDto UpdateDto(ProductOrderItemDto dto)
        {
            var item = _context.ProductOrderItems.AsTracking().FirstOrDefault(s => s.Id == dto.Id);
            if (item != null)
            {
                // فقط توضیحات: تعداد/حذف بدون محاسبه‌ی مبلغ، موجودی و برگشت پول مبلغ سفارش را ناهماهنگ می‌کرد؛
                // کم/حذف کردن کالا فقط از مسیر Admin|Seller ProductOrderItemAdjust (با برگشت پول به کیف پول) انجام می‌شود.
                item.Description = dto.Description;
                _context.ProductOrderItems.Update(item);
                _context.SaveChanges();

            }
            return new BaseResultDto(true);
        }
        public async Task<BaseResultDto> InsertRangeAsyncDto(List<ProductOrderItemDto> dtos)
        {
            try
            {
                foreach (ProductOrderItemDto dto in dtos)
                {
                    var item = mapper.Map<ProductOrderItem>(dto);
                    await _context.ProductOrderItems.AddAsync(item);
                }
            }
            catch
            {
                return new BaseResultDto(isSuccess: false);
            }
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }
        public BaseSearchDto<ProductOrderItemVDto> Search(ProductOrderItemInputDto baseSearchDto)
        {
            var query = _context.ProductOrderItems.Include(s => s.ProductItem).ThenInclude(s => s.VarietyItem).ThenInclude(s => s.Variety).Include(s => s.ProductItem).ThenInclude(s => s.VarietyItem2).ThenInclude(s => s.Variety).Where(s => s.ProductOrderStoreId == baseSearchDto.ProductOrderStoreId).AsQueryable();

            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                query = query.Where(s => s.ProductItem.Product.Name.Contains(baseSearchDto.Q));
            }

            switch (baseSearchDto.SortBy)
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
                case Common.Enumerable.SortEnum.Expensive:
                    {
                        query = query.OrderByDescending(s => s.Price);
                        break;
                    }
                case Common.Enumerable.SortEnum.Inexpensive:
                    {
                        query = query.OrderBy(s => s.Price);
                        break;
                    }
                default:
                    break;
            }

            return new BaseSearchDto<ProductOrderItem, ProductOrderItemVDto>(baseSearchDto, query, mapper);
        }
    }
}
