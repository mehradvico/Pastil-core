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
