using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.WeekDaySrv.WeekDaySrv.Dto;
using Application.Services.WeekDaySrv.WeekDaySrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.WeekDaySrv.WeekDaySrv
{
    public class WeekDayService : CommonSrv<WeekDay, WeekDayDto>, IWeekDayService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public WeekDayService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }
        public async Task<BaseResultDto<WeekDayVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.WeekDays.FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<WeekDayVDto>(true, mapper.Map<WeekDayVDto>(item));
            }
            return new BaseResultDto<WeekDayVDto>(false, mapper.Map<WeekDayVDto>(item));
        }

        public WeekDaySearchDto Search(WeekDayInputDto baseSearchDto)
        {
            var model = _context.WeekDays.AsQueryable();

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
            return new WeekDaySearchDto(baseSearchDto, model, mapper);
        }

        public override async Task<BaseResultDto<WeekDayDto>> InsertAsyncDto(WeekDayDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<WeekDayDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                {
                    return modelCheker;
                }

                bool exists = await _context.WeekDays.AnyAsync(a => a.Name == dto.Name);

                if (exists)
                {
                    return new BaseResultDto<WeekDayDto>(false, Resource.Notification.DuplicateValue, dto);
                }

                var item = mapper.Map<WeekDay>(dto);
                await _context.WeekDays.AddAsync(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<WeekDayDto>(true, mapper.Map<WeekDayDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<WeekDayDto>(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex), data: dto);
            }
        }
        public List<WeekDayDto> GetWeekDays()
        {
            return mapper.Map<List<WeekDayDto>>(_context.WeekDays.OrderBy(s => s.Number));
        }
    }
}
