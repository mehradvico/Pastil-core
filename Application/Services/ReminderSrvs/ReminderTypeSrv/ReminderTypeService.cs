using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Service;
using Application.Services.ReminderSrvs.ReminderTypeSrv.Dto;
using Application.Services.ReminderSrvs.ReminderTypeSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ReminderSrvs.ReminderTypeSrv
{
    public class ReminderTypeService : CommonSrv<ReminderType, ReminderTypeDto>, IReminderTypeService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public ReminderTypeService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }
        public async Task<BaseResultDto<ReminderTypeVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.ReminderTypes.FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<ReminderTypeVDto>(true, mapper.Map<ReminderTypeVDto>(item));
            }
            return new BaseResultDto<ReminderTypeVDto>(false, mapper.Map<ReminderTypeVDto>(item));
        }

        public override async Task<BaseResultDto<ReminderTypeDto>> FindAsyncDto(long id)
        {
            var item = await _context.ReminderTypes.FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<ReminderTypeDto>(true, mapper.Map<ReminderTypeDto>(item));
            }
            return new BaseResultDto<ReminderTypeDto>(false, mapper.Map<ReminderTypeDto>(item));
        }

        public ReminderTypeSearchDto Search(ReminderTypeInputDto baseSearchDto)
        {
            var model = _context.ReminderTypes.AsQueryable().Where(s => !s.Deleted);

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
            return new ReminderTypeSearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<ReminderTypeDto>> InsertAsyncDto(ReminderTypeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return new BaseResultDto<ReminderTypeDto>(false, Resource.Notification.InvalidData, dto);

            var name = dto.Name.Trim();
            var duplicate = await _context.ReminderTypes.AsNoTracking().AnyAsync(item =>
                !item.Deleted && item.Name == name);
            if (duplicate)
                return new BaseResultDto<ReminderTypeDto>(false, Resource.Notification.DuplicateValue, dto);

            var item = new ReminderType { Name = name, Deleted = false };
            await _context.ReminderTypes.AddAsync(item);
            await _context.SaveChangesAsync();
            return new BaseResultDto<ReminderTypeDto>(true, mapper.Map<ReminderTypeDto>(item));
        }

        public override BaseResultDto UpdateDto(ReminderTypeDto dto)
        {
            if (dto.Id <= 0 || string.IsNullOrWhiteSpace(dto.Name))
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var item = _context.ReminderTypes.FirstOrDefault(type => type.Id == dto.Id && !type.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            var name = dto.Name.Trim();
            var duplicate = _context.ReminderTypes.AsNoTracking().Any(type =>
                type.Id != dto.Id &&
                !type.Deleted &&
                type.Name == name);
            if (duplicate)
                return new BaseResultDto(false, Resource.Notification.DuplicateValue);

            item.Name = name;
            _context.SaveChanges();
            return new BaseResultDto(true);
        }

        public override BaseResultDto DeleteDto(long id)
        {
            var item = _context.ReminderTypes.FirstOrDefault(type => type.Id == id && !type.Deleted);
            if (item == null)
                return new BaseResultDto(false, Resource.Notification.NothingFound);

            var inUse = _context.Reminders.AsNoTracking().Any(reminder =>
                !reminder.Deleted && reminder.ReminderTypeId == id);
            if (inUse)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            item.Deleted = true;
            _context.SaveChanges();
            return new BaseResultDto(true);
        }
    }
}
