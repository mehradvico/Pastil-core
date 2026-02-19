using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Services.Accounting.FinanceSrv.Dto;
using Application.Services.Accounting.FinanceSrv.Iface;
using Application.Services.CompanionSrvs.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.FinanceSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using AutoMapper;
using DocumentFormat.OpenXml.Vml.Office;
using Entities.Entities;
using Entities.Entities.PansionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

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
            var storesQ = _context.Stores.Include(s => s.Picture).Where(s => !s.Deleted).AsQueryable();
            var companionsQ = _context.Companions.Include(c => c.Picture).Include(c => c.Owner).Where(c => !c.Deleted).AsQueryable();

            if (dto.Available.HasValue)
            {
                storesQ = storesQ.Where(s => s.Active == dto.Available);
                companionsQ = companionsQ.Where(c => c.Active && c.Approved == dto.Available);
            }
            if (!string.IsNullOrEmpty(dto.Q))
            {
                storesQ = storesQ.Where(s => s.Name.Contains(dto.Q));
                companionsQ = companionsQ.Where(c => c.Name.Contains(dto.Q));
            }

            switch (dto.SortBy)
            {
                case Common.Enumerable.SortEnum.New:
                    storesQ = storesQ.OrderByDescending(s => s.Id);
                    companionsQ = companionsQ.OrderByDescending(c => c.Id);
                    break;

                case Common.Enumerable.SortEnum.Old:
                    storesQ = storesQ.OrderBy(s => s.Id);
                    companionsQ = companionsQ.OrderBy(c => c.Id);
                    break;
            }

            if (dto.IsCompanion.HasValue)
            {
                if (dto.IsCompanion.Value)
                    storesQ = Enumerable.Empty<Store>().AsQueryable();
                else
                    companionsQ = Enumerable.Empty<Companion>().AsQueryable();
            }

            var stores = storesQ.Select(s => mapper.Map<StoreFinanceVDto>(s)).ToList();
            if (dto.HasCommission.HasValue)
            {
                stores = dto.HasCommission.Value ? stores.Where(s => s.CommissionPercent != 0).ToList() : stores.Where(s => s.CommissionPercent == 0).ToList();
            }

            var companionIds = companionsQ.Select(c => c.Id).ToList();

            var assistanceStats = _context.CompanionAssistances.Where(a => companionIds.Contains(a.CompanionId) && !a.Deleted).GroupBy(a => a.CompanionId)
                .Select(g => new
                {
                    CompanionId = g.Key,
                    Total = g.Count(),
                    With = g.Count(x => x.CommissionPercent > 0)
                })
                .ToList().ToDictionary(x => x.CompanionId);

            var pansionStats = _context.Pansions.Where(p => companionIds.Contains(p.CompanionId)).GroupBy(p => p.CompanionId)
                .Select(g => new
                {
                    CompanionId = g.Key,
                    Has = 1,
                    With = g.Any(x =>
                        x.DailyCommissionPercent > 0 &&
                        x.HourlyCommissionPercent > 0) ? 1 : 0

                }).ToList().ToDictionary(x => x.CompanionId);

            var companions = companionsQ.AsEnumerable()
                .Select(c =>
                {
                    var dtoC = mapper.Map<CompanionFinanceVDto>(c);

                    assistanceStats.TryGetValue(c.Id, out var a);
                    pansionStats.TryGetValue(c.Id, out var p);

                    var totalItems = (a?.Total ?? 0) + (p?.Has ?? 0);
                    var withItems = (a?.With ?? 0) + (p?.With ?? 0);

                    dtoC.TotalItemsCount = totalItems;
                    dtoC.ItemsWithCommissionCount = withItems;
                    dtoC.HasCommission = totalItems > 0 && totalItems == withItems;

                    return dtoC;

                }).ToList();

            if (dto.HasCommission.HasValue)
            {
                companions = companions.Where(c => c.HasCommission == dto.HasCommission.Value).ToList();
            }

            return new FinanceSearchDto { Stores = stores, Companions = companions };
        }


        public CompanionFinanceDetailVDto SearchCompanionDetail(long companionId)
        {
            var companion = _context.Companions.Include(c => c.Picture).Include(c => c.Owner).FirstOrDefault(c => c.Id == companionId && !c.Deleted);
            if (companion == null) 
                return null;

            var pansions = _context.Pansions.Include(p => p.Picture).Where(p => p.CompanionId == companionId).ToList();

            var assistances = _context.CompanionAssistances.Include(a => a.Assistance).Where(a => a.CompanionId == companionId && !a.Deleted).ToList();

            var result = mapper.Map<CompanionFinanceDetailVDto>(companion);
            result.Pansions = pansions.Select(p => mapper.Map<PansionFinanceVDto>(p)).ToList();
            result.CompanionAssistances = assistances.Select(a => mapper.Map<CompanionAssistanceFinanceVDto>(a)).ToList();

            var pansionHas = result.Pansions != null && result.Pansions.Count > 0 ? 1 : 0;
            var pansionWith = (pansionHas == 1 && result.Pansions.Any(p => p.DailyCommissionPercent > 0 && p.HourlyCommissionPercent > 0)) ? 1 : 0;

            var assistanceTotal = result.CompanionAssistances?.Count ?? 0;
            var assistanceWith = result.CompanionAssistances?.Count(a => a.CommissionPercent > 0) ?? 0;

            result.TotalItemsCount = assistanceTotal + pansionHas;
            result.ItemsWithCommissionCount = assistanceWith + pansionWith;
            result.HasCommission = result.TotalItemsCount > 0 && result.TotalItemsCount == result.ItemsWithCommissionCount;

            return result;
        }


        public async Task<BaseResultDto> UpdateStoreCommissionAsyncDto(FinanceStoreDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<FinanceStoreDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                    return modelCheker;

                var item = await _context.Stores.FirstOrDefaultAsync(x => x.Id == dto.StoreId && !x.Deleted);

                if (item == null)
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.NothingFound);

                item.CommissionPercent = dto.CommissionPercent;

                _context.Stores.Update(item);
                await _context.SaveChangesAsync();
                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }


        public async Task<BaseResultDto> UpdateCompanionAssistanceCommissionAsyncDto(FinanceCompanionAssistanceDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<FinanceCompanionAssistanceDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                    return modelCheker;

                var item = await _context.CompanionAssistances.FirstOrDefaultAsync(x => x.Id == dto.CompanionAssistanceId && !x.Deleted);

                if (item == null)
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.NothingFound);
                    
                item.CommissionPercent = dto.CommissionPercent;

                _context.CompanionAssistances.Update(item);
                await _context.SaveChangesAsync();
                return new BaseResultDto(isSuccess: true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: ex.Message);
            }
        }


        public async Task<BaseResultDto> UpdatePansionCommissionAsyncDto(FinancePansionDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<FinancePansionDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                    return modelCheker;

                var item = await _context.Pansions.FirstOrDefaultAsync(x => x.Id == dto.PansionId);

                if (item == null)
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.NothingFound);

                item.DailyCommissionPercent = dto.DailyCommissionPercent;
                item.HourlyCommissionPercent = dto.HourlyCommissionPercent;

                _context.Pansions.Update(item);
                await _context.SaveChangesAsync();
                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ex.Message);
            }
        }

    }
}
