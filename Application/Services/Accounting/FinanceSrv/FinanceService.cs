using Application.Services.Accounting.FinanceSrv.Dto;
using Application.Services.Accounting.FinanceSrv.Iface;
using Application.Services.CompanionSrvs.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.FinanceSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Linq;

namespace Application.Services.FinanceSrv
{
    public class FinanceService : IFinanceService
    {
        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        public FinanceService(IDataBaseContext _context, IMapper mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        public FinanceSearchDto Search(FinanceInputDto dto)
        {
            var stores = _context.Stores.Include(s => s.Picture).Where(s => !s.Deleted).AsQueryable();

            var companions = _context.Companions.Include(c => c.Picture).Include(c => c.Owner).Where(c => !c.Deleted).AsQueryable();

            if (dto.Available.HasValue)
            {
                stores = stores.Where(s => s.Active == dto.Available);
                companions = companions.Where(c => c.Active && c.Approved == dto.Available);
            }

            if (!string.IsNullOrEmpty(dto.Q))
            {
                stores = stores.Where(s => s.Name.Contains(dto.Q));
                companions = companions.Where(c => c.Name.Contains(dto.Q));
            }
            if (dto.IsCompanion.HasValue)
            {
                if (dto.IsCompanion.Value)
                {
                    stores = Enumerable.Empty<Store>().AsQueryable();
                }
                else
                {
                    companions = Enumerable.Empty<Companion>().AsQueryable();
                }
            }
            switch (dto.SortBy)
            {
                case Common.Enumerable.SortEnum.New:
                    stores = stores.OrderByDescending(s => s.Id);
                    companions = companions.OrderByDescending(c => c.Id);
                    break;

                case Common.Enumerable.SortEnum.Old:
                    stores = stores.OrderBy(s => s.Id);
                    companions = companions.OrderBy(c => c.Id);
                    break;

                default:
                    break;
            }
            return new FinanceSearchDto(dto, stores, companions, mapper);
        }

        public CompanionFinanceDetailVDto SearchCompanionDetail(long companionId)
        {
            var companion = _context.Companions.Include(c => c.Picture).Include(c => c.Owner).FirstOrDefault(c => c.Id == companionId && !c.Deleted);

            if (companion == null) return null;

            var pansions = _context.Pansions.Include(p => p.Picture).Where(p => p.CompanionId == companionId).ToList();

            var assistances = _context.CompanionAssistances.Include(a => a.Assistance).Where(a => a.CompanionId == companionId && !a.Deleted).ToList();

            var result = mapper.Map<CompanionFinanceDetailVDto>(companion);
            result.Pansions = pansions.Select(p => mapper.Map<PansionFinanceVDto>(p)).ToList();
            result.CompanionAssistances = assistances.Select(a => mapper.Map<CompanionAssistanceFinanceVDto>(a)).ToList();

            return result;
        }
    }
}
