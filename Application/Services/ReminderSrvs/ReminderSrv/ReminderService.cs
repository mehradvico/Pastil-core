using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Common.Enumerable.Message;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.CommonSrv.PushNotificationSrv.Iface;
using Application.Services.ReminderSrvs.ReminderSrv.Dto;
using Application.Services.ReminderSrvs.ReminderSrv.Iface;
using Application.Services.Setting.MessageSenderSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ReminderSrvs.ReminderSrv
{
    public class ReminderService : CommonSrv<Reminder, ReminderDto>, IReminderService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IMessageSenderService _messageSender;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ILogger<ReminderService> _logger;
        private static readonly TimeZoneInfo TehranTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");

        public ReminderService(
            IDataBaseContext _context,
            IMapper mapper,
            IMessageSenderService messageSender,
            IPushNotificationService pushNotificationService,
            ILogger<ReminderService> logger) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._messageSender = messageSender;
            _pushNotificationService = pushNotificationService;
            _logger = logger;
        }

        public async Task<BaseResultDto<ReminderVDto>> FindAsyncVDto(long id)
        {
            var item = await ReminderQuery()
                .FirstOrDefaultAsync(reminder => reminder.Id == id && !reminder.Deleted);
            return ReminderResult(item);
        }

        public async Task<BaseResultDto<ReminderVDto>> FindUserAsyncVDto(long id, long userId)
        {
            var item = await ReminderQuery()
                .FirstOrDefaultAsync(reminder =>
                    reminder.Id == id &&
                    !reminder.Deleted &&
                    reminder.UserPet.UserId == userId);
            return ReminderResult(item);
        }

        public ReminderSearchDto Search(ReminderInputDto baseSearchDto)
        {
            var model = _context.Reminders.Include(s => s.UserPet).ThenInclude(s => s.Pet).Include(s => s.UserPet).ThenInclude(s => s.User)
                .Include(s => s.ReminderCycle).Include(s => s.ReminderType).Include(s => s.UserPet).ThenInclude(s => s.Picture).AsQueryable().Where(s => !s.Deleted);

            if (baseSearchDto.UserId.HasValue)
            {
                model = model.Where(s => s.UserPet.UserId == baseSearchDto.UserId.Value);
            }
            if (baseSearchDto.ReminderCycleId.HasValue)
            {
                model = model.Where(s => s.ReminderCycleId == baseSearchDto.ReminderCycleId);
            }
            if (baseSearchDto.ReminderTypeId.HasValue)
            {
                model = model.Where(s => s.ReminderTypeId == baseSearchDto.ReminderTypeId);
            }
            if (!string.IsNullOrEmpty(baseSearchDto.Q))
            {
                var text = baseSearchDto.Q.Trim();
                model = model.Where(s =>
                    s.UserPet.User.Mobile.Contains(text) ||
                    s.UserPet.User.FirstName.Contains(text) ||
                    s.UserPet.User.LastName.Contains(text) ||
                    s.UserPet.Name.Contains(text));
            }
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
            return new ReminderSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<ReminderDto>> InsertAsyncDto(ReminderDto dto)
        {
            return await InsertValidatedAsync(dto, null);
        }

        public async Task<BaseResultDto<ReminderDto>> InsertUserAsyncDto(ReminderDto dto, long userId)
        {
            return await InsertValidatedAsync(dto, userId);
        }

        public async Task<BaseResultDto> DeleteUserAsync(long id, long userId)
        {
            var item = await _context.Reminders
                .FirstOrDefaultAsync(reminder =>
                    reminder.Id == id &&
                    !reminder.Deleted &&
                    reminder.UserPet.UserId == userId);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            item.Deleted = true;
            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        private async Task<BaseResultDto<ReminderDto>> InsertValidatedAsync(
            ReminderDto dto,
            long? ownerUserId)
        {
            try
            {
                var modelCheker = ModelHelper<ReminderDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                    return modelCheker;

                var today = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TehranTimeZone).Date;
                if (dto.StartDate.Date <= today)
                    return new BaseResultDto<ReminderDto>(false, Resource.Notification.StartDateMustBeFromTommarow, dto);

                if (dto.ReminderTypeId <= 0 || dto.ReminderCycleId <= 0 || dto.UserPetId <= 0)
                    return new BaseResultDto<ReminderDto>(false, Resource.Notification.InvalidData, dto);

                var reminderTypeExists = await _context.ReminderTypes
                    .AsNoTracking()
                    .AnyAsync(item => item.Id == dto.ReminderTypeId && !item.Deleted);
                if (!reminderTypeExists)
                    return new BaseResultDto<ReminderDto>(false, Resource.Notification.NothingFound, dto);

                var reminderCycleExists = await _context.ReminderCycles
                    .AsNoTracking()
                    .AnyAsync(item =>
                        item.Id == dto.ReminderCycleId &&
                        !item.Deleted &&
                        item.Cycle > 0);
                if (!reminderCycleExists)
                    return new BaseResultDto<ReminderDto>(false, Resource.Notification.InvalidData, dto);

                var userPetExists = await _context.UserPets
                    .AsNoTracking()
                    .AnyAsync(item =>
                        item.Id == dto.UserPetId &&
                        item.Active &&
                        !item.Deleted &&
                        (!ownerUserId.HasValue || item.UserId == ownerUserId.Value));
                if (!userPetExists)
                    return new BaseResultDto<ReminderDto>(false, Resource.Notification.NothingFound, dto);

                var startDate = dto.StartDate.Date;
                var endDate = startDate.AddDays(1);
                var duplicateExists = await _context.Reminders
                    .AsNoTracking()
                    .AnyAsync(item =>
                        !item.Deleted &&
                        item.UserPetId == dto.UserPetId &&
                        item.ReminderTypeId == dto.ReminderTypeId &&
                        item.ReminderCycleId == dto.ReminderCycleId &&
                        item.StartDate >= startDate &&
                        item.StartDate < endDate);
                if (duplicateExists)
                    return new BaseResultDto<ReminderDto>(false, Resource.Notification.DuplicateValue, dto);

                var item = mapper.Map<Reminder>(dto);
                item.StartDate = startDate;
                item.LastChecked = null;
                item.Deleted = false;
                await _context.Reminders.AddAsync(item);
                await _context.SaveChangesAsync();
                return new BaseResultDto<ReminderDto>(true, mapper.Map<ReminderDto>(item));
            }
            catch (DbUpdateException)
            {
                return new BaseResultDto<ReminderDto>(false, Resource.Notification.DuplicateValue, dto);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Creating reminder failed for user pet {UserPetId}", dto.UserPetId);
                return new BaseResultDto<ReminderDto>(false, Resource.Notification.Unsuccess, dto);
            }
        }

        public async Task SyncReminderAsync()
        {
            var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TehranTimeZone).DateTime;
            var today = now.Date;
            var failedReminderIds = new HashSet<long>();

            while (true)
            {
                var reminders = await _context.Reminders
                    .Include(reminder => reminder.ReminderCycle)
                    .Include(reminder => reminder.ReminderType)
                    .Include(reminder => reminder.UserPet)
                    .ThenInclude(userPet => userPet.User)
                    .Where(reminder =>
                        !reminder.Deleted &&
                        reminder.UserPet.Active &&
                        !reminder.UserPet.Deleted &&
                        !reminder.UserPet.User.Deleted &&
                        !reminder.UserPet.User.Locked &&
                        (reminder.LastChecked == null || reminder.LastChecked.Value.Date < today) &&
                        !failedReminderIds.Contains(reminder.Id))
                    .OrderBy(reminder => reminder.Id)
                    .Take(150)
                    .AsTracking()
                    .ToListAsync();

                if (reminders.Count == 0)
                    break;

                foreach (var reminder in reminders)
                {
                    if (reminder.ReminderCycle.Deleted ||
                        reminder.ReminderType.Deleted ||
                        reminder.ReminderCycle.Cycle <= 0)
                    {
                        _logger.LogWarning(
                            "Reminder {ReminderId} has an inactive type or invalid cycle",
                            reminder.Id);
                        reminder.LastChecked = now;
                        continue;
                    }

                    try
                    {
                        var moment = ReminderScheduleCalculator.Resolve(
                            reminder.StartDate,
                            reminder.ReminderCycle.Cycle,
                            today);

                        await SendReminderAsync(reminder, moment, today);
                        reminder.LastChecked = now;
                    }
                    catch (Exception exception)
                    {
                        failedReminderIds.Add(reminder.Id);
                        _logger.LogError(
                            exception,
                            "Processing reminder {ReminderId} failed",
                            reminder.Id);
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task SendReminderAsync(
            Reminder reminder,
            ReminderNotificationMoment moment,
            DateTime today)
        {
            var notification = moment switch
            {
                ReminderNotificationMoment.SevenDaysBefore => new ReminderNotification(
                    PushTypeEnum.PushReminderOneWeekBefore,
                    MessageTypeEnum.UserReminderOneWeekAgo,
                    "یک هفته دیگر است."),
                ReminderNotificationMoment.OneDayBefore => new ReminderNotification(
                    PushTypeEnum.PushReminderOneDayBefore,
                    MessageTypeEnum.UserReminderOneDayAgo,
                    "فردا است."),
                ReminderNotificationMoment.OneDayAfter => new ReminderNotification(
                    PushTypeEnum.PushReminderOneDayAfter,
                    MessageTypeEnum.UserReminderTomorrow,
                    "دیروز بوده است."),
                _ => null
            };

            if (notification == null)
                return;

            await _pushNotificationService.SendPushAsync(
                pushType: notification.PushType,
                userId: reminder.UserPet.UserId,
                token1: reminder.UserPet.Name,
                token2: reminder.ReminderType.Name,
                token3: notification.WhenText);

            await _messageSender.SendMessageAsync(
                messageType: notification.MessageType,
                mobileReceptor: reminder.UserPet.User.Mobile,
                emailReceptor: null,
                token1: reminder.UserPet.Name,
                token2: reminder.ReminderType.Name,
                sendDate: today);
        }

        private sealed record ReminderNotification(
            PushTypeEnum PushType,
            MessageTypeEnum MessageType,
            string WhenText);

        private IQueryable<Reminder> ReminderQuery() =>
            _context.Reminders
                .AsNoTracking()
                .Include(reminder => reminder.UserPet)
                .ThenInclude(userPet => userPet.Pet)
                .Include(reminder => reminder.UserPet)
                .ThenInclude(userPet => userPet.User)
                .Include(reminder => reminder.ReminderCycle)
                .Include(reminder => reminder.ReminderType)
                .Include(reminder => reminder.UserPet)
                .ThenInclude(userPet => userPet.Picture);

        private BaseResultDto<ReminderVDto> ReminderResult(Reminder item) =>
            item == null
                ? new BaseResultDto<ReminderVDto>(false, Resource.Notification.NothingFound, null)
                : new BaseResultDto<ReminderVDto>(true, mapper.Map<ReminderVDto>(item));

    }
}
