using Application.Common.Dto.Result;
using Application.Common.Enumerable;
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
                        ReserveCode = r.ReserveCode,
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
                        ReserveCode = r.ReserveCode,
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

                // مشاوره‌های آنلاین: فقط «تکمیل‌شده» (پنجره تمام شده و دیگر قابل بازپرداخت نیست) سهم قابل تسویه دارد
                var completedStatus = (int)ConsultationPurchaseStatusEnum.Completed;
                var consultationQ = _context.ConsultationPurchases
                    .AsNoTracking()
                    .Where(p => p.CompanionId == dto.CompanionId && p.Status == completedStatus)
                    .AsQueryable();

                if (dto.Permitted.HasValue)
                    consultationQ = consultationQ.Where(p => p.Permitted == dto.Permitted.Value);

                var consultations = consultationQ
                    .Select(p => new FinanceCompanionReserveVDto
                    {
                        ReserveId = p.Id.ToString(),
                        ReserveCode = p.PurchaseCode,
                        BookerFullName = ((p.User.FirstName ?? "") + " " + (p.User.LastName ?? "")).Trim(),
                        PaymentPrice = p.PaymentPrice,
                        CommissionPercent = p.PaymentPrice > 0 ? (decimal)(p.SiteShare / p.PaymentPrice * 100) : 0,
                        CompanionShare = p.CompanionShare,
                        SiteShare = p.SiteShare,
                        StatusLabel = null,
                        IsPansion = false,
                        IsConsultation = true,
                        PackageName = p.PackageName,
                        DurationMinutes = p.DurationMinutes,
                        ChannelId = p.ChannelId,
                        Permitted = p.Permitted
                    })
                    .ToList();

                // ثبت‌نام دوره‌های مدرسه‌ی کلینیک (پرداخت‌شده و لغو نشده)
                var schoolQ = _context.SchoolReserves
                    .AsNoTracking()
                    .Where(r => r.IsReserved && !r.IsCancel && r.SchoolCourse.School.CompanionId == dto.CompanionId)
                    .AsQueryable();

                if (dto.Permitted.HasValue)
                    schoolQ = schoolQ.Where(r => r.Permitted == dto.Permitted.Value);

                var schools = schoolQ
                    .Select(r => new FinanceCompanionReserveVDto
                    {
                        ReserveId = r.Id.ToString(),
                        ReserveCode = r.ReserveCode,
                        BookerFullName = ((r.Booker.FirstName ?? "") + " " + (r.Booker.LastName ?? "")).Trim(),
                        PaymentPrice = r.PaymentPrice,
                        CommissionPercent = r.SchoolCourse.CommissionPercent,
                        CompanionShare = r.CompanionShare,
                        SiteShare = r.SiteShare,
                        StatusLabel = null,
                        IsPansion = false,
                        IsSchool = true,
                        PackageName = r.SchoolCourse.Name,
                        Permitted = r.Permitted
                    })
                    .ToList();

                var list = companionReserves.Concat(pansionReserves).Concat(consultations).Concat(schools).ToList();

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
