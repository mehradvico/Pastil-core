using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Dto;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Iface;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Services.FinanceSrvs.FinanceCompanionSrv
{
    public class FinanceCompanionService : IFinanceCompanionService
    {
        private readonly IDataBaseContext _context;

        public FinanceCompanionService(IDataBaseContext context)
        {
            _context = context;
        }

        public BaseResultDto<FinanceCompanionVDto> Search(FinanceCompanionInputDto dto)
        {
            try
            {
                if (dto == null)
                    return new BaseResultDto<FinanceCompanionVDto>(false, null);

                if (dto.CompanionId <= 0)
                    return new BaseResultDto<FinanceCompanionVDto>(false, null);

                var companionReservesQ = _context.CompanionReserves
                    .AsNoTracking()
                    .Where(r => r.IsReserved && !r.IsCancel)
                    .Where(r => r.CompanionAssistance.CompanionId == dto.CompanionId)
                    .Include(r => r.Booker)
                    .Include(r => r.State)
                    .Include(r => r.CompanionAssistance)
                    .AsQueryable();

                if (dto.Permitted.HasValue)
                    companionReservesQ = companionReservesQ.Where(r => r.Permitted == dto.Permitted.Value);

                var companionReserves = companionReservesQ
                    .Select(r => new FinanceCompanionReserveVDto
                    {
                        ReserveId = r.Id.ToString(),
                        BookerFullName = (r.Booker != null ? ((r.Booker.FirstName ?? "") + " " + (r.Booker.LastName ?? "")).Trim() : ""),
                        PaymentPrice = r.PaymentPrice,
                        CommissionPercent = r.CompanionAssistance != null ? r.CompanionAssistance.CommissionPercent : 0,
                        CompanionShare = r.CompanionShare,
                        SiteShare = r.SiteShare,
                        StatusLabel = r.State != null ? r.State.Name : null,
                        IsPansion = false,
                        Permitted = r.Permitted
                    })
                    .ToList();

                var pansionReservesQ = _context.PansionReserves
                    .AsNoTracking()
                    .Where(r => r.IsReserved && !r.IsCancel)
                    .Where(r => r.Pansion.CompanionId == dto.CompanionId)
                    .Include(r => r.Booker)
                    .Include(r => r.Status)
                    .Include(r => r.Pansion)
                    .AsQueryable();

                if (dto.Permitted.HasValue)
                    pansionReservesQ = pansionReservesQ.Where(r => r.Permitted == dto.Permitted.Value);

                var pansionReserves = pansionReservesQ
                    .Select(r => new FinanceCompanionReserveVDto
                    {
                        ReserveId = r.Id.ToString(),
                        BookerFullName = (r.Booker != null ? ((r.Booker.FirstName ?? "") + " " + (r.Booker.LastName ?? "")).Trim() : ""),
                        PaymentPrice = r.PaymentPrice,
                        CommissionPercent = r.DayCount > 0 ? r.Pansion.DailyCommissionPercent : r.Pansion.HourlyCommissionPercent,
                        CompanionShare = r.CompanionShare,
                        SiteShare = r.SiteShare,
                        StatusLabel = r.Status != null ? r.Status.Name : null,
                        IsPansion = true,
                        Permitted = r.Permitted
                    })
                    .ToList();

                var list = companionReserves.Concat(pansionReserves).ToList();

                var res = new FinanceCompanionVDto
                {
                    CompanionId = dto.CompanionId,
                    ReserveCount = list.Count,
                    TotalCompanionShare = list.Sum(x => x.CompanionShare),
                    TotalSiteShare = list.Sum(x => x.SiteShare),
                    FinanceCompanionReserves = list
                };

                return new BaseResultDto<FinanceCompanionVDto>(true, res);
            }
            catch
            {
                return new BaseResultDto<FinanceCompanionVDto>(false, null);
            }
        }
    }
}