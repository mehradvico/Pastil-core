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
                var price = await CalculateOccurrencePriceAsync(dto.Origin, dto.Destination, dto.FromAddress, dto.ToAddress, DateTime.Now, dto.TripOptionIds);
                return new BaseResultDto<double>(true, price);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "PetResan price preview could not be calculated.");
                return new BaseResultDto<double>(false, Resource.Notification.TripPriceCalculationFailed, 0);
            }
        }

        private async Task<double> CalculateOccurrencePriceAsync(
            Application.Common.Dto.LocationPoint.PointDto origin,
            Application.Common.Dto.LocationPoint.PointDto destination,
            string fromAddress,
            string toAddress,
            DateTime atMoment,
            List<long> tripOptionIds = null)
        {
            var priceInput = new TripDto
            {
                Origin = origin,
                Destination = destination,
                FromAddress = fromAddress,
                ToAddress = toAddress,
                TripStartDateTime = atMoment,
                RoundTrip = true,
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

            var pet = await _context.UserPets.AsNoTracking().FirstOrDefaultAsync(s => s.Id == dto.UserPetId && s.UserId == userId);
            if (pet == null)
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.NothingFound, null);

            var uniqueSchedules = new HashSet<string>(StringComparer.Ordinal);
            foreach (var schedule in dto.Schedules)
            {
                if (!ReservationScheduleValidator.TryGetServiceStartDateTime(DateTime.Now, schedule.Time, out _))
                    return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.InvalidTimeFormat, null);
                if (!uniqueSchedules.Add($"{schedule.WeekDayId}:{schedule.Time}"))
                    return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.DuplicateValue, null);
            }

            var startDate = DateTime.Now.Date;
            double price;
            try
            {
                price = await CalculateOccurrencePriceAsync(dto.Origin, dto.Destination, dto.FromAddress, dto.ToAddress, DateTime.Now, tripOptionIds);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "PetResan service activation could not calculate a verified price for user {UserId}.", userId);
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.TripPriceCalculationFailed, null);
            }

            var service = new PetResanService
            {
                UserId = userId,
                UserPetId = dto.UserPetId,
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
                .Select(s => (s.WeekDayId, s.Time))
                .OrderBy(s => s.WeekDayId)
                .ThenBy(s => s.Time, StringComparer.Ordinal);
            var existingSchedules = (existing.Schedules ?? new List<PetResanServiceSchedule>())
                .Select(s => (s.WeekDayId, s.Time))
                .OrderBy(s => s.WeekDayId)
                .ThenBy(s => s.Time, StringComparer.Ordinal);
            var requestedTripOptionIds = (dto.TripOptionIds ?? new List<long>()).OrderBy(id => id);
            var existingTripOptionIds = (existing.TripOptions ?? new List<Entities.Entities.TripOption>()).Select(option => option.Id).OrderBy(id => id);

            return existing.UserPetId == dto.UserPetId &&
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
            _context.Entry(service).State = EntityState.Detached;
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
            exception.InnerException is SqlException sqlException && (sqlException.Number == 2601 || sqlException.Number == 2627);

        public async Task<BaseResultDto<List<PetResanServiceVDto>>> GetMyListAsync(long userId)
        {
            var services = await _context.PetResanServices
                .Include(s => s.UserPet)
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
            Origin = new Application.Common.Dto.LocationPoint.PointDto(service.Origin.X, service.Origin.Y),
            Destination = new Application.Common.Dto.LocationPoint.PointDto(service.Destination.X, service.Destination.Y),
            FromAddress = service.FromAddress,
            ToAddress = service.ToAddress,
            StartDate = service.StartDate,
            TotalWeeks = service.TotalWeeks,
            EndDate = service.EndDate,
            Active = service.Active,
            PricePerOccurrence = service.PricePerOccurrence,
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
                Time = s.Time
            }).OrderBy(s => s.WeekDayNumber).ToList()
        };
    }
}
