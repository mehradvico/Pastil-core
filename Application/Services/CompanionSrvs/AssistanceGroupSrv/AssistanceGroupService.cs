using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.CompanionSrvs.AssistanceGroupSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceGroupSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.AssistanceGroupSrv
{
    public class AssistanceGroupService :
        CommonSrv<AssistanceGroup, AssistanceGroupDto>,
        IAssistanceGroupService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        public AssistanceGroupService(
            IDataBaseContext context,
            IMapper mapper)
            : base(context, mapper)
        {
            _context = context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<AssistanceGroupVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.AssistanceGroups
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);

            if (item == null)
            {
                return new BaseResultDto<AssistanceGroupVDto>(
                    false,
                    mapper.Map<AssistanceGroupVDto>(item));
            }

            return new BaseResultDto<AssistanceGroupVDto>(
                true,
                mapper.Map<AssistanceGroupVDto>(item));
        }

        public AssistanceGroupSearchDto Search(AssistanceGroupInputDto searchDto)
        {
            var query = _context.AssistanceGroups
                .AsNoTracking()
                .Where(x => !x.Deleted)
                .AsQueryable();

            if (searchDto.Available.HasValue)
            {
                query = query.Where(x => x.Active == searchDto.Available.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchDto.Q))
            {
                var q = searchDto.Q.Trim();
                query = query.Where(x => x.Name.Contains(q));
            }

            query = searchDto.SortBy switch
            {
                Common.Enumerable.SortEnum.Old =>
                    query.OrderBy(x => x.Id),
                Common.Enumerable.SortEnum.Name =>
                    query.OrderBy(x => x.Name),
                Common.Enumerable.SortEnum.MorePriority =>
                    query.OrderByDescending(x => x.Priority)
                        .ThenByDescending(x => x.Id),
                Common.Enumerable.SortEnum.LessPriority =>
                    query.OrderBy(x => x.Priority)
                        .ThenByDescending(x => x.Id),
                _ =>
                    query.OrderByDescending(x => x.Id)
            };

            return new AssistanceGroupSearchDto(searchDto, query, mapper);
        }
    }
}
