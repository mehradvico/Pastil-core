using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Services.TripSrv.PetResanServiceSrv.Dto;
using Application.Services.TripSrv.PetResanServiceSrv.Iface;
using Application.Services.TripSrv.PriceCalculationSrv.Iface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.WeekDaySrv.WeekDaySrv.Iface;
using Entities.Entities.PetResanServiceField;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.TripSrv.PetResanServiceSrv
{
    public class PetResanServiceService : IPetResanServiceService
    {
        private readonly IDataBaseContext _context;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IWeekDayService _weekDayService;
        private readonly ILogger<PetResanServiceService> _logger;

        public PetResanServiceService(
            IDataBaseContext context,
            IPriceCalculationService priceCalculationService,
            IWeekDayService weekDayService,
            ILogger<PetResanServiceService> logger)
        {
            _context = context;
            _priceCalculationService = priceCalculationService;
            _weekDayService = weekDayService;
            _logger = logger;
        }

        public async Task<BaseResultDto<double>> PreviewPriceAsync(PetResanServiceCreateDto dto)
        {
            if (dto.Origin == null)
                return new BaseResultDto<double>(false, Resource.Notification.PleaseSetOrigin, 0);
            if (dto.Destination == null)
                return new BaseResultDto<double>(false, Resource.Notification.PleaseSetDestination, 0);

            try
            {
                var (total, _) = await CalculateWeeklyTotalPriceAsync(dto);
                return new BaseResultDto<double>(true, total);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "PetResan price preview could not be calculated.");
                return new BaseResultDto<double>(false, Resource.Notification.TripPriceCalculationFailed, 0);
            }
        }

        // تعداد ردیف‌های یکتای هفته: ترکیب‌های یکتای (روز هفته، ساعتِ رفت). یک ردیف با ReturnTime هم همچنان
        // یک «ردیف/نوبت» حساب می‌شود (برای PricePerOccurrence نمایشی)، حتی اگر واقعاً دو Trip بسازد.
        private static int CountWeeklyOccurrences(List<PetResanServiceScheduleDto> schedules) =>
            (schedules ?? new List<PetResanServiceScheduleDto>()).Select(s => $"{s.WeekDayId}:{s.Time}").Distinct().Count();

        // مبلغ هفتگی: قیمت هر نوبت باید با نرخِ همان ساعتِ نوبت حساب شود (شب/روز نرخ فرق دارد)، نه با نرخ لحظه‌ی
        // درخواست. به تفکیک هر ساعتِ یکتا (نه هر (روز، ساعت)) یک‌بار قیمت گرفته می‌شود — چون در
        // PriceCalculationService.CalculateTripPrice فقط ساعت اثر دارد نه روز هفته — و حاصل‌جمع نوبت‌ها برگردانده می‌شود.
        //
        // اگر یک ردیف ReturnTime داشته باشد، «رفت‌وبرگشتِ کلی» جایش را به دو قیمت یک‌طرفه‌ی جدا می‌دهد: رفت با نرخ
        // ساعتِ Time، برگشت (مبدا/مقصد جابه‌جا) با نرخ ساعتِ ReturnTime — دقیقاً هم‌ارز دو Tripی که
        // GeneratePetResanServiceTripsAsync برای همین ردیف می‌سازد؛ رفتار قدیمی (بدون ReturnTime) دست‌نخورده می‌ماند.
        private async Task<(double Total, int OccurrenceCount)> CalculateWeeklyTotalPriceAsync(PetResanServiceCreateDto dto)
        {
            var schedules = dto.Schedules ?? new List<PetResanServiceScheduleDto>();
            var uniqueSchedules = schedules
                .GroupBy(s => $"{s.WeekDayId}:{s.Time}")
                .Select(g => g.First())
                .ToList();

            if (uniqueSchedules.Count == 0)
            {
                // بدون هیچ نوبتی هنوز (پیش‌نمایش اولیه‌ی فرم): یک نوبت با نرخ همین لحظه، مثل رفتار قبلی
                var now = await CalculateOccurrencePriceAsync(dto.Origin, dto.Destination, dto.FromAddress, dto.ToAddress, DateTime.Now, dto.TripOptionIds, dto.RoundTrip);
                return (now, 1);
            }

            var priceByKey = new Dictionary<string, double>(StringComparer.Ordinal);

            async Task<double> PriceAtAsync(string cacheKey, Application.Common.Dto.LocationPoint.PointDto origin, Application.Common.Dto.LocationPoint.PointDto destination, string fromAddress, string toAddress, string time, bool roundTrip)
            {
                if (priceByKey.TryGetValue(cacheKey, out var cached))
                    return cached;
                var atMoment = ReservationScheduleValidator.TryGetServiceStartDateTime(DateTime.Today, time, out var parsed) ? parsed : DateTime.Now;
                var price = await CalculateOccurrencePriceAsync(origin, destination, fromAddress, toAddress, atMoment, dto.TripOptionIds, roundTrip);
                priceByKey[cacheKey] = price;
                return price;
            }

            double total = 0;
            foreach (var schedule in uniqueSchedules)
            {
                if (!string.IsNullOrWhiteSpace(schedule.ReturnTime))
                {
                    // دو یک‌طرفه‌ی جدا: رفت (مبدا→مقصد) و برگشت (مقصد→مبدا)
                    total += await PriceAtAsync($"go:{schedule.Time}", dto.Origin, dto.Destination, dto.FromAddress, dto.ToAddress, schedule.Time, false);
                    total += await PriceAtAsync($"ret:{schedule.ReturnTime}", dto.Destination, dto.Origin, dto.ToAddress, dto.FromAddress, schedule.ReturnTime, false);
                }
                else
                {
                    total += await PriceAtAsync($"rt:{schedule.Time}:{dto.RoundTrip}", dto.Origin, dto.Destination, dto.FromAddress, dto.ToAddress, schedule.Time, dto.RoundTrip);
                }
            }
            return (total, uniqueSchedules.Count);
        }

        private async Task<double> CalculateOccurrencePriceAsync(
            Application.Common.Dto.LocationPoint.PointDto origin,
            Application.Common.Dto.LocationPoint.PointDto destination,
            string fromAddress,
            string toAddress,
            DateTime atMoment,
            List<long> tripOptionIds = null,
            bool roundTrip = true)
        {
            var priceInput = new TripDto
            {
                Origin = origin,
                Destination = destination,
                FromAddress = fromAddress,
                ToAddress = toAddress,
                TripStartDateTime = atMoment,
                RoundTrip = roundTrip,
                TripOptionIds = tripOptionIds ?? new List<long>()
            };
            return await _priceCalculationService.CalculateTripPrice(priceInput);
        }

        public async Task<BaseResultDto<PetResanServiceVDto>> InsertAsyncDto(PetResanServiceCreateDto dto, long userId, string idempotencyKey)
        {
            if (!TryNormalizeIdempotencyKey(idempotencyKey, out var normalizedIdempotencyKey))
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.PaymentIdempotencyKeyHeaderMustBeValidUuid, null);

            if (normalizedIdempotencyKey != null)
            {
                var existing = await _context.PetResanServices
                    .Include(s => s.Schedules)
                    .Include(s => s.TripOptions)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.IdempotencyKey == normalizedIdempotencyKey);
                if (existing != null)
                {
                    if (!IsSameCreateRequest(existing, dto))
                        return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.PaymentIdempotencyKeyAlreadyUsedForDifferentCheckout, null);

                    return await FindAsyncVDto(existing.Id, userId);
                }
            }

            if (dto.Origin == null)
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.PleaseSetOrigin, null);
            if (dto.Destination == null)
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.PleaseSetDestination, null);
            if (dto.Schedules == null || dto.Schedules.Count == 0)
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.PetResanServiceAtLeastOneScheduleRequired, null);
            if (dto.TotalWeeks.HasValue && dto.TotalWeeks.Value <= 0)
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.PetResanServiceInvalidTotalWeeks, null);

            var requestedTripOptionIds = dto.TripOptionIds ?? new List<long>();
            var tripOptionIds = requestedTripOptionIds.Distinct().ToList();
            if (tripOptionIds.Count != requestedTripOptionIds.Count)
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.DuplicateValue, null);
            if (tripOptionIds.Count != await _context.TripOptions.AsNoTracking()
                    .CountAsync(option => tripOptionIds.Contains(option.Id) && option.Active && !option.Deleted))
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.NothingFound, null);

            var requestedPetIds = (dto.UserPetIds != null && dto.UserPetIds.Any())
                ? dto.UserPetIds
                : (dto.UserPetId > 0 ? new List<long> { dto.UserPetId } : new List<long>());
            var petIds = requestedPetIds.Distinct().ToList();
            if (petIds.Count == 0)
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.SelectAtLeastOneType, null);
            if (petIds.Count != requestedPetIds.Count)
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.DuplicateValue, null);
            var ownedPetCount = await _context.UserPets.AsNoTracking().CountAsync(s => petIds.Contains(s.Id) && s.UserId == userId);
            if (ownedPetCount != petIds.Count)
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.NothingFound, null);

            var uniqueSchedules = new HashSet<string>(StringComparer.Ordinal);
            foreach (var schedule in dto.Schedules)
            {
                if (!ReservationScheduleValidator.TryGetServiceStartDateTime(DateTime.Now, schedule.Time, out _))
                    return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.InvalidTimeFormat, null);
                if (!string.IsNullOrWhiteSpace(schedule.ReturnTime) &&
                    !ReservationScheduleValidator.TryGetServiceTimeRange(schedule.Time, schedule.ReturnTime, out _, out _))
                    return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.PetResanServiceReturnTimeMustBeAfterDeparture, null);
                if (!uniqueSchedules.Add($"{schedule.WeekDayId}:{schedule.Time}"))
                    return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.DuplicateValue, null);
            }

            var startDate = DateTime.Now.Date;
            double price;
            try
            {
                // میانگین قیمتِ هر ردیف با نرخِ واقعیِ ساعتِ همان نوبت (نه نرخ لحظه‌ی ثبت درخواست)؛ فقط برای
                // پیش‌نمایش/نمایش به کاربر است — قیمت قطعی هر Trip دوباره در GeneratePetResanServiceTripsAsync
                // با نرخ همان ساعت محاسبه می‌شود.
                var (total, occurrenceCount) = await CalculateWeeklyTotalPriceAsync(new PetResanServiceCreateDto
                {
                    Origin = dto.Origin,
                    Destination = dto.Destination,
                    FromAddress = dto.FromAddress,
                    ToAddress = dto.ToAddress,
                    RoundTrip = dto.RoundTrip,
                    TripOptionIds = tripOptionIds,
                    Schedules = dto.Schedules
                });
                price = occurrenceCount > 0 ? total / occurrenceCount : total;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "PetResan service activation could not calculate a verified price for user {UserId}.", userId);
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.TripPriceCalculationFailed, null);
            }

            var service = new PetResanService
            {
                UserId = userId,
                UserPetId = petIds[0],
                Pets = petIds.Select(id => new PetResanServicePet { UserPetId = id }).ToList(),
                Origin = new Point(dto.Origin.x, dto.Origin.y) { SRID = 4326 },
                Destination = new Point(dto.Destination.x, dto.Destination.y) { SRID = 4326 },
                FromAddress = dto.FromAddress,
                ToAddress = dto.ToAddress,
                StartDate = startDate,
                TotalWeeks = dto.TotalWeeks,
                EndDate = dto.TotalWeeks.HasValue ? startDate.AddDays(dto.TotalWeeks.Value * 7) : (DateTime?)null,
                Active = true,
                CreateDate = DateTime.Now,
                IdempotencyKey = normalizedIdempotencyKey,
                PricePerOccurrence = price,
                TripOptions = tripOptionIds.Select(id => new Entities.Entities.TripOption { Id = id }).ToList(),
                Schedules = dto.Schedules.Select(s => new PetResanServiceSchedule
                {
                    WeekDayId = s.WeekDayId,
                    Time = s.Time,
                    ReturnTime = string.IsNullOrWhiteSpace(s.ReturnTime) ? null : s.ReturnTime,
                    Active = true
                }).ToList()
            };

            foreach (var option in service.TripOptions)
                _context.Entry(option).State = EntityState.Unchanged;

            await _context.PetResanServices.AddAsync(service);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException exception) when (normalizedIdempotencyKey != null && IsUniqueConstraintViolation(exception))
            {
                DetachServiceGraph(service);
                var duplicate = await _context.PetResanServices
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.IdempotencyKey == normalizedIdempotencyKey);
                if (duplicate == null)
                    throw;

                _logger.LogInformation("Replayed concurrent PetResan service creation for user {UserId}.", userId);
                return await FindAsyncVDto(duplicate.Id, userId);
            }

            return await FindAsyncVDto(service.Id, userId);
        }

        private static bool TryNormalizeIdempotencyKey(string value, out string normalized)
        {
            normalized = null;
            if (string.IsNullOrWhiteSpace(value))
                return true;
            if (!Guid.TryParseExact(value.Trim(), "D", out var parsed) || parsed == Guid.Empty)
                return false;

            normalized = parsed.ToString("D");
            return true;
        }

        private static bool IsSameCreateRequest(PetResanService existing, PetResanServiceCreateDto dto)
        {
            if (existing == null || dto == null || existing.Origin == null || existing.Destination == null || dto.Origin == null || dto.Destination == null)
                return false;

            var requestedSchedules = (dto.Schedules ?? new List<PetResanServiceScheduleDto>())
                .Select(s => (s.WeekDayId, s.Time, ReturnTime: s.ReturnTime ?? string.Empty))
                .OrderBy(s => s.WeekDayId)
                .ThenBy(s => s.Time, StringComparer.Ordinal);
            var existingSchedules = (existing.Schedules ?? new List<PetResanServiceSchedule>())
                .Select(s => (s.WeekDayId, s.Time, ReturnTime: s.ReturnTime ?? string.Empty))
                .OrderBy(s => s.WeekDayId)
                .ThenBy(s => s.Time, StringComparer.Ordinal);
            var requestedTripOptionIds = (dto.TripOptionIds ?? new List<long>()).OrderBy(id => id);
            var existingTripOptionIds = (existing.TripOptions ?? new List<Entities.Entities.TripOption>()).Select(option => option.Id).OrderBy(id => id);

            var existingPetIds = (existing.Pets != null && existing.Pets.Any()) ? existing.Pets.Select(p => p.UserPetId).OrderBy(id => id) : new[] { existing.UserPetId }.OrderBy(id => id);
            var requestedPetIdsForCompare = ((dto.UserPetIds != null && dto.UserPetIds.Any()) ? dto.UserPetIds : new List<long> { dto.UserPetId }).OrderBy(id => id);

            return existingPetIds.SequenceEqual(requestedPetIdsForCompare) &&
                   existing.TotalWeeks == dto.TotalWeeks &&
                   SameCoordinate(existing.Origin.X, dto.Origin.x) &&
                   SameCoordinate(existing.Origin.Y, dto.Origin.y) &&
                   SameCoordinate(existing.Destination.X, dto.Destination.x) &&
                   SameCoordinate(existing.Destination.Y, dto.Destination.y) &&
                   string.Equals(existing.FromAddress, dto.FromAddress, StringComparison.Ordinal) &&
                   string.Equals(existing.ToAddress, dto.ToAddress, StringComparison.Ordinal) &&
                   requestedTripOptionIds.SequenceEqual(existingTripOptionIds) &&
                   requestedSchedules.SequenceEqual(existingSchedules);
        }

        private static bool SameCoordinate(double left, double right) => Math.Abs(left - right) < 0.000001;

        private void DetachServiceGraph(PetResanService service)
        {
            foreach (var schedule in service.Schedules ?? Enumerable.Empty<PetResanServiceSchedule>())
                _context.Entry(schedule).State = EntityState.Detached;
            foreach (var petLink in service.Pets ?? Enumerable.Empty<PetResanServicePet>())
                _context.Entry(petLink).State = EntityState.Detached;
            _context.Entry(service).State = EntityState.Detached;
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
            exception.InnerException is SqlException sqlException && (sqlException.Number == 2601 || sqlException.Number == 2627);

        public async Task<BaseResultDto<List<PetResanServiceVDto>>> GetMyListAsync(long userId)
        {
            var services = await _context.PetResanServices
                .Include(s => s.UserPet)
                .Include(s => s.Pets).ThenInclude(p => p.UserPet)
                .Include(s => s.TripOptions)
                .Include(s => s.Schedules).ThenInclude(s => s.WeekDay)
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            return new BaseResultDto<List<PetResanServiceVDto>>(true, services.Select(ToVDto).ToList());
        }

        public async Task<BaseResultDto<PetResanServiceVDto>> FindAsyncVDto(long id, long userId)
        {
            var service = await _context.PetResanServices
                .Include(s => s.UserPet)
                .Include(s => s.Pets).ThenInclude(p => p.UserPet)
                .Include(s => s.TripOptions)
                .Include(s => s.Schedules).ThenInclude(s => s.WeekDay)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (service == null)
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.NothingFound, null);

            return new BaseResultDto<PetResanServiceVDto>(true, ToVDto(service));
        }

        public async Task<BaseResultDto> CancelAsync(long id, long userId)
        {
            var service = await _context.PetResanServices.AsTracking().FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
            if (service == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            service.Active = false;
            service.CancelDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return new BaseResultDto(true);
        }

        private static PetResanServiceVDto ToVDto(PetResanService service) => new PetResanServiceVDto
        {
            Id = service.Id,
            UserPetId = service.UserPetId,
            UserPetName = service.UserPet?.Name,
            UserPets = (service.Pets != null && service.Pets.Any())
                ? service.Pets.Select(p => new PetResanServicePetVDto { UserPetId = p.UserPetId, Name = p.UserPet?.Name }).ToList()
                : new List<PetResanServicePetVDto> { new PetResanServicePetVDto { UserPetId = service.UserPetId, Name = service.UserPet?.Name } },
            Origin = new Application.Common.Dto.LocationPoint.PointDto(service.Origin.X, service.Origin.Y),
            Destination = new Application.Common.Dto.LocationPoint.PointDto(service.Destination.X, service.Destination.Y),
            FromAddress = service.FromAddress,
            ToAddress = service.ToAddress,
            StartDate = service.StartDate,
            TotalWeeks = service.TotalWeeks,
            EndDate = service.EndDate,
            Active = service.Active,
            PricePerOccurrence = service.PricePerOccurrence,
            PricePerWeek = service.PricePerOccurrence * Math.Max(1, (service.Schedules ?? new List<PetResanServiceSchedule>()).Count(x => x.Active)),
            TripOptions = (service.TripOptions ?? new List<Entities.Entities.TripOption>()).Select(option => new Application.Services.TripSrv.TripOptionSrv.Dto.TripOptionVDto
            {
                Id = option.Id,
                Name = option.Name,
                Price = option.Price,
                Active = option.Active
            }).ToList(),
            Schedules = (service.Schedules ?? new List<PetResanServiceSchedule>()).Select(s => new PetResanServiceScheduleVDto
            {
                Id = s.Id,
                WeekDayId = s.WeekDayId,
                WeekDayName = s.WeekDay?.Name,
                WeekDayNumber = s.WeekDay?.Number ?? 0,
                Time = s.Time,
                ReturnTime = s.ReturnTime
            }).OrderBy(s => s.WeekDayNumber).ToList()
        };
    }
}
