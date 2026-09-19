using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.Setting.MapKeyService.Dto;
using Application.Services.Setting.MapKeyService.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Setting.MapKeyService
{
    public class MapKeyService : CommonSrv<MapKey, MapKeyDto>, IMapKeyService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ICodeService _codeService;
        public MapKeyService(IDataBaseContext _context, IMapper mapper, ICodeService codeService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._codeService = codeService;
        }

        public async Task<BaseResultDto<MapKeyVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.MapKeys.Include(s => s.Type).FirstOrDefaultAsync(s => s.Id == id && s.Deleted != true);
            if (item != null)
            {
                return new BaseResultDto<MapKeyVDto>(true, mapper.Map<MapKeyVDto>(item));
            }
            return new BaseResultDto<MapKeyVDto>(false, mapper.Map<MapKeyVDto>(item));
        }

        public MapKeySearchDto Search(MapKeyInputDto baseSearchDto)
        {
            var model = _context.MapKeys.Include(s => s.Type).AsQueryable().Where(s => s.Deleted != true);

            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Available.Value);
            }
            if (baseSearchDto.TypeId.HasValue)
            {
                model = model.Where(s => s.TypeId == baseSearchDto.TypeId.Value);
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
                default:
                    break;
            }
            return new MapKeySearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<MapKeyDto>> InsertAsyncDto(MapKeyDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<MapKeyDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var mapType = await _codeService.GetIdByLabelAsync(GeoGraphyMapEnum.MapType_MapIr.ToString());
                    dto.TypeId = mapType;
                    var item = mapper.Map<MapKey>(dto);
                    await _context.MapKeys.AddAsync(item);
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<MapKeyDto>(true, mapper.Map<MapKeyDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<MapKeyDto>(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), data: dto);
            }
        }
    }
}
