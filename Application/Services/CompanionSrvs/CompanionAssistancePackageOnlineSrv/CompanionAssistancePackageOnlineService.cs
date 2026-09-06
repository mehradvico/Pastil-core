using Application.Common.Dto.Result;
using Application.Common.Service;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSrv.Dto;
using Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionAssistancePackageOnlineSrv
{
    public class CompanionAssistancePackageOnlineService : CommonSrv<CompanionAssistancePackageOnline, CompanionAssistancePackageOnlineDto>, ICompanionAssistancePackageOnlineService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;
        public CompanionAssistancePackageOnlineService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public async Task<BaseResultDto<CompanionAssistancePackageOnlineVDto>> FindAsyncVDto(long id)
        {
            var item = await _context.CompanionAssistancePackageOnlines.FirstOrDefaultAsync(s => s.Id == id && !s.Deleted);
            if (item != null)
            {
                return new BaseResultDto<CompanionAssistancePackageOnlineVDto>(true, mapper.Map<CompanionAssistancePackageOnlineVDto>(item));
            }
            return new BaseResultDto<CompanionAssistancePackageOnlineVDto>(false, mapper.Map<CompanionAssistancePackageOnlineVDto>(item));
        }

        public CompanionAssistancePackageOnlineSearchDto Search(CompanionAssistancePackageOnlineInputDto baseSearchDto)
        {
            var model = _context.CompanionAssistancePackageOnlines.AsQueryable().Where(s => !s.Deleted);

            if (baseSearchDto.Available.HasValue)
            {
                model = model.Where(s => s.Active == baseSearchDto.Available.Value);
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
                case Common.Enumerable.SortEnum.Expensive:
                    {
                        model = model.OrderByDescending(s => s.Price);
                        break;
                    }
                case Common.Enumerable.SortEnum.Inexpensive:
                    {
                        model = model.OrderBy(s => s.Price);
                        break;
                    }
                default:
                    break;
            }
            return new CompanionAssistancePackageOnlineSearchDto(baseSearchDto, model, mapper);
        }

        public async Task<List<CompanionAssistancePackageOnline>> GetListAsync(List<long> ids)
        {
            return await _context.CompanionAssistancePackageOnlines.Where(x => ids.Contains(x.Id) && !x.Deleted).ToListAsync();
        }
    }
}
