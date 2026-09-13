using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.Content.StoryItemSrv.Dto;
using Application.Services.Content.StoryItemSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Content.StoryItemSrv
{
    public class StoryItemService : CommonSrv<StoryItem, StoryItemDto>, IStoryItemService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IQueryable<StoryItem> _baseQuery;


        public StoryItemService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._baseQuery = _context.StoryItems.Where(s => !s.Deleted);
        }

        public async Task<BaseResultDto<StoryItemVDto>> FindAsyncVDto(long id, bool view = true)
        {

            var item = await _baseQuery.Include(s => s.Picture).Include(s => s.Companion).Include(s => s.Pansion).Include(s => s.Store).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                if (view)
                {
                    item.ViewCount++;
                    _context.StoryItems.Update(item);
                    await _context.SaveChangesAsync();
                }
                return new BaseResultDto<StoryItemVDto>(true, mapper.Map<StoryItemVDto>(item));
            }
            return new BaseResultDto<StoryItemVDto>(false, mapper.Map<StoryItemVDto>(item));
        }

        public async Task<BaseResultDto<StoryItemVDto>> FindAsyncAdminVDto(long id)
        {

            var item = await _baseQuery.Include(s => s.Picture).Include(s => s.Companion).Include(s => s.Pansion).Include(s => s.Store).FirstOrDefaultAsync(s => s.Id == id);
            if (item != null)
            {
                return new BaseResultDto<StoryItemVDto>(true, mapper.Map<StoryItemVDto>(item));
            }
            return new BaseResultDto<StoryItemVDto>(false, mapper.Map<StoryItemVDto>(item));
        }

        public BaseSearchDto<StoryItemVDto> Search(StoryItemInputDto searchDto)
        {
            var now = DateTime.Now;
            // Picture/File قبلاً اینجا Include نمی‌شدن؛ یعنی حتی با PictureId/FileId معتبر،
            // نویگیشن‌شون توی نتیجه‌ی لیست همیشه null برمی‌گشت و پنل هیچ تصویری نداشت نشون بده.
            var query = _context.StoryItems.Include(s => s.Companion).Include(s => s.Pansion).Include(s => s.Store).Include(s => s.StoryGroup).Include(s => s.Picture).Include(s => s.File).AsQueryable().Where(s => !s.Deleted);

            if (searchDto.Available.HasValue)
                query = query.Where(s => s.Active == searchDto.Available);

            if (searchDto.Expired.HasValue)
            {
                if (searchDto.Expired.Value)
                {
                    query = query.Where(s => s.ExpireDate <= now);
                }
                else
                {
                    query = query.Where(s => s.ExpireDate > now);
                }
            }

            if (searchDto.StoryGroupId.HasValue)
                query = query.Where(s => s.StoryGroupId == searchDto.StoryGroupId.Value);

            if (searchDto.PansionId.HasValue)
                query = query.Where(s => s.PansionId == searchDto.PansionId.Value);

            if (searchDto.CompanionId.HasValue)
                query = query.Where(s => s.CompanionId == searchDto.CompanionId.Value);

            if (searchDto.StoreId.HasValue)
                query = query.Where(s => s.StoreId == searchDto.StoreId.Value);

            if (searchDto.SortBy != Common.Enumerable.SortEnum.Default)
            {
                switch (searchDto.SortBy)
                {
                    case Common.Enumerable.SortEnum.New:
                        query = query.OrderByDescending(s => s.Id);
                        break;
                    case Common.Enumerable.SortEnum.Old:
                        query = query.OrderBy(s => s.Id);
                        break;
                    case Common.Enumerable.SortEnum.MorePriority:
                        query = query.OrderByDescending(s => s.Priority);
                        break;
                    case Common.Enumerable.SortEnum.LessPriority:
                        query = query.OrderBy(s => s.Priority);
                        break;
                }
            }

            return new BaseSearchDto<StoryItem, StoryItemVDto>(searchDto, query, mapper);
        }


        // تمدید استوری منقضی‌شده: چون UpdateDto عمومی هیچ‌جا ExpireDate/CreateDate رو
        // دوباره از روی DayCount محاسبه نمی‌کنه (فقط InsertAsyncDto این کارو موقع ساخت
        // انجام می‌ده)، ادمین قبلاً هیچ راهی برای زنده‌کردن یه استوری منقضی‌شده نداشت.
        public async Task<BaseResultDto<StoryItemDto>> RenewAsyncDto(StoryItemRenewDto dto)
        {
            try
            {
                var item = await _context.StoryItems.AsTracking().FirstOrDefaultAsync(s => s.Id == dto.Id && !s.Deleted);
                if (item == null)
                    return new BaseResultDto<StoryItemDto>(isSuccess: false, val: Resource.Notification.NothingFound, data: null);

                var dayCount = dto.DayCount ?? item.DayCount;
                if (dayCount <= 0)
                    return new BaseResultDto<StoryItemDto>(isSuccess: false, val: Resource.Notification.PleaseEnterDayCount, data: null);

                item.DayCount = dayCount;
                item.CreateDate = DateTime.Now;
                item.ExpireDate = item.CreateDate.AddDays(dayCount);

                _context.StoryItems.Update(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<StoryItemDto>(true, mapper.Map<StoryItemDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<StoryItemDto>(isSuccess: false, val: ex.Message, data: null);
            }
        }

        public override async Task<BaseResultDto<StoryItemDto>> InsertAsyncDto(StoryItemDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<StoryItemDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                    return modelCheker;

                var hasPicture = dto.PictureId.HasValue && dto.PictureId.Value > 0;
                var hasFile = dto.FileId.HasValue && dto.FileId.Value > 0;

                if (!hasPicture && !hasFile)
                    return new BaseResultDto<StoryItemDto>(false, Resource.Notification.PleaseUploadAMedia, dto);

                if (dto.DayCount <= 0)
                    return new BaseResultDto<StoryItemDto>(false, Resource.Notification.PleaseEnterDayCount, dto);

                var item = mapper.Map<StoryItem>(dto);

                item.CreateDate = DateTime.Now;
                item.ExpireDate = item.CreateDate.AddDays(item.DayCount);

                await _context.StoryItems.AddAsync(item);
                await _context.SaveChangesAsync();

                return new BaseResultDto<StoryItemDto>(true, mapper.Map<StoryItemDto>(item));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<StoryItemDto>(isSuccess: false, val: ex.Message, data: dto);
            }
        }


    }
}
