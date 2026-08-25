using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Geography.Iface;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.TripSrv.PriceCalculationSrv.Dto;
using Application.Services.TripSrv.PriceCalculationSrv.Iface;
using Application.Services.TripSrv.TripOptionSrv.Iface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.TripSrv.TripStopSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.TripSrv.PriceCalculationSrv
{
    public class PriceCalculationService : CommonSrv<PriceCalculation, PriceCalculationDto>, IPriceCalculationService
    {
        private readonly IGeographyService _geographyService;
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ITripStopService _tripStopService;
        private readonly ITripOptionService _tripOptionService;
        public PriceCalculationService(IDataBaseContext _context, IMapper mapper, ITripStopService tripStopService, ITripOptionService tripOptionService, IGeographyService geographyService) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            _geographyService = geographyService;
            _tripStopService = tripStopService;
            _tripOptionService = tripOptionService;
        }
        public async Task<BaseResultDto<PriceCalculationVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.PriceCalculations.FirstOrDefaultAsync(s => s.Id == id && s.Deleted == false);
            if (item != null)
            {
                return new BaseResultDto<PriceCalculationVDto>(true, mapper.Map<PriceCalculationVDto>(item));
            }
            return new BaseResultDto<PriceCalculationVDto>(false, mapper.Map<PriceCalculationVDto>(item));
        }

        public PriceCalculationSearchDto Search(PriceCalculationInputDto baseSearchDto)
        {
            var model = _context.PriceCalculations.AsQueryable().Where(s => s.Deleted == false);

            switch (baseSearchDto.SortBy)
            {
                case SortEnum.New:
                    {
                        model = model.OrderByDescending(s => s.Id);
                        break;
                    }
                case SortEnum.Old:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }
                default:
                    break;
            }
            return new PriceCalculationSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<PriceCalculationDto>> InsertAsyncDto(PriceCalculationDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<PriceCalculationDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    if (dto.FromTime >= dto.ToTime)
                    {
                        return new BaseResultDto<PriceCalculationDto>(false, Resource.Notification.ToTimeMustBeBiggerThanFromTime, dto);
                    }

                    bool isOverlapping = await _context.PriceCalculations.AnyAsync(pc => !(dto.ToTime <= pc.FromTime || dto.FromTime >= pc.ToTime));

                    if (isOverlapping)
                    {
                        return new BaseResultDto<PriceCalculationDto>(false, Resource.Notification.TimesHaveOverlap, dto);
                    }
                    var item = mapper.Map<PriceCalculation>(dto);

                    await _context.PriceCalculations.AddAsync(item);
                    await _context.SaveChangesAsync();
                    return new BaseResultDto<PriceCalculationDto>(true, mapper.Map<PriceCalculationDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<PriceCalculationDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }

        public async Task<PriceCalculationVDto> GetForNow()
        {
            var hour = DateTime.Now.Hour;
            var item = await _context.PriceCalculations.FirstOrDefaultAsync(s => s.Deleted == false && s.FromTime <= hour && s.ToTime >= hour);
            return mapper.Map<PriceCalculationVDto>(item);
        }
        public async Task<double> CalculateTripPrice(TripDto tripDto)
        {
            var priceCalculation = await GetForNow();
            if (priceCalculation == null)
            {
                return 0;
            }
            double price = 0;
            var distanceMeter = await _geographyService.GetDrivingDistanceAsync(tripDto.Origin, tripDto.Destination, false, true);
            if (tripDto.SecondDestination != null && tripDto.SecondDestination.x > 0)
                distanceMeter += await _geographyService.GetDrivingDistanceAsync(tripDto.Destination, tripDto.SecondDestination, false, true);
            if (tripDto.RoundTrip)
                distanceMeter = distanceMeter * 2;
            price = distanceMeter * priceCalculation.Price;
            if (tripDto.TripStopId.HasValue)
            {
                var tripStop = await _tripStopService.FindAsync(tripDto.TripStopId.Value);
                if (tripStop != null)
                {
                    price += tripStop.Price;
                }
            }
            if (tripDto.TripOptionIds != null && tripDto.TripOptionIds.Any())
            {
                var optionList = await _tripOptionService.GetListAsync(tripDto.TripOptionIds);
                foreach (var tripOption in optionList)
                {
                    price += tripOption.Price;
                }
            }
            return price;

        }
    }
}
