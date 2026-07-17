using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CompanionSrvs.CompanionZoneSrv.Dto;
using Application.Services.CompanionSrvs.CompanionZoneSrv.Iface;
using Application.Services.Setting.CodeSrv.Iface;
using Application.Services.Setting.NoticeSrv.Iface;
using AutoMapper;
using Entities.Entities.CompanionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionZoneSrv
{
    public class CompanionZoneService : CommonSrv<CompanionZone, CompanionZoneDto>, ICompanionZoneService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public CompanionZoneService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public override async Task<BaseResultDto<CompanionZoneDto>> FindAsyncDto(long id)
        {
            var item = await _context.CompanionZones.Include(p => p.Neighborhood).Include(s => s.City).ThenInclude(s => s.State).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
                return new BaseResultDto<CompanionZoneDto>(true, mapper.Map<CompanionZoneDto>(item));
            return new BaseResultDto<CompanionZoneDto>(false, mapper.Map<CompanionZoneDto>(item));
        }

        public CompanionZoneSearchDto Search(CompanionZoneInputDto dto)
        {

            var model = _context.CompanionZones.Include(p => p.Neighborhood).Include(s => s.City).ThenInclude(s => s.State).AsQueryable().Where(s => !s.Deleted);

            if (dto.CompanionId.HasValue)
            {
                model = model.Where(s => s.CompanionId == dto.CompanionId);
            }
            if (dto.NeighborhoodId.HasValue)
            {
                model = model.Where(s => s.NeighborhoodId == dto.NeighborhoodId.Value);
            }
            if (dto.CityId.HasValue)
            {
                model = model.Where(s => s.CityId == dto.CityId.Value);
            }
            if (dto.StateId.HasValue)
            {
                model = model.Where(s => s.City.StateId == dto.StateId.Value);
            }

            return new CompanionZoneSearchDto(dto, model, mapper);

        }

        public override async Task<BaseResultDto<CompanionZoneDto>> InsertAsyncDto(CompanionZoneDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionZoneDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<CompanionZone>(dto);
                    var city = await _context.Cities.FirstOrDefaultAsync(s => s.Id == dto.CityId);
                    item.StateId = city.StateId;
                    await _context.CompanionZones.AddAsync(item);
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<CompanionZoneDto>(true, mapper.Map<CompanionZoneDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionZoneDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }
    }
}
