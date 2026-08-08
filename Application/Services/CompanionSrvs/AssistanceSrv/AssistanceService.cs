using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceSrv.Iface;
using Application.Services.Filing.PictureSrv.Dto;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.AssistanceSrv
{
    public class AssistanceService : CommonSrv<Assistance, AssistanceDto>, IAssistanceService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public AssistanceService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<AssistanceVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.Assistances
                .AsNoTracking()
                .Include(s => s.AssistanceGroup)
                .Include(s => s.Picture)
                .FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);

            if (item != null)
            {
                return new BaseResultDto<AssistanceVDto>(true, mapper.Map<AssistanceVDto>(item));
            }

            return new BaseResultDto<AssistanceVDto>(false, mapper.Map<AssistanceVDto>(item));
        }

        public AssistanceSearchDto Search(AssistanceInputDto baseSearchDto)
        {
            var model = _context.Assistances
                .AsNoTracking()
                .Include(s => s.AssistanceGroup)
                .Include(s => s.Picture)
                .Where(s => !s.Deleted)
                .AsQueryable();

            if (baseSearchDto.Available == true)
            {
                model = model.Where(
                    s => s.Active &&
                         (!s.AssistanceGroupId.HasValue ||
                          (s.AssistanceGroup.Active &&
                           !s.AssistanceGroup.Deleted)));
            }
            else if (baseSearchDto.Available == false)
            {
                model = model.Where(s => !s.Active);
            }

            if (baseSearchDto.IsPersonal.HasValue)
            {
                model = model.Where(s => s.IsPersonal == baseSearchDto.IsPersonal.Value);
            }

            if (baseSearchDto.AssistanceGroupId.HasValue)
            {
                model = model.Where(
                    s => s.AssistanceGroupId == baseSearchDto.AssistanceGroupId.Value);
            }

            if (!string.IsNullOrWhiteSpace(baseSearchDto.Q))
            {
                var q = baseSearchDto.Q.Trim();
                model = model.Where(
                    s => s.Name.Contains(q) ||
                         (s.Summary != null && s.Summary.Contains(q)) ||
                         (s.Description != null && s.Description.Contains(q)));
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

            return new AssistanceSearchDto(baseSearchDto, model, mapper);
        }

        public async Task<List<SearchAssistanceDto>> SearchMinAsync(SearchRequestDto request)
        {
            var list = await _context.Assistances.Where(s => s.Deleted == false && s.Active && s.Name.Contains(request.Q)).Take(request.AssistanceCount).Select(s => new SearchAssistanceDto { Id = s.Id, Name = s.Name, PictureId = s.PictureId, Picture = mapper.Map<PictureVDto>(s.Picture) }).ToListAsync();
            return list;
        }
    }
}
