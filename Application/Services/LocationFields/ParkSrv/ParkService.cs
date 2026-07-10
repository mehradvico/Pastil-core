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
            var model = _context.Parks.Include(s => s.Picture).Where(s => s.NeighborhoodId == baseSearchDto.NeighborhoodId).AsQueryable();
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                model = model.Where(s => s.Name.Contains(baseSearchDto.Q)).OrderBy(o => o.Name);
            }
            return new BaseSearchDto<Park, ParkVDto>(baseSearchDto, model, mapper);
        }
        public BaseResultDto GetAll()
        {
            var model = _context.Parks.Include(s => s.Picture).Include(s => s.Neighborhood).AsQueryable();

            return new BaseResultDto<List<ParkNeighborhoodVDto>>(true, data: mapper.Map<List<ParkNeighborhoodVDto>>(model));
        }

    }
}
