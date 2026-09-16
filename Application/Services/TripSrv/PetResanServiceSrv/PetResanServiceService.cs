using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Services.TripSrv.PetResanServiceSrv.Dto;
using Application.Services.TripSrv.PetResanServiceSrv.Iface;
using Application.Services.TripSrv.PriceCalculationSrv.Iface;
using Application.Services.TripSrv.TripSrv.Dto;
using Application.Services.WeekDaySrv.WeekDaySrv.Iface;
using Entities.Entities.PetResanServiceField;
using Microsoft.EntityFrameworkCore;
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

        public PetResanServiceService(
            IDataBaseContext context,
            IPriceCalculationService priceCalculationService,
            IWeekDayService weekDayService)
        {
            _context = context;
            _priceCalculationService = priceCalculationService;
            _weekDayService = weekDayService;
        }

        public async Task<BaseResultDto<double>> PreviewPriceAsync(PetResanServiceCreateDto dto)
        {
            if (dto.Origin == null)
                return new BaseResultDto<double>(false, Resource.Notification.PleaseSetOrigin, 0);
            if (dto.Destination == null)
                return new BaseResultDto<double>(false, Resource.Notification.PleaseSetDestination, 0);

            var price = await CalculateOccurrencePriceAsync(dto.Origin, dto.Destination, dto.FromAddress, dto.ToAddress, DateTime.Now);
            return new BaseResultDto<double>(true, price);
        }

        private async Task<double> CalculateOccurrencePriceAsync(
            Application.Common.Dto.LocationPoint.PointDto origin,
            Application.Common.Dto.LocationPoint.PointDto destination,
            string fromAddress,
            string toAddress,
            DateTime atMoment)
        {
            var priceInput = new TripDto
            {
                Origin = origin,
                Destination = destination,
                FromAddress = fromAddress,
                ToAddress = toAddress,
                TripStartDateTime = atMoment,
                RoundTrip = true
            };
            return await _priceCalculationService.CalculateTripPrice(priceInput);
        }

        public async Task<BaseResultDto<PetResanServiceVDto>> InsertAsyncDto(PetResanServiceCreateDto dto, long userId)
        {
            if (dto.Origin == null)
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.PleaseSetOrigin, null);
            if (dto.Destination == null)
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.PleaseSetDestination, null);
            if (dto.Schedules == null || dto.Schedules.Count == 0)
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.PetResanServiceAtLeastOneScheduleRequired, null);
            if (dto.TotalWeeks.HasValue && dto.TotalWeeks.Value <= 0)
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.PetResanServiceInvalidTotalWeeks, null);

            var pet = await _context.UserPets.AsNoTracking().FirstOrDefaultAsync(s => s.Id == dto.UserPetId && s.UserId == userId);
            if (pet == null)
                return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.NothingFound, null);

            foreach (var schedule in dto.Schedules)
            {
                if (!ReservationScheduleValidator.TryGetServiceStartDateTime(DateTime.Now, schedule.Time, out _))
                    return new BaseResultDto<PetResanServiceVDto>(false, Resource.Notification.InvalidTimeFormat, null);
            }

            var startDate = DateTime.Now.Date;
            var price = await CalculateOccurrencePriceAsync(dto.Origin, dto.Destination, dto.FromAddress, dto.ToAddress, DateTime.Now);

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
                PricePerOccurrence = price,
                Schedules = dto.Schedules.Select(s => new PetResanServiceSchedule
                {
                    WeekDayId = s.WeekDayId,
                    Time = s.Time,
                    Active = true
                }).ToList()
            };

            await _context.PetResanServices.AddAsync(service);
            await _context.SaveChangesAsync();

            return await FindAsyncVDto(service.Id, userId);
        }

        public async Task<BaseResultDto<List<PetResanServiceVDto>>> GetMyListAsync(long userId)
        {
            var services = await _context.PetResanServices
                .Include(s => s.UserPet)
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
