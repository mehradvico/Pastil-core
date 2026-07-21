using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.LocationFields.UserCurrentLocationSrv.Dto;
using Application.Services.LocationFields.UserCurrentLocationSrv.Iface;
using AutoMapper;
using Entities.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.LocationFields.UserCurrentLocationSrv
{
    public class UserCurrentLocationService : CommonSrv<UserCurrentLocation, UserCurrentLocationDto>, IUserCurrentLocationService
    {
        private const double MaximumCityFallbackDistanceMeters = 2000;
        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;

        public UserCurrentLocationService(IDataBaseContext context, IMapper mapper) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public UserCurrentLocationSearchDto Search(UserCurrentLocationInputDto inputDto)
        {
            var query = _context.UserCurrentLocations.AsNoTracking().Include(x => x.User).Include(x => x.City).Include(x => x.Neighborhood).AsQueryable();

            if (inputDto.UserId.HasValue)
                query = query.Where(x => x.UserId == inputDto.UserId.Value);

            if (inputDto.CityId.HasValue)
                query = query.Where(x => x.CityId == inputDto.CityId.Value);

            if (inputDto.NeighborhoodId.HasValue)
                query = query.Where(x => x.NeighborhoodId == inputDto.NeighborhoodId.Value);

            return new UserCurrentLocationSearchDto(inputDto, query, _mapper);
        }

        public async Task<BaseResultDto<UserCurrentLocationDto>> SetAsyncDto(long userId, SetUserCurrentLocationDto dto)
        {
            try
            {
                var modelChecker = ModelHelper<SetUserCurrentLocationDto>.ModelErrors(dto);
                if (!modelChecker.IsSuccess)
                    return new BaseResultDto<UserCurrentLocationDto>(false, modelChecker.Messages, null, modelChecker.Code);

                if (!double.IsFinite(dto.Location.x) || !double.IsFinite(dto.Location.y))
                    return new BaseResultDto<UserCurrentLocationDto>(false, Resource.Notification.InvalidData, null);

                var point = _mapper.Map<NetTopologySuite.Geometries.Point>(dto.Location);
                var neighborhood = await _context.Neighborhoods
                    .AsNoTracking()
                    .Where(x => x.Boundary != null && x.Boundary.Intersects(point))
                    .Select(x => new { x.Id, x.CityId })
                    .FirstOrDefaultAsync();

                long cityId;
                long? neighborhoodId = null;
                if (neighborhood != null)
                {
                    cityId = neighborhood.CityId;
                    neighborhoodId = neighborhood.Id;
                }
                else
                {
                    var city = await _context.Cities
                        .AsNoTracking()
                        .Where(x => x.Boundary != null && x.Boundary.Intersects(point))
                        .Select(x => new { x.Id })
                        .FirstOrDefaultAsync();

                    if (city != null)
                    {
                        cityId = city.Id;
                    }
                    else
                    {
                        var nearestNeighborhood = await _context.Neighborhoods
                            .AsNoTracking()
                            .Where(x => x.Boundary != null)
                            .Select(x => new
                            {
                                x.CityId,
                                Distance = x.Boundary.Distance(point)
                            })
                            .OrderBy(x => x.Distance)
                            .FirstOrDefaultAsync();

                        if (nearestNeighborhood == null || nearestNeighborhood.Distance > MaximumCityFallbackDistanceMeters)
                            return new BaseResultDto<UserCurrentLocationDto>(false, Resource.Notification.NothingFound, null);

                        cityId = nearestNeighborhood.CityId;
                    }
                }

                var location = await _context.UserCurrentLocations
                    .AsTracking()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (location == null)
                {
                    location = new UserCurrentLocation
                    {
                        UserId = userId,
                        Location = point,
                        CityId = cityId,
                        NeighborhoodId = neighborhoodId,
                        LastUpdateDate = DateTime.UtcNow
                    };
                    await _context.UserCurrentLocations.AddAsync(location);
                }
                else
                {
                    location.Location = point;
                    location.CityId = cityId;
                    location.NeighborhoodId = neighborhoodId;
                    location.LastUpdateDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return new BaseResultDto<UserCurrentLocationDto>(true, _mapper.Map<UserCurrentLocationDto>(location));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<UserCurrentLocationDto>(false, ex.Message, null);
            }
        }
    }
}
