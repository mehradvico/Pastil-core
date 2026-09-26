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
                return new BaseResultDto<PriceCalculationDto>(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), data: dto);
            }
        }

        public async Task<PriceCalculationVDto> GetForNow()
        {
            return await GetForHour(DateTime.Now.Hour);
        }
        public async Task<PriceCalculationVDto> GetForHour(int hour)
        {
            var item = await _context.PriceCalculations.FirstOrDefaultAsync(s => s.Deleted == false && s.FromTime <= hour && s.ToTime >= hour);
            return mapper.Map<PriceCalculationVDto>(item);
        }
        // مبلغ پایه‌ی هر درخواست پت‌رسان (تومان): بدون توجه به مسافت، این مبلغ اول حساب می‌شود و
        // هزینه‌ی متری، رفت‌وبرگشت، توقف، گزینه‌ها، پت اضافه و وانت روی آن اضافه می‌شود.
        public const double RequestBasePrice = 500_000;
        public const double DefaultExtraPetPrice = 100_000;

        public async Task<double> CalculateTripPrice(TripDto tripDto)
        {
            // برای سفرهای زمان‌بندی‌شده (رزرو پت‌رسان)، نرخ باید بر اساس ساعتِ حرکتِ واقعی سفر
            // محاسبه بشه، نه ساعتِ لحظه‌ی ثبت درخواست — وگرنه یه رزرو نیمه‌شب برای فردا صبح با
            // نرخ نیمه‌شب حساب می‌شه.
            var referenceHour = (tripDto.TripStartDateTime ?? DateTime.Now).Hour;
            var priceCalculation = await GetForHour(referenceHour);
            if (priceCalculation == null)
            {
                return 0;
            }
            double price = 0;
            var distanceMeter = await _geographyService.GetDrivingDistanceAsync(tripDto.Origin, tripDto.Destination, false, true);
            if (tripDto.SecondDestination != null && tripDto.SecondDestination.x > 0)
                distanceMeter += await _geographyService.GetDrivingDistanceAsync(tripDto.Destination, tripDto.SecondDestination, false, true);
            // رفت‌وبرگشت دیگر مسافت را دوبرابر نمی‌کند؛ در انتها نصف «کل هزینه» به مبلغ اضافه می‌شود.
            price = distanceMeter * priceCalculation.Price;
            if (tripDto.TripStopId.HasValue)
            {
                var tripStop = await _tripStopService.FindAsync(tripDto.TripStopId.Value);
                if (tripStop != null)
                {
                    price += tripStop.Price;
                }
            }
            // توقف در مسیر: تعرفه‌ی «زمان انتظار ۵ دقیقه‌ای» از پنل (/admin/tripstop) خوانده می‌شود و
            // فرانت ۵ دقیقه‌ـ۵ دقیقه اضافه می‌کند؛ ۶۰ دقیقه = ۱۲ × قیمت آن تعرفه.
            // اگر تعرفه‌ای تعریف نشده بود، به PriceCalculation.StopPrice (قیمت هر ۵ دقیقه) برمی‌گردد.
            if (tripDto.StopMinutes.HasValue && tripDto.StopMinutes.Value > 0)
            {
                var unit = await _context.TripStops.AsNoTracking()
                    .Where(t => !t.Deleted && t.Active)
                    .OrderBy(t => t.Id)
                    .Select(t => (double?)t.Price)
                    .FirstOrDefaultAsync() ?? priceCalculation.StopPrice;
                price += Math.Ceiling(tripDto.StopMinutes.Value / 5.0) * unit;
            }
            double optionsTotal = 0;
            if (tripDto.TripOptionIds != null && tripDto.TripOptionIds.Any())
            {
                var optionList = await _tripOptionService.GetListAsync(tripDto.TripOptionIds);
                foreach (var tripOption in optionList)
                {
                    optionsTotal += tripOption.Price;
                }
            }
            price += optionsTotal;

            // چند پت در یک سفر: پت اول رایگان است، از پت دوم به بعد هر پت اضافه یک‌بار ExtraPetPrice اضافه می‌کند.
            var petIds = (tripDto.UserPetIds != null && tripDto.UserPetIds.Any())
                ? tripDto.UserPetIds.Distinct().ToList()
                : (tripDto.UserPetId.HasValue ? new System.Collections.Generic.List<long> { tripDto.UserPetId.Value } : new System.Collections.Generic.List<long>());
            var extraPetCount = System.Math.Max(0, petIds.Count - 1);
            if (extraPetCount > 0)
            {
                // هر پت اضافه: مبلغ ثابت پت اضافه (اگر در ردیف نرخ صفر باشد ۱۰۰٬۰۰۰ تومان) + نصفِ هزینه‌ی گزینه‌های
                // انتخاب‌شده‌ی سفر که دوباره برای همان پت اعمال می‌شود.
                var extraPetFee = priceCalculation.ExtraPetPrice > 0 ? priceCalculation.ExtraPetPrice : DefaultExtraPetPrice;
                price += extraPetCount * (extraPetFee + optionsTotal / 2);
            }

            // نوع خودرو: فقط وقتی کاربر صراحتاً «وانت» را انتخاب کرده (نه «فرقی نداره»)، مبلغ ثابت اضافه می‌شود.
            if (tripDto.VehicleTypeId.HasValue)
            {
                var vehicleTypeName = await _context.Codes.AsNoTracking()
                    .Where(c => c.Id == tripDto.VehicleTypeId.Value)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync();
                if (vehicleTypeName != null && vehicleTypeName.Contains("وانت"))
                    price += priceCalculation.PickupVehicleExtraPrice;
            }

            // مبلغ پایه‌ی درخواست، جدا از هزینه‌های محاسبه‌شده‌ی بالا
            var total = RequestBasePrice + price;
            // رفت‌وبرگشت: نصف کل هزینه‌ی سفر (مبلغ پایه + همه‌ی هزینه‌ها) به مبلغ کل اضافه می‌شود.
            // مثال: ۵۰۰٬۰۰۰ + ۶۰۰٬۰۰۰ = ۱٬۱۰۰٬۰۰۰ ← با رفت‌وبرگشت: ۱٬۶۵۰٬۰۰۰
            if (tripDto.RoundTrip)
                total += total / 2;
            return total;

        }
    }
}
