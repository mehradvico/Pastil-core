using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.Content.StoryGroupSrv.Dto;
using Application.Services.Content.StoryGroupSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Content.StoryGroupSrv
{
    public class StoryGroupService : CommonSrv<StoryGroup, StoryGroupDto>, IStoryGroupService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        private readonly IQueryable<StoryGroup> _baseQuery;

        public StoryGroupService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
            this._baseQuery = _context.StoryGroups.Where(s => !s.Deleted);
        }

        // گروه‌های فعال صفحه‌ی اصلی، فقط آن‌هایی که استوری فعال و منقضی‌نشده دارند، با اولین استوری (جدیدترین، همان ترتیب پیش‌فرض لیست آیتم‌ها)
        public async Task<List<StoryGroupHomeVDto>> HomeAsync()
        {
            var now = DateTime.Now;
            var groups = await _context.StoryGroups.AsNoTracking().Include(s => s.Picture)
                .Where(s => !s.Deleted && s.Active)
                .OrderByDescending(s => s.Id)
                .ToListAsync();
            if (groups.Count == 0)
                return new List<StoryGroupHomeVDto>();

            var groupIds = groups.Select(s => s.Id).ToList();
            var liveItems = _context.StoryItems.AsNoTracking()
                .Where(s => !s.Deleted && s.Active && s.ExpireDate > now && groupIds.Contains(s.StoryGroupId));
            var counts = await liveItems.GroupBy(s => s.StoryGroupId)
                .Select(g => new { g.Key, Count = g.Count(), FirstId = g.Max(s => s.Id) })
                .ToListAsync();
            var firstIds = counts.Select(c => c.FirstId).ToList();
            var firstItems = await _context.StoryItems.AsNoTracking()
                .Include(s => s.Picture).Include(s => s.File).Include(s => s.Companion).Include(s => s.Pansion).Include(s => s.Store)
                .Where(s => firstIds.Contains(s.Id))
                .ToListAsync();

            return groups
                .Select(group =>
                {
                    var count = counts.FirstOrDefault(c => c.Key == group.Id);
                    return new StoryGroupHomeVDto
                    {
                        Group = mapper.Map<StoryGroupVDto>(group),
                        ItemCount = count?.Count ?? 0,
                        FirstItem = count == null ? null : mapper.Map<Application.Services.Content.StoryItemSrv.Dto.StoryItemVDto>(firstItems.FirstOrDefault(s => s.Id == count.FirstId))
                    };
                })
                .Where(s => s.ItemCount > 0)
                .ToList();
        }

        public async Task<BaseResultDto<StoryGroupVDto>> FindAsyncVDto(long id)
        {

            var item = await _context.StoryGroups.AsNoTracking().Include(s => s.Picture).FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<StoryGroupVDto>(true, mapper.Map<StoryGroupVDto>(item));
            }
            return new BaseResultDto<StoryGroupVDto>(false, mapper.Map<StoryGroupVDto>(item));
        }

        public BaseSearchDto<StoryGroupVDto> Search(StoryGroupInputDto searchDto)
        {
            var query = _context.StoryGroups.AsNoTracking().Include(s => s.Picture).Where(s => !s.Deleted).AsQueryable();

            if (searchDto.Available.HasValue)
            {
                query = query.Where(s => s.Active == searchDto.Available.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchDto.Q))
            {
                var q = searchDto.Q.Trim();
                query = query.Where( s => s.Name.Contains(q));
            }

            switch (searchDto.SortBy)
            {
                case Common.Enumerable.SortEnum.Old:
                    query = query.OrderBy(s => s.Id);
                    break;

                case Common.Enumerable.SortEnum.Name:
                    query = query.OrderBy(s => s.Name);
                    break;

                case Common.Enumerable.SortEnum.MorePriority:
                    query = query
                        .OrderByDescending(s => s.Priority)
                        .ThenByDescending(s => s.Id);
                    break;

                case Common.Enumerable.SortEnum.LessPriority:
                    query = query
                        .OrderBy(s => s.Priority)
                        .ThenByDescending(s => s.Id);
                    break;

                case Common.Enumerable.SortEnum.New:
                default:
                    query = query.OrderByDescending(s => s.Id);
                    break;
            }

            return new BaseSearchDto<StoryGroup, StoryGroupVDto>(searchDto, query, mapper);
        }


    }
}
