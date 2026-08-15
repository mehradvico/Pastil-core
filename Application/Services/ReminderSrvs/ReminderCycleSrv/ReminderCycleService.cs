using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.ReminderSrvs.ReminderCycleSrv.Dto;
using Application.Services.ReminderSrvs.ReminderCycleSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ReminderSrvs.ReminderCycleSrv
{
    public class ReminderCycleService : CommonSrv<ReminderCycle, ReminderCycleDto>, IReminderCycleService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public ReminderCycleService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }
        public async Task<BaseResultDto<ReminderCycleVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.ReminderCycles.FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<ReminderCycleVDto>(true, mapper.Map<ReminderCycleVDto>(item));
            }
            return new BaseResultDto<ReminderCycleVDto>(false, mapper.Map<ReminderCycleVDto>(item));
        }

        public override async Task<BaseResultDto<ReminderCycleDto>> FindAsyncDto(long id)
        {
            var item = await _context.ReminderCycles.FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<ReminderCycleDto>(true, mapper.Map<ReminderCycleDto>(item));
            }
            return new BaseResultDto<ReminderCycleDto>(false, mapper.Map<ReminderCycleDto>(item));
        }

        public ReminderCycleSearchDto Search(ReminderCycleInputDto baseSearchDto)
        {
            var model = _context.ReminderCycles.AsQueryable().Where(s => !s.Deleted);

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
            return new ReminderCycleSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<ReminderCycleDto>> InsertAsyncDto(ReminderCycleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Cycle <= 0)
                return new BaseResultDto<ReminderCycleDto>(false, Resource.Notification.InvalidData, dto);

            var name = dto.Name.Trim();
            var duplicate = await _context.ReminderCycles.AsNoTracking().AnyAsync(item =>
                !item.Deleted &&
                (item.Name == name || item.Cycle == dto.Cycle));
            if (duplicate)
                return new BaseResultDto<ReminderCycleDto>(false, Resource.Notification.DuplicateValue, dto);

            var item = new ReminderCycle
            {
                Name = name,
                Cycle = dto.Cycle,
                Deleted = false
            };
            await _context.ReminderCycles.AddAsync(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto<ReminderCycleDto>(true, mapper.Map<ReminderCycleDto>(item));
        }

        public override BaseResultDto UpdateDto(ReminderCycleDto dto)
        {
            if (dto.Id <= 0 || string.IsNullOrWhiteSpace(dto.Name) || dto.Cycle <= 0)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var item = _context.ReminderCycles.FirstOrDefault(cycle => cycle.Id == dto.Id && !cycle.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            var name = dto.Name.Trim();
            var duplicate = _context.ReminderCycles.AsNoTracking().Any(cycle =>
                cycle.Id != dto.Id &&
                !cycle.Deleted &&
                (cycle.Name == name || cycle.Cycle == dto.Cycle));
            if (duplicate)
                return new BaseResultDto(false, Resource.Notification.DuplicateValue);

            item.Name = name;
            item.Cycle = dto.Cycle;
            _context.SaveChanges();
            return new BaseResultDto(true);
        }

        public override BaseResultDto DeleteDto(long id)
        {
            var item = _context.ReminderCycles.FirstOrDefault(cycle => cycle.Id == id && !cycle.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            var inUse = _context.Reminders.AsNoTracking().Any(reminder =>
                !reminder.Deleted && reminder.ReminderCycleId == id);
            if (inUse)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            item.Deleted = true;
            _context.SaveChanges();
            return new BaseResultDto(true);
        }
    }
}
