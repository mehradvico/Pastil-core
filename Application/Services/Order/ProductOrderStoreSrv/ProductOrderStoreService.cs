using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Service;
using Application.Services.Order.ProductOrderStoreOrderSrv.Dto;
using Application.Services.Order.ProductOrderStoreSrv.Dto;
using Application.Services.Order.ProductOrderStoreSrv.Iface;
using Application.Services.Setting.CodeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Order.ProductOrderStoreItemSrv
{
    public class ProductOrderStoreService : CommonSrv<ProductOrderStore, ProductOrderStoreDto>, IProductOrderStoreService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICodeService _codeService;

        public ProductOrderStoreService(IDataBaseContext _context, IMapper mapper, ICodeService codeService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._codeService = codeService;
        }
        public override BaseResultDto UpdateDto(ProductOrderStoreDto dto)
        {

            var item = _context.ProductOrderStores.AsTracking().Include(s => s.ProductOrder).Include(s => s.ProductOrderItems).FirstOrDefault(s => s.Id == dto.Id);
            if (item != null)
            {
                item.Description = string.IsNullOrEmpty(dto.Description) ? item.Description : dto.Description;
                if (dto.Edited)
                {
                    item.Edited = dto.Edited;
                }
                if (dto.Deleted)
                {
                    item.Deleted = dto.Deleted;
                    foreach (var item1 in item.ProductOrderItems)
                    {
                        item1.Deleted = true;
                        item1.Edited = true;
                    }

                }
                _context.ProductOrderStores.Update(item);
                if (dto.Edited || dto.Deleted)
                {
                    var editedCode = _codeService.GetByLabelAsync(ProductOrderStateEnum.ProductOrderState_Edited.ToString()).Result;
                    item.ProductOrder.ProductOrderState = null;
                    item.ProductOrder.ProductOrderStateId = editedCode.Id;
                }

                _context.SaveChanges();
            }
            return new BaseResultDto(true);
        }

        public async Task<BaseResultDto> InsertRangeAsyncDto(List<ProductOrderStoreDto> dtos)
        {
            try
            {
                foreach (ProductOrderStoreDto dto in dtos)
                {
                    var item = mapper.Map<ProductOrderStore>(dto);
                    await _context.ProductOrderStores.AddAsync(item);
                }
            }
            catch
            {
                return new BaseResultDto(isSuccess: false);
            }
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }
        public ProductOrderStoreSearchDto Search(ProductOrderStoreInputDto baseSearchDto)
        {
            var query = _context.ProductOrderStores.Include(s => s.Store).ThenInclude(s => s.Picture).Include(s => s.ProductOrderItems).ThenInclude(s => s.ProductItem).ThenInclude(s => s.VarietyItem).ThenInclude(s => s.Variety).Include(s => s.ProductOrderItems).ThenInclude(s => s.ProductItem).ThenInclude(s => s.VarietyItem2).ThenInclude(s => s.Variety).Include(s => s.ProductOrderItems).ThenInclude(s => s.ProductItem).ThenInclude(s => s.Product).ThenInclude(s => s.Picture).AsQueryable();

            if (!string.IsNullOrEmpty(baseSearchDto.ProductOrderId))
            {
                query = query.Where(s => s.ProductOrderId == baseSearchDto.ProductOrderId);
            }
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                query = query.Where(s => s.ProductOrderItems.Any(s => s.ProductItem.Product.Name.Contains(baseSearchDto.Q)));
            }
            if (baseSearchDto.StoreId.HasValue)
            {
                query = query.Where(s => s.StoreId == baseSearchDto.StoreId);
            }
            if (baseSearchDto.UserId.HasValue)
            {
                query = query.Where(s => s.ProductOrder.UserId == baseSearchDto.UserId);
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

            return new ProductOrderStoreSearchDto(baseSearchDto, query, mapper);
        }


    }
}
