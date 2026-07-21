using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CompanionSrvs.CompanionZoneSrv.Dto;
using Application.Services.CompanionSrvs.CompanionZoneSrv.Iface;
using AutoMapper;
using Entities.Entities.CompanionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionZoneSrv
{
    public class CompanionZoneService : CommonSrv<CompanionZone, CompanionZoneDto>, ICompanionZoneService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper _mapper;

        public CompanionZoneService(IDataBaseContext context, IMapper mapper) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public override async Task<BaseResultDto<CompanionZoneDto>> FindAsyncDto(long id)
        {
            var item = await ZoneQuery().FirstOrDefaultAsync(x => x.Id == id);
            return new BaseResultDto<CompanionZoneDto>(item != null, _mapper.Map<CompanionZoneDto>(item));
        }

        public async Task<BaseResultDto<CompanionZoneDto>> FindForCompanionAsync(long id, long companionId)
        {
            var item = await ZoneQuery().FirstOrDefaultAsync(x => x.Id == id && x.CompanionId == companionId);
            return new BaseResultDto<CompanionZoneDto>(item != null, _mapper.Map<CompanionZoneDto>(item));
        }

        public CompanionZoneSearchDto Search(CompanionZoneInputDto dto)
        {
            var model = ZoneQuery();

            if (dto.CompanionId.HasValue)
                model = model.Where(x => x.CompanionId == dto.CompanionId.Value);

            if (dto.NeighborhoodId.HasValue)
                model = model.Where(x => x.NeighborhoodId == dto.NeighborhoodId.Value);

            if (dto.CityId.HasValue)
                model = model.Where(x => x.CityId == dto.CityId.Value);

            if (dto.StateId.HasValue)
                model = model.Where(x => x.StateId == dto.StateId.Value);

            model = model
                .OrderBy(x => x.City.Name)
                .ThenBy(x => x.NeighborhoodId.HasValue)
                .ThenBy(x => x.Neighborhood.Name);

            return new CompanionZoneSearchDto(dto, model, _mapper);
        }

        public override Task<BaseResultDto<CompanionZoneDto>> InsertAsyncDto(CompanionZoneDto dto)
        {
            dto.Id = 0;
            return SaveAsyncDto(dto);
        }

        public async Task<BaseResultDto<CompanionZoneDto>> UpdateAsyncDto(CompanionZoneDto dto, long? companionId = null)
        {
            if (dto.Id <= 0)
                return new BaseResultDto<CompanionZoneDto>(false, Resource.Notification.InvalidData, dto);

            return await SaveAsyncDto(dto, companionId);
        }

        public override BaseResultDto UpdateDto(CompanionZoneDto dto)
        {
            return UpdateAsyncDto(dto).GetAwaiter().GetResult();
        }

        public async Task<BaseResultDto> DeleteAsync(long id, long? companionId = null)
        {
            var item = await _context.CompanionZones
                .AsTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.Deleted &&
                    (!companionId.HasValue || x.CompanionId == companionId.Value));

            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            item.Deleted = true;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        private async Task<BaseResultDto<CompanionZoneDto>> SaveAsyncDto(CompanionZoneDto dto, long? requiredCompanionId = null)
        {
            try
            {
                if (requiredCompanionId.HasValue)
                    dto.CompanionId = requiredCompanionId.Value;

                var modelChecker = ModelHelper<CompanionZoneDto>.ModelErrors(dto);
                if (!modelChecker.IsSuccess)
                    return modelChecker;

                var companionExists = await _context.Companions
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == dto.CompanionId && !x.Deleted);
                if (!companionExists)
                    return new BaseResultDto<CompanionZoneDto>(false, Resource.Notification.NothingFound, dto);

                var city = await _context.Cities
                    .AsNoTracking()
                    .Where(x => x.Id == dto.CityId)
                    .Select(x => new { x.Id, x.StateId })
                    .FirstOrDefaultAsync();
                if (city == null)
                    return new BaseResultDto<CompanionZoneDto>(false, Resource.Notification.InvalidData, dto);

                if (dto.NeighborhoodId.HasValue)
                {
                    var neighborhoodIsValid = await _context.Neighborhoods
                        .AsNoTracking()
                        .AnyAsync(x => x.Id == dto.NeighborhoodId.Value && x.CityId == dto.CityId);
                    if (!neighborhoodIsValid)
                        return new BaseResultDto<CompanionZoneDto>(false, Resource.Notification.InvalidData, dto);
                }

                var currentItem = dto.Id > 0
                    ? await _context.CompanionZones.AsTracking().FirstOrDefaultAsync(x => x.Id == dto.Id && !x.Deleted)
                    : null;

                if (dto.Id > 0 && (currentItem == null ||
                    (requiredCompanionId.HasValue && currentItem.CompanionId != requiredCompanionId.Value)))
                    return new BaseResultDto<CompanionZoneDto>(false, Resource.Notification.NothingFound, dto);

                var sameCityZones = await _context.CompanionZones
                    .AsTracking()
                    .Where(x => !x.Deleted &&
                        x.Id != dto.Id &&
                        x.CompanionId == dto.CompanionId &&
                        x.CityId == dto.CityId)
                    .ToListAsync();

                var exactDuplicate = sameCityZones.Any(x => x.NeighborhoodId == dto.NeighborhoodId);
                if (exactDuplicate)
                    return new BaseResultDto<CompanionZoneDto>(false, Resource.Notification.DuplicateValue, dto);

                if (dto.NeighborhoodId.HasValue && sameCityZones.Any(x => !x.NeighborhoodId.HasValue))
                    return new BaseResultDto<CompanionZoneDto>(false, Resource.Notification.DuplicateValue, dto);

                if (!dto.NeighborhoodId.HasValue)
                {
                    foreach (var zone in sameCityZones)
                        zone.Deleted = true;
                }

                if (currentItem == null)
                {
                    currentItem = new CompanionZone();
                    await _context.CompanionZones.AddAsync(currentItem);
                }

                currentItem.CompanionId = dto.CompanionId;
                currentItem.StateId = city.StateId;
                currentItem.CityId = dto.CityId;
                currentItem.NeighborhoodId = dto.NeighborhoodId;
                currentItem.Deleted = false;

                await _context.SaveChangesAsync();
                return new BaseResultDto<CompanionZoneDto>(true, _mapper.Map<CompanionZoneDto>(currentItem));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionZoneDto>(false, ex.Message, dto);
            }
        }

        private IQueryable<CompanionZone> ZoneQuery()
        {
            return _context.CompanionZones
                .AsNoTracking()
                .Include(x => x.Neighborhood)
                .Include(x => x.City).ThenInclude(x => x.State)
                .Where(x => !x.Deleted);
        }
    }
}
