using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Message;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.ReminderSrvs.ReminderSrv.Dto;
using Application.Services.ReminderSrvs.ReminderSrv.Iface;
using Application.Services.Setting.MessageSenderSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ReminderSrvs.ReminderSrv
{
    public class ReminderService : CommonSrv<Reminder, ReminderDto>, IReminderService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IMessageSenderService _messageSender;
        private readonly ICurrentUserHelper _currentUser;
        public ReminderService(IDataBaseContext _context, IMapper mapper, IMessageSenderService messageSender, ICurrentUserHelper _currentUser) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._messageSender = messageSender;
            this._currentUser = _currentUser;
        }
        public async Task<BaseResultDto<ReminderVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Reminders.Include(s => s.UserPet).ThenInclude(s => s.Pet).Include(s => s.UserPet).ThenInclude(s => s.User)
                .Include(s => s.ReminderCycle).Include(s => s.ReminderType).Include(s => s.UserPet).ThenInclude(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<ReminderVDto>(true, mapper.Map<ReminderVDto>(item));
            }
            return new BaseResultDto<ReminderVDto>(false, mapper.Map<ReminderVDto>(item));
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
                model = model.Where(s => s.UserPet.User.Mobile.Contains(baseSearchDto.Q));
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
            try
            {
                var modelCheker = ModelHelper<ReminderDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                else
                {
                    var item = mapper.Map<Reminder>(dto);
                    item.LastChecked = null;

                    if (dto.StartDate.Date <= DateTime.Today)
                    {
                        return new BaseResultDto<ReminderDto>(false, Resource.Notification.StartDateMustBeFromTommarow, dto);
                    }
                    await _context.Reminders.AddAsync(item);


                    await _context.SaveChangesAsync();
                    return new BaseResultDto<ReminderDto>(true, mapper.Map<ReminderDto>(item));
                }

            }
            catch (Exception ex)
            {
                return new BaseResultDto<ReminderDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }

        public async Task SyncReminderAsync()
        {
            var today = DateTime.Today;

            var reminders = await _context.Reminders
                .Include(r => r.ReminderCycle)
                .Include(r => r.ReminderType)
                .Include(r => r.UserPet).ThenInclude(s => s.User)
                .Where(r => !r.Deleted &&
                            (r.LastChecked == null || r.LastChecked.Value.Date < today) && r.StartDate < today)
                .OrderBy(r => r.LastChecked)
                .Take(150).AsTracking()
                .ToListAsync();

            foreach (var reminder in reminders)
            {
                var startDate = reminder.StartDate;
                var cycleMonths = reminder.ReminderCycle.Cycle;

                var nthDate = startDate;
                bool shouldNotify = false;
                int whenText = 0;

                while (nthDate <= today.AddDays(1))
                {
                    if (today == nthDate.AddDays(-7))
                    {
                        whenText = -7;
                        shouldNotify = true;
                        break;
                    }
                    if (today == nthDate.AddDays(-1))
                    {
                        whenText = -1;
                        shouldNotify = true;
                        break;
                    }
                    if (today == nthDate.AddDays(1))
                    {
                        whenText = 1;
                        shouldNotify = true;
                        break;
                    }

                    nthDate = nthDate.AddMonths(cycleMonths);
                }

                if (shouldNotify)
                {
                    if (whenText == -7)
                    {

                        await _messageSender.SendMessageAsync(messageType: MessageTypeEnum.UserReminderOneWeekAgo, mobileReceptor: reminder.UserPet.User.Mobile, emailReceptor: null, token1: reminder.UserPet.Name, token2: reminder.ReminderType.Name, sendDate: today);
                    }
                    else if (whenText == -1)
                    {
                        await _messageSender.SendMessageAsync(messageType: MessageTypeEnum.UserReminderOneDayAgo, mobileReceptor: reminder.UserPet.User.Mobile, emailReceptor: null, token1: reminder.UserPet.Name, token2: reminder.ReminderType.Name, sendDate: today);
                    }
                    else if (whenText == 1)
                    {
                        await _messageSender.SendMessageAsync(messageType: MessageTypeEnum.UserReminderTomorrow, mobileReceptor: reminder.UserPet.User.Mobile, emailReceptor: null, token1: reminder.UserPet.Name, token2: reminder.ReminderType.Name, sendDate: today);
                    }
                }

                reminder.LastChecked = DateTime.Now;
                //_context.Reminders.Attach(reminder);
                //_context.Entry(reminder).Property("LastChecked").IsModified = true;

            }
            await _context.SaveChangesAsync();



        }


    }
}
