using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Iface;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Application.Services.StoreSrv.Iface;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Dapper;
using Entities.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.StoreSrv
{
    public class StoreService : CommonSrv<Store, StoreDto>, IStoreService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IProductService _productService;
        private readonly string connectionString;

        public StoreService(IDataBaseContext _context, IMapper mapper, IConfiguration config, IProductService productService) : base(_context: _context, mapper: mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._productService = productService;
            this.connectionString = config.GetValue<string>(
            "connection");
        }
        public async Task<BaseResultDto<StoreVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Stores.Include(s => s.City).ThenInclude(p => p.State).Include(s => s.Picture).Include(s => s.Users).FirstOrDefaultAsync(s => s.Id == id && s.Active && s.Deleted == false);
            if (item == null)
                return new BaseResultDto<StoreVDto>(false, null);
            return new BaseResultDto<StoreVDto>(true, mapper.Map<StoreVDto>(item));
        }

        public StoreSearchDto Search(StoreInputDto baseSearchDto)
        {
            var model = _context.Stores.Include(s => s.City).ThenInclude(p => p.State).Include(s => s.Type).Include(s => s.Picture).Include(s => s.Users).Where(s => s.Deleted == false).AsQueryable();
            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Available);
            }
            if (baseSearchDto.TypeId.HasValue)
            {
                model = model.Where(s => s.TypeId == baseSearchDto.TypeId);
            }
            if (baseSearchDto.CityId.HasValue)
            {
                model = model.Where(s => s.CityId == baseSearchDto.CityId);
            }
            if (baseSearchDto.StateId.HasValue)
            {
                model = model.Where(s => s.City.StateId == baseSearchDto.StateId);
            }
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                model = model.Where(s => s.Name.Contains(baseSearchDto.Q)).OrderByDescending(o => o.Id);
            }
            switch (baseSearchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.New:
                    {
                        model = model.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Old:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }

                case Common.Enumerable.SortEnum.MoreSell:
                    {
                        model = model.OrderByDescending(s => s.RateAvg).ThenByDescending(ad => ad.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.LessSell:
                    {
                        model = model.OrderBy(s => s.RateAvg).ThenByDescending(ad => ad.Id);
                        break;
                    }
                default:
                    break;
            }

            return new StoreSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<StoreDto>> InsertAsyncDto(StoreDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<StoreDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    if ((!NameIsUnique(dto.Name)))
                    {
                        return new BaseResultDto<StoreDto>(isSuccess: false, val1: Resource.Notification.TheNameIsDuplicate, val2: nameof(dto.Name), dto);
                    }
                    var item = mapper.Map<Store>(dto);
                    item.CreateDate = DateTime.Now;
                    await _context.Stores.AddAsync(item);
                    _context.SaveChanges();
                    return new BaseResultDto<StoreDto>(true, mapper.Map<StoreDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<StoreDto>(isSuccess: false, val: ex.Message, data: dto);
            }


        }

        public override BaseResultDto UpdateDto(StoreDto dto)
        {

            try
            {
                var modelCheker = ModelHelper<StoreDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = _context.Stores.FirstOrDefault(s => s.Id == dto.Id);
                    if (dto.Name != item.Name && (!NameIsUnique(dto.Name)))
                    {
                        return new BaseResultDto<StoreDto>(isSuccess: false, val1: Resource.Notification.TheNameIsDuplicate, val2: nameof(dto.Name), dto);
                    }
                    bool updateProducts = false;
                    if (dto.Active != item.Active)
                    {
                        updateProducts = true;
                    }
                    mapper.Map(dto, item);
                    _context.Stores.Update(item);
                    _context.SaveChanges();
                    if (updateProducts)
                    {
                        _productService.UpdateProductPriceAsync(Common.Enumerable.ProductUpdateTypeEnum.Store, dto.Id.ToString());
                    }
                    return new BaseResultDto<StoreDto>(true, mapper.Map<StoreDto>(item));
                }
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }

        }
        public override BaseResultDto DeleteDto(long id)
        {

            var del = base.DeleteDto(id);
            _productService.UpdateProductPriceAsync(Common.Enumerable.ProductUpdateTypeEnum.Store, id.ToString());
            return del;
        }
        bool NameIsUnique(string name)
        {
            var item = _context.Stores.FirstOrDefault(x => x.Name == name);
            if (item == null)
                return true;
            return false;
        }
        public async Task SetMaxDiscountAsync(long storeId, int maxDiscount)
        {
            var item = await _context.Stores.AsTracking().FirstOrDefaultAsync(s => s.Id == storeId);
            if (item == null) return;
            item.MaxDiscountPercent = maxDiscount;
            _context.Stores.Update(item);
            await _context.SaveChangesAsync();
        }
        public void UpdateStoreCommentCount(long storeId)
        {
            var item = _context.Stores.Include(s => s.StoreComments).ThenInclude(s => s.Status).AsTracking().FirstOrDefault(s => s.Id == storeId);
            item.CommentCount = item.StoreComments.Count(c => c.Status.Label == CommentEnum.Comment_Accept.ToString());
            _context.Stores.Update(item);
            _context.SaveChanges();
        }
        public async Task UpdateStoreCommentRateAsync(long Id)
        {
            var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("UpdateStoreCommentsRate", new { FilterIds = Id }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<List<SearchStoreDto>> SearchMinAsync(SearchRequestDto request)
        {
            var q = request.Q;
            return await _context.Stores.Where(s => !s.Deleted && s.Active && (s.Name.Contains(q))).OrderByDescending(s => s.RateAvg).Take(request.StoreCount).ProjectTo<SearchStoreDto>(mapper.ConfigurationProvider).ToListAsync();
        }

    }

}

