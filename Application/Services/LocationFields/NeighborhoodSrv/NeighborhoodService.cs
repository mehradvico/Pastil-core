using Application.Common.Service;
using Application.Services.LocationFields.LocationSrv.Dto;
using Application.Services.LocationFields.NeighborhoodSrv.Dto;
using Application.Services.LocationFields.NeighborhoodSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.NeighborhoodSrv
{
    public class NeighborhoodService : CommonSrv<Neighborhood, NeighborhoodDto>, INeighborhoodService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;
        public NeighborhoodService(IDataBaseContext context, IMapper mapper) : base(context, mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }



        public NeighborhoodSearchDto Search(NeighborhoodInputDto inputdto)
        {
            var query = _context.Neighborhoods.Include(s => s.City).ThenInclude(s => s.State).ThenInclude(s => s.Country).AsQueryable();

            if (!string.IsNullOrWhiteSpace(inputdto.Q))
            {
                var q = inputdto.Q.Trim();
                query = query.Where(s => s.Name.Contains(q));
            }

            if (inputdto.CityId.HasValue && inputdto.CityId.Value > 0)
            {
                query = query.Where(s => s.CityId == inputdto.CityId.Value);
            }

            if (inputdto.StateId.HasValue && inputdto.StateId.Value > 0)
            {
                query = query.Where(s => s.City.StateId == inputdto.StateId.Value);
            }

            return new NeighborhoodSearchDto(inputdto, query, _mapper);
        }

        public async Task<LocationBoundaryVDto> FindBoundaryAsync(long id)
        {
            return await _context.Neighborhoods.AsNoTracking().Where(x => x.Id == id).Select(x => new LocationBoundaryVDto { Id = x.Id, Name = x.Name, Boundary = x.Boundary }).FirstOrDefaultAsync();
        }
    }
}
