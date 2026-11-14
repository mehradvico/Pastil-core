using AngleSharp.Dom;
using Application.Common.Dto.Result;
using Application.Common.Enumerable.Code;
using Application.Common.Helpers;
using Application.Services.CompanionSrvs.CompanionAssistanceSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.FinanceSrvs.FinanceSrv.Dto;
using Application.Services.FinanceSrvs.FinanceSrv.Iface;
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

namespace Application.Services.FinanceSrvs.FinanceSrv
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
            var storesQ = _context.Stores.Where(s => !s.Deleted).AsQueryable();

            var companionsQ = _context.Companions.Include(c => c.Owner).Where(c => !c.Deleted).AsQueryable();

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

            var storeIds = storesQ.Select(s => s.Id).ToList();

            var orderStats = _context.ProductOrderStores.Where(pos =>storeIds.Contains(pos.StoreId) && !pos.Deleted && !pos.ProductOrder.Deleted && pos.ProductOrder.IsPaid)
                .GroupBy(pos => pos.StoreId).Select(g => new
                {
                    StoreId = g.Key,
                    OrderCount = g.Select(x => x.ProductOrderId).Distinct().Count()
                }).ToList().ToDictionary(x => x.StoreId, x => x.OrderCount);

            var stores = storesQ.Select(s => mapper.Map<StoreFinanceVDto>(s)).ToList();

            foreach (var s in stores)
            {
                s.OrderCount = orderStats.TryGetValue(s.Id, out var cnt) ? cnt : 0;
            }

            if (dto.HasCommission.HasValue)
            {
                stores = dto.HasCommission.Value? stores.Where(s => s.CommissionPercent != 0).ToList() : stores.Where(s => s.CommissionPercent == 0).ToList();
            }

            var companionIds = companionsQ.Select(c => c.Id).ToList();

            var assistanceStats = _context.CompanionAssistances
                .Where(a => companionIds.Contains(a.CompanionId) && !a.Deleted).GroupBy(a => a.CompanionId).Select(g => new
                {
                    CompanionId = g.Key,
                    Total = g.Count(),
                    With = g.Count(x => x.CommissionPercent > 0)
                }).ToList().ToDictionary(x => x.CompanionId);

            var pansionStats = _context.Pansions.Where(p => !p.Deleted && p.CompanionId != null && companionIds.Contains(p.CompanionId.Value)).GroupBy(p => p.CompanionId.Value).Select(g => new
                {
                    CompanionId = g.Key,
                    Has = 1,
                    With = g.Any(x => x.CommissionPercent > 0 || (x.DailyCommissionPercent > 0 && x.HourlyCommissionPercent > 0)) ? 1 : 0
                }).ToList().ToDictionary(x => x.CompanionId);

            var companionReserveStats =
                (from r in _context.CompanionReserves
                 join a in _context.CompanionAssistances on r.CompanionAssistanceId equals a.Id
                 where companionIds.Contains(a.CompanionId) && r.IsReserved && !r.IsCancel
                 group r by a.CompanionId into g
                 select new
                 {
                     CompanionId = g.Key,
                     Count = g.Count()

                 }).ToList().ToDictionary(x => x.CompanionId, x => x.Count);

            var pansionReserveStats =
                (from r in _context.PansionReserves
                 join p in _context.Pansions on r.PansionId equals p.Id
                 where p.CompanionId != null && companionIds.Contains(p.CompanionId.Value)
                       && r.IsReserved
                       && !r.IsCancel
                 group r by p.CompanionId.Value into g
                 select new
                 {
                     CompanionId = g.Key,
                     Count = g.Count()
                 })
                .ToList()
                .ToDictionary(x => x.CompanionId, x => x.Count);

            // مشاوره‌های آنلاین پرداخت‌شده (Paid / Active / Completed): Refunded، Cancelled و Expired جزو رزرو معتبر نیستند
            var consultationPaidStatuses = new[]
            {
                (int)Common.Enumerable.ConsultationPurchaseStatusEnum.Paid,
                (int)Common.Enumerable.ConsultationPurchaseStatusEnum.Active,
                (int)Common.Enumerable.ConsultationPurchaseStatusEnum.Completed
            };
            var consultationStats = _context.ConsultationPurchases
                .Where(p => p.CompanionId != null && companionIds.Contains(p.CompanionId.Value) && consultationPaidStatuses.Contains(p.Status))
                .GroupBy(p => p.CompanionId.Value)
                .Select(g => new { CompanionId = g.Key, Count = g.Count() })
                .ToList()
                .ToDictionary(x => x.CompanionId, x => x.Count);

            var companions = companionsQ
                .AsEnumerable()
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

                    dtoC.CompanionReserveCount = companionReserveStats.TryGetValue(c.Id, out var cr) ? cr : 0;
                    dtoC.PansionReserveCount = pansionReserveStats.TryGetValue(c.Id, out var pr) ? pr : 0;
                    dtoC.ConsultationPurchaseCount = consultationStats.TryGetValue(c.Id, out var cn) ? cn : 0;
                    dtoC.TotalReserveCount = dtoC.CompanionReserveCount + dtoC.PansionReserveCount + dtoC.ConsultationPurchaseCount;

                    return dtoC;
                })
                .ToList();

            if (dto.HasCommission.HasValue)
            {
                companions = companions.Where(c => c.HasCommission == dto.HasCommission.Value).ToList();
            }

            var venues = dto.IsCompanion == false ? new System.Collections.Generic.List<FinanceVenueVDto>() : BuildVenues(dto);

            return new FinanceSearchDto { Stores = stores, Companions = companions, Venues = venues };
        }

        // تب «مراکز و متخصصین»: هر نوع مرکز از جدول خودش با کمیسیون و تعداد رزرو معتبر (پرداخت‌شده و لغونشده)
        private System.Collections.Generic.List<FinanceVenueVDto> BuildVenues(FinanceInputDto dto)
        {
            var kind = (dto.VenueKind ?? string.Empty).Trim().ToLowerInvariant();
            bool Wants(string k) => kind.Length == 0 || kind == k;
            var q = string.IsNullOrWhiteSpace(dto.Q) ? null : dto.Q.Trim();
            var venues = new System.Collections.Generic.List<FinanceVenueVDto>();

            if (Wants("clinic"))
            {
                var counts = _context.ClinicReserves.Where(r => r.IsReserved && !r.IsCancel)
                    .GroupBy(r => r.ClinicId).Select(g => new { g.Key, Count = g.Count() }).ToDictionary(x => x.Key, x => x.Count);
                venues.AddRange(_context.Clinics.Where(c => !c.Deleted && (q == null || c.Name.Contains(q)))
                    .Select(c => new FinanceVenueVDto { Kind = "clinic", Id = c.Id, Name = c.Name, PictureUrl = c.Picture != null ? c.Picture.Url : null, Active = c.Active, CommissionPercent = (decimal)c.CommissionPercent })
                    .ToList()
                    .Select(v => { v.ReserveCount = counts.TryGetValue(v.Id, out var n) ? n : 0; v.HasCommission = v.CommissionPercent > 0; return v; }));
            }

            if (Wants("coach"))
            {
                var counts = _context.CoachReserves.Where(r => r.IsReserved && !r.IsCancel)
                    .GroupBy(r => r.CoachId).Select(g => new { g.Key, Count = g.Count() }).ToDictionary(x => x.Key, x => x.Count);
                venues.AddRange(_context.Coaches.Where(c => !c.Deleted && (q == null || c.Name.Contains(q)))
                    .Select(c => new FinanceVenueVDto { Kind = "coach", Id = c.Id, Name = c.Name, PictureUrl = c.Picture != null ? c.Picture.Url : null, Active = c.Active, CommissionPercent = (decimal)c.CommissionPercent })
                    .ToList()
                    .Select(v => { v.ReserveCount = counts.TryGetValue(v.Id, out var n) ? n : 0; v.HasCommission = v.CommissionPercent > 0; return v; }));
            }

            if (Wants("groomer"))
            {
                var counts = _context.GroomerReserves.Where(r => r.IsReserved && !r.IsCancel)
                    .GroupBy(r => r.GroomerId).Select(g => new { g.Key, Count = g.Count() }).ToDictionary(x => x.Key, x => x.Count);
                venues.AddRange(_context.Groomers.Where(c => !c.Deleted && (q == null || c.Name.Contains(q)))
                    .Select(c => new FinanceVenueVDto { Kind = "groomer", Id = c.Id, Name = c.Name, PictureUrl = c.Picture != null ? c.Picture.Url : null, Active = c.Active, CommissionPercent = (decimal)c.CommissionPercent })
                    .ToList()
                    .Select(v => { v.ReserveCount = counts.TryGetValue(v.Id, out var n) ? n : 0; v.HasCommission = v.CommissionPercent > 0; return v; }));
            }

            if (Wants("pansion"))
            {
                var counts = _context.PansionReserves.Where(r => r.IsReserved && !r.IsCancel)
                    .GroupBy(r => r.PansionId).Select(g => new { g.Key, Count = g.Count() }).ToDictionary(x => x.Key, x => x.Count);
                venues.AddRange(_context.Pansions.Where(p => !p.Deleted && (q == null || p.Name.Contains(q)))
                    .Select(p => new FinanceVenueVDto { Kind = "pansion", Id = p.Id, Name = p.Name, PictureUrl = p.Picture != null ? p.Picture.Url : null, Active = p.Active, CommissionPercent = p.CommissionPercent, DailyCommissionPercent = p.DailyCommissionPercent, HourlyCommissionPercent = p.HourlyCommissionPercent, CompanionId = p.CompanionId })
                    .ToList()
                    .Select(v => { v.ReserveCount = counts.TryGetValue(v.Id, out var n) ? n : 0; v.HasCommission = v.CommissionPercent > 0 || (v.DailyCommissionPercent > 0 && v.HourlyCommissionPercent > 0); return v; }));
            }

            if (Wants("school"))
            {
                var courses = _context.SchoolCourses.Where(c => !c.Deleted)
                    .GroupBy(c => c.SchoolId).Select(g => new { g.Key, Total = g.Count(), With = g.Count(c => c.CommissionPercent > 0) }).ToDictionary(x => x.Key);
                var counts = (from r in _context.SchoolReserves
                              join c in _context.SchoolCourses on r.SchoolCourseId equals c.Id
                              where r.IsReserved && !r.IsCancel
                              group r by c.SchoolId into g
                              select new { g.Key, Count = g.Count() }).ToDictionary(x => x.Key, x => x.Count);
                venues.AddRange(_context.Schools.Where(s => !s.Deleted && (q == null || s.Name.Contains(q)))
                    .Select(s => new FinanceVenueVDto { Kind = "school", Id = s.Id, Name = s.Name, PictureUrl = s.Picture != null ? s.Picture.Url : null, Active = s.Active, CompanionId = s.CompanionId, CommissionPercent = s.CommissionPercent })
                    .ToList()
                    .Select(v =>
                    {
                        if (courses.TryGetValue(v.Id, out var c)) { v.CourseCount = c.Total; v.CoursesWithCommissionCount = c.With; }
                        v.HasCommission = v.CommissionPercent > 0 || (v.CourseCount > 0 && v.CourseCount == v.CoursesWithCommissionCount);
                        v.ReserveCount = counts.TryGetValue(v.Id, out var n) ? n : 0;
                        return v;
                    }));
            }

            System.Collections.Generic.IEnumerable<FinanceVenueVDto> result = venues;
            if (dto.Available.HasValue) result = result.Where(v => v.Active == dto.Available.Value);
            if (dto.HasCommission.HasValue) result = result.Where(v => v.HasCommission == dto.HasCommission.Value);
            result = dto.SortBy == Common.Enumerable.SortEnum.Old
                ? result.OrderBy(v => v.Kind).ThenBy(v => v.Id)
                : result.OrderByDescending(v => v.ReserveCount).ThenBy(v => v.Name);
            return result.ToList();
        }


        public CompanionFinanceDetailVDto SearchCompanionDetail(long companionId)
        {
            var companion = _context.Companions.Include(c => c.Picture).Include(c => c.Owner).FirstOrDefault(c => c.Id == companionId && !c.Deleted);
            if (companion == null) 
                return null;

            var pansions = _context.Pansions.Include(p => p.Picture).Where(p => p.CompanionId == companionId && !p.Deleted).ToList();

            var assistances = _context.CompanionAssistances.Include(a => a.Assistance).Where(a => a.CompanionId == companionId && !a.Deleted).ToList();

            var result = mapper.Map<CompanionFinanceDetailVDto>(companion);
            result.Pansions = pansions.Select(p => mapper.Map<PansionFinanceVDto>(p)).ToList();
            result.CompanionAssistances = assistances.Select(a => mapper.Map<CompanionAssistanceFinanceVDto>(a)).ToList();
            result.ConsultationPackages = _context.ConsultationPackages
                .Where(p => p.CompanionId == companionId && !p.Deleted)
                .OrderBy(p => p.ChannelId).ThenBy(p => p.SortOrder).ThenBy(p => p.DurationMinutes).ThenBy(p => p.Id)
                .Select(p => new ConsultationPackageFinanceVDto
                {
                    Id = p.Id,
                    CompanionId = p.CompanionId.Value,
                    Name = p.Name,
                    ChannelId = p.ChannelId,
                    DurationMinutes = p.DurationMinutes,
                    Price = p.Price,
                    Active = p.Active,
                    Bookable = p.Bookable,
                    CommissionPercent = p.CommissionPercent
                })
                .ToList();

            var pansionHas = result.Pansions != null && result.Pansions.Count > 0 ? 1 : 0;
            var pansionWith = (pansionHas == 1 && result.Pansions.Any(p => p.HasCommission)) ? 1 : 0;

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

                // سهم پاستیل نمی‌تواند از تخفیفِ فعالِ «سهم پاستیل» همین فروشگاه کمتر شود (وگرنه پاستیل ناخواسته ضرر می‌کند)
                var maxPastilDiscount = await _context.Discounts.AsNoTracking()
                    .Where(d => d.StoreId == item.Id && d.FundedByPastil && d.Active && !d.Deleted && (!d.EndDate.HasValue || d.EndDate.Value >= DateTime.Now.Date))
                    .Select(d => (int?)d.Percent).MaxAsync() ?? 0;
                if (maxPastilDiscount > dto.CommissionPercent)
                    return new BaseResultDto(false, string.Format(System.Globalization.CultureInfo.InvariantCulture, Resource.Notification.StoreCommissionBelowPastilDiscount, maxPastilDiscount));

                item.CommissionPercent = dto.CommissionPercent;

                _context.Stores.Update(item);
                await _context.SaveChangesAsync();
                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
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
                return new BaseResultDto(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }


        public async Task<BaseResultDto> UpdateConsultationPackageCommissionAsyncDto(FinanceConsultationPackageDto dto)
        {
            try
            {
                if (dto == null || dto.ConsultationPackageId <= 0 || dto.CommissionPercent < 0 || dto.CommissionPercent > 100)
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.InvalidData);

                // فقط خریدهای بعدی اثر می‌گیرند؛ خریدهای انجام‌شده درصد لحظه‌ی خریدشان را نگه می‌دارند
                var affected = await _context.ConsultationPackages
                    .Where(x => x.Id == dto.ConsultationPackageId && !x.Deleted)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.CommissionPercent, dto.CommissionPercent)
                        .SetProperty(x => x.UpdateDate, (DateTime?)DateTime.Now));

                return affected == 0
                    ? new BaseResultDto(isSuccess: false, val: Resource.Notification.NothingFound)
                    : new BaseResultDto(isSuccess: true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(isSuccess: false, val: Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
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

                if (dto.CommissionPercent is < 0 or > 100 || dto.DailyCommissionPercent is < 0 or > 100 || dto.HourlyCommissionPercent is < 0 or > 100)
                    return new BaseResultDto(isSuccess: false, val: Resource.Notification.InvalidData);

                if (dto.CommissionPercent.HasValue)
                    item.CommissionPercent = dto.CommissionPercent.Value;
                item.DailyCommissionPercent = dto.DailyCommissionPercent;
                item.HourlyCommissionPercent = dto.HourlyCommissionPercent;

                _context.Pansions.Update(item);
                await _context.SaveChangesAsync();
                return new BaseResultDto(true);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, Application.Common.Helpers.ExceptionResultHelper.ToClientMessage(ex));
            }
        }

    }
}
