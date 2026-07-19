using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.LocationFields.CitySrv.Dto;
using Application.Services.LocationFields.CitySrv.Iface;
using Application.Services.LocationFields.LocationSrv.Dto;
using AutoMapper;
using Entities.Entities.LocationField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var model = _context.Cities.Include(s => s.State).ThenInclude(s => s.Country).AsQueryable();

            if (baseSearchDto.StateId.HasValue && baseSearchDto.StateId.Value > 0)
            {
                model = model.Where(s => s.StateId == baseSearchDto.StateId.Value);
            }

            if (!string.IsNullOrWhiteSpace(baseSearchDto.Q))
            {
                var q = baseSearchDto.Q.Trim();

                model = model.Where(s =>s.Name.Contains(q) || s.State.Name.Contains(q) || s.State.Country.Name.Contains(q));
            }

            return new BaseSearchDto<City, CityVDto>(baseSearchDto, model, mapper);
        }
        public BaseResultDto GetAll()
        {
            var model = _context.Cities.Include(s => s.State).AsQueryable();

            return new BaseResultDto<List<CityStateVDto>>(true, data: mapper.Map<List<CityStateVDto>>(model));
        }
        public async Task<LocationBoundaryVDto> FindBoundaryAsync(long id)
        {
            return await _context.Cities.AsNoTracking().Where(x => x.Id == id).Select(x => new LocationBoundaryVDto{Id = x.Id, Name = x.Name, Boundary = x.Boundary}).FirstOrDefaultAsync();
        }
    }
}
