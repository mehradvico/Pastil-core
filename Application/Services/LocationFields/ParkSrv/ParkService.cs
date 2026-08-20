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
                .Include(s => s.ParkPictures.Where(p => !p.Deleted)).ThenInclude(s => s.Picture).AsQueryable();

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

        public override BaseResultDto UpdateDto(ParkDto dto)
        {
            try
            {
                var park = _context.Parks.FirstOrDefault(s => s.Id == dto.Id);
                if (park == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);

                if (dto.PictureId.HasValue && !_context.Pictures.Any(s => s.Id == dto.PictureId.Value))
                    return new BaseResultDto(false, Resource.Notification.NothingFound);

                var isMainPictureOnlyUpdate =
                    string.IsNullOrWhiteSpace(dto.Name) &&
                    dto.NeighborhoodId <= 0 &&
                    dto.Location == null &&
                    dto.AddressValue == null;

                if (isMainPictureOnlyUpdate)
                {
                    park.PictureId = dto.PictureId;
                    _context.SaveChanges();
                    return new BaseResultDto(true);
                }

                if (!_context.Neighborhoods.Any(s => s.Id == dto.NeighborhoodId))
                    return new BaseResultDto(false, Resource.Notification.NothingFound);

                park.Name = dto.Name;
                park.NeighborhoodId = dto.NeighborhoodId;
                park.Suggested = dto.Suggested;
                park.PictureId = dto.PictureId;
                park.AddressValue = dto.AddressValue;
                park.Location = mapper.Map<NetTopologySuite.Geometries.Point>(dto.Location);

                _context.SaveChanges();
                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

        public async Task<BaseResultDto> UpdateMainPictureAsync(ParkMainPictureDto dto)
        {
            try
            {
                var park = await _context.Parks.FirstOrDefaultAsync(s => s.Id == dto.Id);
                if (park == null)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);

                if (dto.PictureId.HasValue &&
                    !await _context.Pictures.AnyAsync(s => s.Id == dto.PictureId.Value))
                {
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                }

                park.PictureId = dto.PictureId;
                await _context.SaveChangesAsync();
                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }
    }
}
