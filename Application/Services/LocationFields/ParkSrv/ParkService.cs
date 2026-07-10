using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.LocationFields.ParkSrv.Dto;
using Application.Services.LocationFields.ParkSrv.Iface;
using AutoMapper;
using Entities.Entities.LocationField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.ParkSrv
{
    public class ParkService : CommonSrv<Park, ParkDto>, IParkService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        public ParkService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }


        public BaseSearchDto<ParkVDto> Search(ParkInputDto baseSearchDto)
        {
            var model = _context.Parks.Include(s => s.Picture).Include(s => s.Neighborhood).ThenInclude(s => s.City).ThenInclude(s => s.State)
                .Include(s => s.ParkPictures).ThenInclude(s => s.Picture).AsQueryable();

            if (baseSearchDto.NeighborhoodId.HasValue && baseSearchDto.NeighborhoodId.Value > 0)
            {
                model = model.Where(s => s.NeighborhoodId == baseSearchDto.NeighborhoodId.Value);
            }

            if (baseSearchDto.CityId.HasValue && baseSearchDto.CityId.Value > 0)
            {
                model = model.Where(s => s.Neighborhood.CityId == baseSearchDto.CityId.Value);
            }

            if (baseSearchDto.StateId.HasValue && baseSearchDto.StateId.Value > 0)
            {
                model = model.Where(s => s.Neighborhood.City.StateId == baseSearchDto.StateId.Value);
            }

            if (!string.IsNullOrWhiteSpace(baseSearchDto.Q))
            {
                var q = baseSearchDto.Q.Trim();
                model = model.Where(s => s.Name.Contains(q));
            }

            return new BaseSearchDto<Park, ParkVDto>(baseSearchDto, model, mapper);
        }
    }
}
