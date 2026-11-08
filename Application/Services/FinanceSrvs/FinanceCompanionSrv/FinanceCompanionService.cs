using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Dto;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Iface;
using Application.Services.PansionSrvs.PansionOwnershipSrv;
using Entities.Entities.PansionField;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceCompanionSrv
{
    public class FinanceCompanionService : IFinanceCompanionService
    {
        private readonly IDataBaseContext _context;

        public FinanceCompanionService(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<BaseResultDto<FinanceCompanionVDto>> SearchAsync(FinanceCompanionInputDto dto)
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

                var pansionReserves = PansionRows(_context.PansionReserves.Where(r => r.Pansion.CompanionId == dto.CompanionId), dto.Permitted);

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
                        CommissionPercent = p.CommissionPercent,
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

                // سفر پت‌رسانِ متصل به هر رزرو خدمت/پانسیون/مدرسه (اگر بود) — پرداختش جدا از رزرو است،
                // فقط برای نمایش کنار هم در مدیریت مالی؛ چیزی در CompanionShare/SiteShare بالا تغییر نمی‌کند.
                var companionTripMap = await Application.Services.TripSrv.TripSrv.LinkedPetResanTripHelper.ForCompanionReservesAsync(
                    _context, companionReserves.Select(r => long.Parse(r.ReserveId)));
                foreach (var row in companionReserves)
                {
                    if (!companionTripMap.TryGetValue(long.Parse(row.ReserveId), out var trip)) continue;
                    row.HasPetResan = true;
                    row.PetResanPrice = trip.PaymentPrice > 0 ? trip.PaymentPrice : trip.Price;
                }

                await AttachPansionTripsAsync(pansionReserves);

                var schoolTripMap = await Application.Services.TripSrv.TripSrv.LinkedPetResanTripHelper.ForSchoolReservesAsync(
                    _context, schools.Select(r => long.Parse(r.ReserveId)));
                foreach (var row in schools)
                {
                    if (!schoolTripMap.TryGetValue(long.Parse(row.ReserveId), out var trip)) continue;
                    row.HasPetResan = true;
                    row.PetResanPrice = trip.PaymentPrice > 0 ? trip.PaymentPrice : trip.Price;
                }

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

        public async Task<BaseResultDto<FinancePansionVDto>> SearchPansionAsync(FinancePansionInputDto dto, long? ownerUserId)
        {
            try
            {
                var pansionId = dto?.PansionId ?? 0;
                // ادمین باید پانسیون را مشخص کند؛ مالک بدون شناسه همه‌ی پانسیون‌های خودش را می‌گیرد.
                if (!ownerUserId.HasValue && pansionId <= 0)
                    return new BaseResultDto<FinancePansionVDto>(false, Resource.Notification.InvalidData, null);

                var scope = _context.Pansions.AsNoTracking().AsQueryable();
                if (ownerUserId.HasValue)
                {
                    // خواندن گزارش مال خود به تأیید بودن ارائه‌دهنده وابسته نیست؛ پانسیون حذف‌شده هم گزارش دارد.
                    var owned = PansionOwnershipService.OwnedPansionIds(_context, ownerUserId.Value, approvedOnly: false);
                    scope = scope.Where(p => owned.Contains(p.Id));
                }
                if (pansionId > 0)
                    scope = scope.Where(p => p.Id == pansionId);

                var pansions = await scope.OrderBy(p => p.Id)
                    .Select(p => new FinancePansionInfoVDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        IsDaycare = p.IsDaycare,
                        Active = p.Active,
                        Deleted = p.Deleted,
                        ProviderId = p.ProviderId,
                        CompanionId = p.CompanionId,
                        DailyCommissionPercent = p.DailyCommissionPercent,
                        HourlyCommissionPercent = p.HourlyCommissionPercent,
                        HasCommission = p.DailyCommissionPercent > 0 && p.HourlyCommissionPercent > 0
                    })
                    .ToListAsync();
                if (pansions.Count == 0)
                    return new BaseResultDto<FinancePansionVDto>(false, ownerUserId.HasValue ? Resource.Notification.AccessDenied : Resource.Notification.NothingFound, null);

                var ids = pansions.Select(p => p.Id).ToList();
                var rows = PansionRows(_context.PansionReserves.Where(r => ids.Contains(r.PansionId)), dto?.Permitted);
                await AttachPansionTripsAsync(rows);

                return new BaseResultDto<FinancePansionVDto>(true, new FinancePansionVDto
                {
                    Pansions = pansions,
                    ReserveCount = rows.Count,
                    TotalCompanionShare = rows.Sum(x => x.CompanionShare),
                    TotalSiteShare = rows.Sum(x => x.SiteShare),
                    FinanceCompanionReserves = rows
                });
            }
            catch
            {
                return new BaseResultDto<FinancePansionVDto>(false, Resource.Notification.Unsuccess, null);
            }
        }

        // ردیف‌های مالی رزرو پانسیون (پرداخت‌شده و لغونشده)؛ PackageName = نام پانسیون.
        private static List<FinanceCompanionReserveVDto> PansionRows(IQueryable<PansionReserve> source, bool? permitted)
        {
            var query = source.AsNoTracking().Where(r => r.IsReserved && !r.IsCancel);
            if (permitted.HasValue)
                query = query.Where(r => r.Permitted == permitted.Value);

            return query
                .OrderByDescending(r => r.Id)
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
                    PackageName = r.Pansion.Name,
                    Permitted = r.Permitted
                })
                .ToList();
        }

        private async Task AttachPansionTripsAsync(List<FinanceCompanionReserveVDto> rows)
        {
            var trips = await Application.Services.TripSrv.TripSrv.LinkedPetResanTripHelper.ForPansionReservesAsync(
                _context, rows.Select(r => long.Parse(r.ReserveId)));
            foreach (var row in rows)
            {
                if (!trips.TryGetValue(long.Parse(row.ReserveId), out var trip)) continue;
                row.HasPetResan = true;
                row.PetResanPrice = trip.PaymentPrice > 0 ? trip.PaymentPrice : trip.Price;
            }
        }
    }
}
