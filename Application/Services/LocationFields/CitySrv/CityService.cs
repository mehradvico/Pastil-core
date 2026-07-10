using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.LocationFields.CitySrv.Dto;
using Application.Services.LocationFields.CitySrv.Iface;
using AutoMapper;
using Entities.Entities.LocationField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.LocationFields.CitySrv
{
    public class CityService : CommonSrv<City, CityDto>, ICityService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        public CityService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }


        public BaseSearchDto<CityVDto> Search(CityInputDto baseSearchDto)
        {
            var model = _context.Cities.Where(s => s.StateId == baseSearchDto.StateId).AsQueryable();
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                model = model.Where(s => s.Name.Contains(baseSearchDto.Q)).OrderBy(o => o.Name);
            }
            return new BaseSearchDto<City, CityVDto>(baseSearchDto, model, mapper);
        }
        public BaseResultDto GetAll()
        {
            var model = _context.Cities.Include(s => s.State).AsQueryable();

            return new BaseResultDto<List<CityStateVDto>>(true, data: mapper.Map<List<CityStateVDto>>(model));
        }

    }
}
