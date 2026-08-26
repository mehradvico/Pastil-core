using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Common.Service;
using Application.Services.CompanionSrv.CompanionAssistanceTimeSrv.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceTimeSrv.Dto.Dto;
using Application.Services.CompanionSrv.CompanionAssistanceTimeSrv.Iface;
using Application.Services.Setting.SmsSrv.Iface;
using Application.Services.WeekDaySrv.WeekDaySrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrv.CompanionAssistanceTimeSrv
{
    public class CompanionAssistanceTimeService : CommonSrv<CompanionAssistanceTime, CompanionAssistanceTimeDto>, ICompanionAssistanceTimeService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly ISmsService _smsService;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IWeekDayService _weekDayService;
        public CompanionAssistanceTimeService(IDataBaseContext _context, IWeekDayService weekDayService, IMapper mapper, ISmsService smsService, ICurrentUserHelper currentUser) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._smsService = smsService;
            this._currentUser = currentUser;
            _weekDayService = weekDayService;
        }
        public async Task<BaseResultDto<CompanionAssistanceTimeVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.CompanionAssistanceTimes.Where(s => s.Deleted == false).Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).ThenInclude(s => s.Picture).Include(s => s.WeekDay).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<CompanionAssistanceTimeVDto>(true, mapper.Map<CompanionAssistanceTimeVDto>(item));
            }
            return new BaseResultDto<CompanionAssistanceTimeVDto>(false, mapper.Map<CompanionAssistanceTimeVDto>(item));
        }

        public CompanionAssistanceTimeSearchDto Search(CompanionAssistanceTimeInputDto baseSearchDto)
        {
            var model = _context.CompanionAssistanceTimes.Where(s => s.Deleted == false).Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).ThenInclude(s => s.Picture).Include(s => s.WeekDay).AsQueryable();

            if (baseSearchDto.CompanionAssistanceId.HasValue)
            {
                model = model.Where(s => s.CompanionAssistanceId == baseSearchDto.CompanionAssistanceId.Value);
            }
            if (baseSearchDto.WeekDayId.HasValue)
            {
                model = model.Where(s => s.WeekDayId == baseSearchDto.WeekDayId.Value);
            }
            if (baseSearchDto.Active.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Active.Value);
            }
            switch (baseSearchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.New:
                    {
                        model = model.OrderByDescending(s => s.Id);
                        break;
                    }
                case Common.Enumerable.SortEnum.Old:
                    {
                        model = model.OrderBy(s => s.Id);
                        break;
                    }
                default:
                    break;
            }
            return new CompanionAssistanceTimeSearchDto(baseSearchDto, model, mapper);
        }
        public async Task<BaseResultDto> GetForTomarowAsync(CompanionAssistanceTimeInputDto baseSearchDto)
        {
            var model = _context.CompanionAssistanceTimes.Where(s => s.Deleted == false).Include(s => s.CompanionAssistance).ThenInclude(s => s.Assistance).ThenInclude(s => s.Picture).Include(s => s.WeekDay).AsQueryable();
            model = model.Where(s => s.CompanionAssistanceId == baseSearchDto.CompanionAssistanceId.Value);
            model = model.Where(s => s.Active);
            model = model.OrderBy(o => o.WeekDayId).ThenBy(t => t.StartTime).AsQueryable();
            var resultList = mapper.Map<List<CompanionAssistanceTimeVDto>>(await model.ToListAsync());
            if (resultList.Any())
            {
                int index = -1;
                for (int i = 1; i < 8; i++)
                {
                    var dayOfWeek = DateTime.UtcNow.AddDays(i).DayOfWeek.ToString();
                    var item = resultList.FirstOrDefault(s => s.WeekDay.Label == dayOfWeek);
                    if (item != null)
                    {
                        index = resultList.IndexOf(item);
                        break;
                    }
                }
                if (index > 0)
                {
                    var temp = resultList.GetRange(0, index);
                    resultList.RemoveRange(0, index);
                    resultList.AddRange(temp);
                }
            }
            return new BaseResultDto<List<CompanionAssistanceTimeVDto>>(true, resultList);

        }

        public override async Task<BaseResultDto<CompanionAssistanceTimeDto>> InsertAsyncDto(CompanionAssistanceTimeDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionAssistanceTimeDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                if (_currentUser.CurrentUser.RoleEnum != RoleEnum.Admin.ToString())
                {
                    return new BaseResultDto<CompanionAssistanceTimeDto>(false, Resource.Notification.YouHaveNotPermissionToAddInformationForThisCompanion, dto);
                }
                if (!TimeSpan.TryParseExact(dto.StartTime, "hh\\:mm", CultureInfo.InvariantCulture, out var startTime) ||
                    !TimeSpan.TryParseExact(dto.EndTime, "hh\\:mm", CultureInfo.InvariantCulture, out var endTime))
                {
                    return new BaseResultDto<CompanionAssistanceTimeDto>(false, Resource.Notification.InvalidTimeFormat, dto);
                }
                if (startTime.TotalHours < 0 || startTime.TotalHours > 23 || endTime.TotalHours < 0 || endTime.TotalHours > 23)
                {
                    return new BaseResultDto<CompanionAssistanceTimeDto>(false, Resource.Notification.TheTimeRangeMustBeBetween0And23, dto);
                }

                if (startTime >= endTime)
                {
                    return new BaseResultDto<CompanionAssistanceTimeDto>(false, Resource.Notification.ToTimeMustBeBiggerThanFromTime, dto);
                }

                var existingTimes = await _context.CompanionAssistanceTimes
                    .Where(s => s.CompanionAssistanceId == dto.CompanionAssistanceId && s.WeekDayId == dto.WeekDayId)
                    .ToListAsync();

                bool isOverlapping = existingTimes.Any(s =>
                {
                    if (TimeSpan.TryParseExact(s.StartTime, "hh\\:mm", CultureInfo.InvariantCulture, out var dbStartTime) &&
                        TimeSpan.TryParseExact(s.EndTime, "hh\\:mm", CultureInfo.InvariantCulture, out var dbEndTime))
                    {
                        return startTime < dbEndTime && endTime > dbStartTime;
                    }
                    return false;
                });

                if (isOverlapping)
                {
                    return new BaseResultDto<CompanionAssistanceTimeDto>(false, Resource.Notification.TimesHaveOverlap, dto);
                }

                var isDuplicate = _context.CompanionAssistanceTimes.AsNoTracking().Any(x =>
                    x.WeekDayId == dto.WeekDayId && x.StartTime == dto.StartTime && x.EndTime == dto.EndTime);

                if (isDuplicate)
                {
                    return new BaseResultDto<CompanionAssistanceTimeDto>(false, Resource.Notification.DuplicateValue, dto);
                }

                var item = mapper.Map<CompanionAssistanceTime>(dto);
                await _context.CompanionAssistanceTimes.AddAsync(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<CompanionAssistanceTimeDto>(true, mapper.Map<CompanionAssistanceTimeDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<CompanionAssistanceTimeDto>(false, ex.Message, dto);
            }
        }
        public override BaseResultDto UpdateDto(CompanionAssistanceTimeDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<CompanionAssistanceTimeDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }
                if (!TimeSpan.TryParseExact(dto.StartTime, "hh\\:mm", CultureInfo.InvariantCulture, out var startTime) ||
                    !TimeSpan.TryParseExact(dto.EndTime, "hh\\:mm", CultureInfo.InvariantCulture, out var endTime))
                {
                    return new BaseResultDto<CompanionAssistanceTimeDto>(false, Resource.Notification.InvalidTimeFormat, dto);
                }

                var role = _context.Users.Where(u => u.Id == _currentUser.CurrentUser.UserId).Select(u => u.RoleId).FirstOrDefault();

                if (role != 2)
                {
                    return new BaseResultDto<CompanionAssistanceTimeDto>(false, Resource.Notification.YouHaveNotPermissionToAddInformationForThisCompanion, dto);
                }
                var isDuplicate = _context.CompanionAssistanceTimes.AsNoTracking().Any(x => x.WeekDayId == dto.WeekDayId && x.StartTime == dto.StartTime && x.EndTime == dto.EndTime && x.Id != dto.Id);
                if (isDuplicate)
                {
                    return new BaseResultDto<CompanionAssistanceTimeDto>(false, Resource.Notification.DuplicateValue, dto);
                }
                if (startTime.TotalHours < 0 || startTime.TotalHours > 23 || endTime.TotalHours < 0 || endTime.TotalHours > 23)
                {
                    return new BaseResultDto<CompanionAssistanceTimeDto>(false, Resource.Notification.TheTimeRangeMustBeBetween0And23, dto);
                }

                if (startTime >= endTime)
                {
                    return new BaseResultDto<CompanionAssistanceTimeDto>(false, Resource.Notification.ToTimeMustBeBiggerThanFromTime, dto);
                }

                var existingTimes = _context.CompanionAssistanceTimes.Where(s => s.CompanionAssistanceId == dto.CompanionAssistanceId && s.WeekDayId == dto.WeekDayId).ToList();

                bool isOverlapping = existingTimes.Any(s =>
                {
                    if (TimeSpan.TryParseExact(s.StartTime, "hh\\:mm", CultureInfo.InvariantCulture, out var dbStartTime) &&
                        TimeSpan.TryParseExact(s.EndTime, "hh\\:mm", CultureInfo.InvariantCulture, out var dbEndTime))
                    {
                        return startTime < dbEndTime && endTime > dbStartTime;
                    }
                    return false;
                });

                if (isOverlapping)
                {
                    return new BaseResultDto<CompanionAssistanceTimeDto>(false, Resource.Notification.TimesHaveOverlap, dto);
                }

                var item = mapper.Map<CompanionAssistanceTime>(dto);
                _context.CompanionAssistanceTimes.Attach(item);
                _context.Entry(item).State = EntityState.Modified;
                _context.SaveChanges();

                return new BaseResultDto(isSuccess: true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }
        public async Task<BaseResultDto<CompanionAssistanceTimeUpdateListDto>> GetListAsync(long companionAssistanceId)
        {
            var group = await _context.CompanionAssistanceTimes.Where(s => s.Deleted == false && s.CompanionAssistanceId == companionAssistanceId).OrderBy(s => s.WeekDayId).ThenBy(s => s.StartTime).GroupBy(g => g.WeekDay).ToListAsync();
            var list = mapper.Map<List<IGrouping<WeekDay, CompanionAssistanceTime>>, List<CompanionAssistanceTimeUpdateDto>>(group);
            if (list.Count < 7)
            {
                var weekDays = _weekDayService.GetWeekDays();
                foreach (var weekDay in weekDays)
                {
                    if (!list.Any(a => a.WeekDay.Id == weekDay.Id))
                    {
                        list.Add(new CompanionAssistanceTimeUpdateDto() { WeekDay = weekDay });
                    }
                }
                list = list.OrderBy(s => s.WeekDay.Id).ToList();
            }
            var result = new CompanionAssistanceTimeUpdateListDto()
            {
                CompanionAssistanceId = companionAssistanceId,
                CompanionAssistanceTimeUpdateList = list
            };
            return new BaseResultDto<CompanionAssistanceTimeUpdateListDto>(true, result);
        }

        public async Task<BaseResultDto> InsertUpdateListAsync(CompanionAssistanceTimeUpdateListDto dto, long? companionId = null)
        {
            try
            {
                if (companionId.HasValue)
                {
                    var belongsToCompanion = await _context.CompanionAssistances
                        .AsNoTracking()
                        .AnyAsync(s => s.Id == dto.CompanionAssistanceId && s.CompanionId == companionId.Value && !s.Deleted);
                    if (!belongsToCompanion)
                        return new BaseResultDto<CompanionAssistanceTimeUpdateListDto>(false, Resource.Notification.AccessDenied, dto);
                }

                foreach (var item in dto.CompanionAssistanceTimeUpdateList)
                {
                    var dbTimes = await _context.CompanionAssistanceTimes
                        .Where(s => s.CompanionAssistanceId == dto.CompanionAssistanceId
                                    && s.WeekDayId == item.WeekDay.Id
                                    && !s.Deleted)
                        .ToListAsync();

                    var newTimes = item.CompanionAssistanceTimes.Where(s => s.Id == 0).ToList();

                    foreach (var newTime in newTimes)
                    {
                        var startTime = TimeSpan.Parse(newTime.StartTime);
                        var endTime = TimeSpan.Parse(newTime.EndTime);

                        var hasOverlap = dbTimes.Any(db =>
                        {
                            var dbStart = TimeSpan.Parse(db.StartTime);
                            var dbEnd = TimeSpan.Parse(db.EndTime);
                            return startTime < dbEnd && endTime > dbStart;
                        });

                        if (hasOverlap)
                            return new BaseResultDto<CompanionAssistanceTimeUpdateListDto>(false, Resource.Notification.TimesHaveOverlap, dto);

                        foreach (var other in newTimes.Where(x => x != newTime))
                        {
                            var oStart = TimeSpan.Parse(other.StartTime);
                            var oEnd = TimeSpan.Parse(other.EndTime);
                            if (startTime < oEnd && endTime > oStart)
                                return new BaseResultDto<CompanionAssistanceTimeUpdateListDto>(false, Resource.Notification.TimesHaveOverlap, dto);
                        }
                    }
                }

                foreach (var item in dto.CompanionAssistanceTimeUpdateList)
                {
                    foreach (var item2 in item.CompanionAssistanceTimes.Where(s => s.Id == 0))
                    {
                        item2.Active = true;
                        var i = mapper.Map<CompanionAssistanceTime>(item2);
                        await _context.CompanionAssistanceTimes.AddAsync(i);
                    }
                }

                var existList = dto.CompanionAssistanceTimeUpdateList
                    .SelectMany(s => s.CompanionAssistanceTimes.Where(a => a.Id > 0).Select(a => a.Id))
                    .ToList();

                await _context.CompanionAssistanceTimes
                    .Where(s => s.CompanionAssistanceId == dto.CompanionAssistanceId && !existList.Contains(s.Id))
                    .ExecuteUpdateAsync(s => s.SetProperty(a => a.Deleted, true));

                await _context.SaveChangesAsync();
                return new BaseResultDto(true);
            }
            catch
            {
                return new BaseResultDto(false);
            }
        }
        public async Task<BaseResultDto> ActiveAsync(CompanionAssistanceTimeDto dto)
        {
            var item = await _context.CompanionAssistanceTimes.FirstOrDefaultAsync(s => s.Id == dto.Id);
            if (item == null)
            {
                item.Active = dto.Active;
                _context.CompanionAssistanceTimes.Update(item);
                _context.SaveChanges();
                return new BaseResultDto(true);
            }
            return new BaseResultDto(false);

        }
    }
}
