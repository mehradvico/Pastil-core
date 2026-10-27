using Application.Common.Dto.Result;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Dto;
using Application.Services.FinanceSrvs.FinanceCompanionSrv.Iface;
using Application.Services.FinanceSrvs.FinanceStoreSrv.Dto;
using Application.Services.FinanceSrvs.SettlementSrv;
using Application.Services.FinanceSrvs.UserBankCardSrv.Dto;
using Application.Services.FinanceSrvs.UserBankCardSrv.Iface;
using Application.Services.ProviderSrvs.ProviderFinanceSrv.Dto;
using Application.Services.ProviderSrvs.ProviderFinanceSrv.Iface;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IFinanceStoreService = Application.Services.FinanceSrvs.FinanceStoreSrv.Iface.IFinanceStoreService;

namespace Application.Services.ProviderSrvs.ProviderFinanceSrv
{
    /// <summary>
    /// حسابداری ارائه‌دهنده‌محور. مانده‌ها با همان تعریفِ ردیف‌های قابل تسویه (SettlementSources) از دیتابیس تجمیع می‌شوند و
    /// ریز ردیف‌ها از سرویس‌های مالی موجود (FinanceCompanion/FinanceStore) می‌آید تا یک عدد در دو جا دو جور نشان داده نشود.
    /// درصدها این‌جا محاسبه نمی‌شوند؛ از سهم‌های ثبت‌شده‌ی هر ردیف می‌خوانیم.
    /// </summary>
    public class ProviderFinanceService : IProviderFinanceService
    {
        private const int UnassignedVenueLimit = 50;

        private readonly IDataBaseContext _context;
        private readonly IFinanceCompanionService _financeCompanion;
        private readonly IFinanceStoreService _financeStore;
        private readonly IUserBankCardService _bankCards;

        public ProviderFinanceService(
            IDataBaseContext context,
            IFinanceCompanionService financeCompanion,
            IFinanceStoreService financeStore,
            IUserBankCardService bankCards)
        {
            _context = context;
            _financeCompanion = financeCompanion;
            _financeStore = financeStore;
            _bankCards = bankCards;
        }

        // ------------------------------------------------------------------ مدل‌های داخلی

        private sealed class Totals
        {
            public int Count { get; set; }
            public int UnsettledCount { get; set; }
            public double ProviderShare { get; set; }
            public double SiteShare { get; set; }
            public double SettledShare { get; set; }
            public double UnsettledShare { get; set; }

            public void Add(double providerShare, double siteShare, bool permitted)
            {
                Count++;
                ProviderShare += providerShare;
                SiteShare += siteShare;
                if (permitted)
                    SettledShare += providerShare;
                else
                {
                    UnsettledShare += providerShare;
                    UnsettledCount++;
                }
            }

            public void Add(Totals other)
            {
                Count += other.Count;
                UnsettledCount += other.UnsettledCount;
                ProviderShare += other.ProviderShare;
                SiteShare += other.SiteShare;
                SettledShare += other.SettledShare;
                UnsettledShare += other.UnsettledShare;
            }
        }

        private sealed class StoreData
        {
            public List<StoreOrderRow> Rows { get; set; } = new List<StoreOrderRow>();
            public Dictionary<string, OrderSpan> Spans { get; set; } = new Dictionary<string, OrderSpan>();
        }

        private sealed class SettlementStat
        {
            public double Paid { get; set; }
            public int Count { get; set; }
            public DateTime? Last { get; set; }
        }

        // ------------------------------------------------------------------ لیست

        public async Task<ProviderFinanceSearchVDto> SearchAsync(ProviderFinanceInputDto input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Clamp(input.PageSize, 1, 200);

            var providersQuery = _context.Providers.AsNoTracking().Where(p => !p.Deleted);
            if (!string.IsNullOrWhiteSpace(input.Q))
            {
                var q = input.Q.Trim();
                providersQuery = providersQuery.Where(p => p.User.Mobile.Contains(q) || p.User.FirstName.Contains(q) || p.User.LastName.Contains(q));
            }

            var providers = await providersQuery
                .Select(p => new { p.Id, p.UserId, p.Active, p.User.FirstName, p.User.LastName, p.User.Mobile })
                .ToListAsync();
            var providerIds = providers.Select(p => p.Id).ToList();

            var companionMap = await _context.Companions.AsNoTracking()
                .Where(c => c.ProviderId != null && providerIds.Contains(c.ProviderId.Value))
                .Select(c => new { c.Id, ProviderId = c.ProviderId.Value, c.Deleted })
                .ToListAsync();
            var storeMap = await _context.Stores.AsNoTracking()
                .Where(s => s.ProviderId != null && providerIds.Contains(s.ProviderId.Value))
                .Select(s => new { s.Id, ProviderId = s.ProviderId.Value, s.Deleted })
                .ToListAsync();
            var groomerMap = await _context.Groomers.AsNoTracking()
                .Where(g => g.ProviderId != null && providerIds.Contains(g.ProviderId.Value))
                .Select(g => new { g.Id, ProviderId = g.ProviderId.Value, g.Deleted })
                .ToListAsync();

            var clinicMap = await _context.Clinics.AsNoTracking()
                .Where(g => g.ProviderId != null && providerIds.Contains(g.ProviderId.Value))
                .Select(g => new { g.Id, ProviderId = g.ProviderId.Value, g.Deleted })
                .ToListAsync();
            var coachMap = await _context.Coaches.AsNoTracking()
                .Where(g => g.ProviderId != null && providerIds.Contains(g.ProviderId.Value))
                .Select(g => new { g.Id, ProviderId = g.ProviderId.Value, g.Deleted })
                .ToListAsync();

            var groomerTotals = await GroomerTotalsAsync(groomerMap.Select(g => g.Id).ToList());
            var clinicTotals = await ClinicTotalsAsync(clinicMap.Select(g => g.Id).ToList());
            var coachTotals = await CoachTotalsAsync(coachMap.Select(g => g.Id).ToList());
            var companionTotals = await CompanionTotalsAsync(companionMap.Select(c => c.Id).ToList());
            var providerSchoolTotals = await ProviderSchoolTotalsAsync(providerIds);
            var storeData = await LoadStoreDataAsync(storeMap.Select(s => s.Id).ToList());
            var stats = await SettlementStatsAsync(providerIds);

            var userIds = providers.Select(p => p.UserId).ToList();
            var usersWithCard = (await _context.UserBankCards.AsNoTracking()
                    .Where(c => userIds.Contains(c.UserId) && c.Approved && !c.Deleted)
                    .Select(c => c.UserId).Distinct().ToListAsync())
                .ToHashSet();

            var storeToProvider = storeMap.ToDictionary(s => s.Id, s => s.ProviderId);
            var rows = new List<ProviderFinanceRowVDto>();
            foreach (var p in providers)
            {
                var companionPool = new Totals();
                foreach (var c in companionMap.Where(c => c.ProviderId == p.Id))
                    if (companionTotals.TryGetValue(c.Id, out var t))
                        companionPool.Add(t);
                // مدرسه‌های فقط-ارائه‌دهنده در همین استخر تسویه می‌شوند
                if (providerSchoolTotals.TryGetValue(p.Id, out var schoolPool))
                    companionPool.Add(schoolPool);

                var storePool = StorePoolTotals(storeData, storeToProvider, p.Id, out _, out _);

                stats.TryGetValue((p.Id, SettlementKindEnum.Companion), out var cs);
                stats.TryGetValue((p.Id, SettlementKindEnum.Store), out var ss);
                stats.TryGetValue((p.Id, SettlementKindEnum.Groomer), out var gs);
                stats.TryGetValue((p.Id, SettlementKindEnum.Clinic), out var cls);
                stats.TryGetValue((p.Id, SettlementKindEnum.Coach), out var cos);

                var groomerPool = new Totals();
                foreach (var g in groomerMap.Where(g => g.ProviderId == p.Id))
                    if (groomerTotals.TryGetValue(g.Id, out var gt))
                        groomerPool.Add(gt);

                var clinicPool = new Totals();
                foreach (var g in clinicMap.Where(g => g.ProviderId == p.Id))
                    if (clinicTotals.TryGetValue(g.Id, out var gt))
                        clinicPool.Add(gt);
                var coachPool = new Totals();
                foreach (var g in coachMap.Where(g => g.ProviderId == p.Id))
                    if (coachTotals.TryGetValue(g.Id, out var gt))
                        coachPool.Add(gt);

                rows.Add(new ProviderFinanceRowVDto
                {
                    ProviderId = p.Id,
                    UserId = p.UserId,
                    Name = $"{p.FirstName} {p.LastName}".Trim(),
                    Mobile = p.Mobile,
                    Active = p.Active,
                    CompanionCount = companionMap.Count(c => c.ProviderId == p.Id && !c.Deleted),
                    StoreCount = storeMap.Count(s => s.ProviderId == p.Id && !s.Deleted),
                    GroomerCount = groomerMap.Count(g => g.ProviderId == p.Id && !g.Deleted),
                    ClinicCount = clinicMap.Count(g => g.ProviderId == p.Id && !g.Deleted),
                    CoachCount = coachMap.Count(g => g.ProviderId == p.Id && !g.Deleted),
                    CompanionUnsettled = companionPool.UnsettledShare,
                    StoreUnsettled = storePool.UnsettledShare,
                    GroomerUnsettled = groomerPool.UnsettledShare,
                    ClinicUnsettled = clinicPool.UnsettledShare,
                    CoachUnsettled = coachPool.UnsettledShare,
                    TotalUnsettled = companionPool.UnsettledShare + storePool.UnsettledShare + groomerPool.UnsettledShare + clinicPool.UnsettledShare + coachPool.UnsettledShare,
                    TotalSettled = companionPool.SettledShare + storePool.SettledShare + groomerPool.SettledShare + clinicPool.SettledShare + coachPool.SettledShare,
                    PaidOut = (cs?.Paid ?? 0) + (ss?.Paid ?? 0) + (gs?.Paid ?? 0) + (cls?.Paid ?? 0) + (cos?.Paid ?? 0),
                    LastSettlementDate = new[] { cs?.Last, ss?.Last, gs?.Last, cls?.Last, cos?.Last }.Max(),
                    HasApprovedCard = usersWithCard.Contains(p.UserId),
                });
            }

            if (input.HasUnsettled == true)
                rows = rows.Where(r => r.TotalUnsettled > 0).ToList();
            else if (input.HasUnsettled == false)
                rows = rows.Where(r => r.TotalUnsettled <= 0).ToList();

            rows = input.SortBy == Common.Enumerable.SortEnum.Old
                ? rows.OrderBy(r => r.ProviderId).ToList()
                : rows.OrderByDescending(r => r.TotalUnsettled).ThenByDescending(r => r.ProviderId).ToList();

            var result = new ProviderFinanceSearchVDto
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = rows.Count,
                List = rows.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList(),
                UnassignedVenues = await UnassignedVenuesAsync(),
            };
            return result;
        }

        private async Task<List<ProviderUnassignedVenueVDto>> UnassignedVenuesAsync()
        {
            var companions = await _context.Companions.AsNoTracking()
                .Where(c => c.ProviderId == null && !c.Deleted)
                .OrderBy(c => c.Id).Take(UnassignedVenueLimit)
                .Select(c => new ProviderUnassignedVenueVDto { Kind = SettlementKindEnum.Companion, Id = c.Id, Name = c.Name })
                .ToListAsync();
            var stores = await _context.Stores.AsNoTracking()
                .Where(s => s.ProviderId == null && !s.Deleted)
                .OrderBy(s => s.Id).Take(UnassignedVenueLimit)
                .Select(s => new ProviderUnassignedVenueVDto { Kind = SettlementKindEnum.Store, Id = s.Id, Name = s.Name })
                .ToListAsync();
            var groomers = await _context.Groomers.AsNoTracking()
                .Where(g => g.ProviderId == null && !g.Deleted)
                .OrderBy(g => g.Id).Take(UnassignedVenueLimit)
                .Select(g => new ProviderUnassignedVenueVDto { Kind = SettlementKindEnum.Groomer, Id = g.Id, Name = g.Name })
                .ToListAsync();

            var clinics = await _context.Clinics.AsNoTracking()
                .Where(g => g.ProviderId == null && !g.Deleted)
                .OrderBy(g => g.Id).Take(UnassignedVenueLimit)
                .Select(g => new ProviderUnassignedVenueVDto { Kind = SettlementKindEnum.Clinic, Id = g.Id, Name = g.Name })
                .ToListAsync();
            var coaches = await _context.Coaches.AsNoTracking()
                .Where(g => g.ProviderId == null && !g.Deleted)
                .OrderBy(g => g.Id).Take(UnassignedVenueLimit)
                .Select(g => new ProviderUnassignedVenueVDto { Kind = SettlementKindEnum.Coach, Id = g.Id, Name = g.Name })
                .ToListAsync();
            return companions.Concat(stores).Concat(groomers).Concat(clinics).Concat(coaches).ToList();
        }

        // ------------------------------------------------------------------ جزئیات

        public async Task<BaseResultDto<ProviderFinanceDetailVDto>> GetDetailAsync(long providerId)
        {
            var provider = await _context.Providers.AsNoTracking()
                .Where(p => p.Id == providerId && !p.Deleted)
                .Select(p => new { p.Id, p.UserId, p.Active, p.User.FirstName, p.User.LastName, p.User.Mobile })
                .FirstOrDefaultAsync();
            if (provider == null)
                return new BaseResultDto<ProviderFinanceDetailVDto>(false, Resource.Notification.ProviderNotFound, null);

            var companions = await _context.Companions.AsNoTracking()
                .Where(c => c.ProviderId == providerId)
                .OrderBy(c => c.Id)
                .Select(c => new { c.Id, c.Name, c.Active, c.Deleted })
                .ToListAsync();
            var stores = await _context.Stores.AsNoTracking()
                .Where(s => s.ProviderId == providerId)
                .OrderBy(s => s.Id)
                .Select(s => new { s.Id, s.Name, s.Active, s.Deleted })
                .ToListAsync();

            var groomers = await _context.Groomers.AsNoTracking()
                .Where(g => g.ProviderId == providerId)
                .OrderBy(g => g.Id)
                .Select(g => new { g.Id, g.Name, g.Active, g.Deleted })
                .ToListAsync();

            var clinics = await _context.Clinics.AsNoTracking()
                .Where(g => g.ProviderId == providerId)
                .OrderBy(g => g.Id)
                .Select(g => new { g.Id, g.Name, g.Active, g.Deleted })
                .ToListAsync();
            var coaches = await _context.Coaches.AsNoTracking()
                .Where(g => g.ProviderId == providerId)
                .OrderBy(g => g.Id)
                .Select(g => new { g.Id, g.Name, g.Active, g.Deleted })
                .ToListAsync();

            var companionTotals = await CompanionTotalsAsync(companions.Select(c => c.Id).ToList());
            var groomerTotals = await GroomerTotalsAsync(groomers.Select(g => g.Id).ToList());
            var clinicTotals = await ClinicTotalsAsync(clinics.Select(g => g.Id).ToList());
            var coachTotals = await CoachTotalsAsync(coaches.Select(g => g.Id).ToList());
            var storeData = await LoadStoreDataAsync(stores.Select(s => s.Id).ToList());
            var stats = await SettlementStatsAsync(new[] { providerId });

            var storeToProvider = stores.ToDictionary(s => s.Id, _ => providerId);
            var companionPool = new Totals();
            var venues = new List<ProviderFinanceVenueVDto>();

            foreach (var c in companions)
            {
                companionTotals.TryGetValue(c.Id, out var t);
                t ??= new Totals();
                companionPool.Add(t);
                venues.Add(ToVenue(SettlementKindEnum.Companion, c.Id, c.Name, c.Active, c.Deleted, t));
            }
            if ((await ProviderSchoolTotalsAsync(new[] { providerId })).TryGetValue(providerId, out var providerSchools))
                companionPool.Add(providerSchools);

            var groomerPool = new Totals();
            foreach (var g in groomers)
            {
                groomerTotals.TryGetValue(g.Id, out var t);
                t ??= new Totals();
                groomerPool.Add(t);
                venues.Add(ToVenue(SettlementKindEnum.Groomer, g.Id, g.Name, g.Active, g.Deleted, t));
            }

            var clinicPool = new Totals();
            foreach (var g in clinics)
            {
                clinicTotals.TryGetValue(g.Id, out var t);
                t ??= new Totals();
                clinicPool.Add(t);
                venues.Add(ToVenue(SettlementKindEnum.Clinic, g.Id, g.Name, g.Active, g.Deleted, t));
            }
            var coachPool = new Totals();
            foreach (var g in coaches)
            {
                coachTotals.TryGetValue(g.Id, out var t);
                t ??= new Totals();
                coachPool.Add(t);
                venues.Add(ToVenue(SettlementKindEnum.Coach, g.Id, g.Name, g.Active, g.Deleted, t));
            }

            var storePool = StorePoolTotals(storeData, storeToProvider, providerId, out var notReceived, out var shared);
            foreach (var s in stores)
            {
                var t = new Totals();
                foreach (var row in storeData.Rows.Where(r => r.StoreId == s.Id && storeData.Spans.TryGetValue(r.OrderId, out var span) && span.StoreCount == 1))
                    t.Add(row.StoreShare, row.SiteShare, row.Permitted);
                venues.Add(ToVenue(SettlementKindEnum.Store, s.Id, s.Name, s.Active, s.Deleted, t));
            }

            stats.TryGetValue((providerId, SettlementKindEnum.Companion), out var cs);
            stats.TryGetValue((providerId, SettlementKindEnum.Store), out var ss);
            stats.TryGetValue((providerId, SettlementKindEnum.Groomer), out var gs);
            stats.TryGetValue((providerId, SettlementKindEnum.Clinic), out var cls);
            stats.TryGetValue((providerId, SettlementKindEnum.Coach), out var cos);

            var detail = new ProviderFinanceDetailVDto
            {
                ProviderId = provider.Id,
                UserId = provider.UserId,
                Name = $"{provider.FirstName} {provider.LastName}".Trim(),
                Mobile = provider.Mobile,
                Active = provider.Active,
                CompanionPool = ToSummary(SettlementKindEnum.Companion, companionPool, cs, 0, 0),
                StorePool = ToSummary(SettlementKindEnum.Store, storePool, ss, notReceived, shared),
                GroomerPool = ToSummary(SettlementKindEnum.Groomer, groomerPool, gs, 0, 0),
                ClinicPool = ToSummary(SettlementKindEnum.Clinic, clinicPool, cls, 0, 0),
                CoachPool = ToSummary(SettlementKindEnum.Coach, coachPool, cos, 0, 0),
                Venues = venues,
                Cards = GetCards(provider.UserId),
            };
            return new BaseResultDto<ProviderFinanceDetailVDto>(true, detail);
        }

        private static ProviderFinanceVenueVDto ToVenue(SettlementKindEnum kind, long id, string name, bool active, bool deleted, Totals t)
            => new ProviderFinanceVenueVDto
            {
                Kind = kind,
                Id = id,
                Name = name,
                Active = active,
                Deleted = deleted,
                ItemCount = t.Count,
                UnsettledCount = t.UnsettledCount,
                ProviderShare = t.ProviderShare,
                SiteShare = t.SiteShare,
                SettledShare = t.SettledShare,
                UnsettledShare = t.UnsettledShare,
            };

        private static ProviderPoolSummaryVDto ToSummary(SettlementKindEnum kind, Totals t, SettlementStat stat, int notReceived, int shared)
            => new ProviderPoolSummaryVDto
            {
                Kind = kind,
                ItemCount = t.Count,
                UnsettledCount = t.UnsettledCount,
                ProviderShare = t.ProviderShare,
                SiteShare = t.SiteShare,
                SettledShare = t.SettledShare,
                UnsettledShare = t.UnsettledShare,
                PaidOut = stat?.Paid ?? 0,
                SettlementCount = stat?.Count ?? 0,
                LastSettlementDate = stat?.Last,
                NotReceivedCount = notReceived,
                SharedOrderCount = shared,
            };

        private List<ProviderFinanceCardVDto> GetCards(long userId)
        {
            var search = _bankCards.Search(new UserBankCardInputDto { UserId = userId, PageIndex = 1, PageSize = 50 });
            return (search.List ?? new List<UserBankCardVDto>())
                .Select(x => new ProviderFinanceCardVDto
                {
                    Id = x.Id,
                    BankName = x.BankCard?.BankName,
                    CardNumber = x.CardNumber,
                    ShebaNumber = x.ShebaNumber,
                    CardHolderName = x.CardHolderName,
                    Approved = x.Approved,
                })
                .ToList();
        }

        // ------------------------------------------------------------------ پیش‌نمایش تسویه

        public async Task<BaseResultDto<ProviderPendingVDto>> GetPendingAsync(long providerId, SettlementKindEnum kind)
        {
            var provider = await _context.Providers.AsNoTracking()
                .Where(p => p.Id == providerId && !p.Deleted)
                .Select(p => new { p.Id, p.UserId, p.User.FirstName, p.User.LastName })
                .FirstOrDefaultAsync();
            if (provider == null)
                return new BaseResultDto<ProviderPendingVDto>(false, Resource.Notification.ProviderNotFound, null);

            var items = kind switch
            {
                SettlementKindEnum.Companion => await PendingCompanionItemsAsync(providerId),
                SettlementKindEnum.Groomer => await PendingGroomerItemsAsync(providerId),
                SettlementKindEnum.Clinic => await PendingClinicItemsAsync(providerId),
                SettlementKindEnum.Coach => await PendingCoachItemsAsync(providerId),
                _ => await PendingStoreItemsAsync(providerId),
            };

            var result = new ProviderPendingVDto
            {
                Kind = kind,
                ProviderId = provider.Id,
                ProviderName = $"{provider.FirstName} {provider.LastName}".Trim(),
                ItemCount = items.Count,
                PaymentPrice = items.Sum(i => i.PaymentPrice),
                SiteShare = items.Sum(i => i.SiteShare),
                ProviderShare = items.Sum(i => i.ProviderShare),
                NotReceivedCount = items.Count(i => i.NotReceived),
                Items = items,
                Cards = GetCards(provider.UserId),
                Breakdown = items.GroupBy(i => i.Type).Select(g => new ProviderPendingBreakdownVDto
                {
                    Type = g.Key,
                    Count = g.Count(),
                    PaymentPrice = g.Sum(i => i.PaymentPrice),
                    SiteShare = g.Sum(i => i.SiteShare),
                    ProviderShare = g.Sum(i => i.ProviderShare),
                }).ToList(),
            };
            return new BaseResultDto<ProviderPendingVDto>(true, result);
        }

        private async Task<List<ProviderPendingItemVDto>> PendingCompanionItemsAsync(long providerId)
        {
            var companions = await _context.Companions.AsNoTracking()
                .Where(c => c.ProviderId == providerId)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            var items = new List<ProviderPendingItemVDto>();
            foreach (var c in companions)
            {
                var finance = await _financeCompanion.SearchAsync(new FinanceCompanionInputDto { CompanionId = c.Id, Permitted = false });
                if (!finance.IsSuccess || finance.Data == null)
                    continue;
                items.AddRange(finance.Data.FinanceCompanionReserves.Select(r => new ProviderPendingItemVDto
                {
                    VenueId = c.Id,
                    VenueName = c.Name,
                    Type = r.IsConsultation ? "Consultation" : r.IsSchool ? "School" : r.IsPansion ? "Pansion" : "Reserve",
                    Code = r.ReserveCode,
                    BookerFullName = r.BookerFullName,
                    PaymentPrice = r.PaymentPrice,
                    CommissionPercent = r.CommissionPercent,
                    SiteShare = r.SiteShare,
                    ProviderShare = r.CompanionShare,
                    StatusLabel = r.StatusLabel,
                }));
            }

            // مدرسه‌های فقط-ارائه‌دهنده (بدون نمایندگی) — همان تعریف SettlementSources.ProviderSchoolReserves که تسویه می‌پردازد
            items.AddRange(await SettlementSources.ProviderSchoolReserves(_context, providerId)
                .Where(r => !r.Permitted)
                .OrderBy(r => r.Id)
                .Select(r => new ProviderPendingItemVDto
                {
                    VenueId = r.SchoolCourse.SchoolId,
                    VenueName = r.SchoolCourse.School.Name,
                    Type = "School",
                    Code = r.ReserveCode,
                    BookerFullName = ((r.Booker.FirstName ?? "") + " " + (r.Booker.LastName ?? "")).Trim(),
                    PaymentPrice = r.PaymentPrice,
                    CommissionPercent = r.SchoolCourse.CommissionPercent,
                    SiteShare = r.SiteShare,
                    ProviderShare = r.CompanionShare,
                    StatusLabel = null,
                }).ToListAsync());
            return items;
        }

        /// <summary>رزروهای تکمیل‌شده‌ی تسویه‌نشده‌ی آرایشگاه‌ها؛ همان تعریف SettlementSources که تسویه می‌پردازد.</summary>
        private async Task<List<ProviderPendingItemVDto>> PendingGroomerItemsAsync(long providerId)
        {
            var groomerIds = await _context.Groomers.AsNoTracking()
                .Where(g => g.ProviderId == providerId)
                .Select(g => g.Id)
                .ToListAsync();
            if (groomerIds.Count == 0)
                return new List<ProviderPendingItemVDto>();

            return await SettlementSources.GroomerReserves(_context, groomerIds)
                .Where(r => !r.Permitted)
                .OrderBy(r => r.Id)
                .Select(r => new ProviderPendingItemVDto
                {
                    VenueId = r.GroomerId,
                    VenueName = r.Groomer.Name,
                    Type = "Groomer",
                    Code = r.ReserveCode,
                    BookerFullName = (r.Booker.FirstName + " " + r.Booker.LastName).Trim(),
                    PaymentPrice = r.PaymentPrice,
                    CommissionPercent = r.CommissionPercent,
                    SiteShare = r.SiteShare,
                    ProviderShare = r.ProviderShare,
                    StatusLabel = "Complete",
                })
                .ToListAsync();
        }

        private async Task<List<ProviderPendingItemVDto>> PendingClinicItemsAsync(long providerId)
        {
            var clinicIds = await _context.Clinics.AsNoTracking()
                .Where(g => g.ProviderId == providerId)
                .Select(g => g.Id)
                .ToListAsync();
            if (clinicIds.Count == 0)
                return new List<ProviderPendingItemVDto>();

            var items = await SettlementSources.ClinicReserves(_context, clinicIds)
                .Where(r => !r.Permitted)
                .OrderBy(r => r.Id)
                .Select(r => new ProviderPendingItemVDto
                {
                    VenueId = r.ClinicId,
                    VenueName = r.Clinic.Name,
                    Type = "Clinic",
                    Code = r.ReserveCode,
                    BookerFullName = (r.Booker.FirstName + " " + r.Booker.LastName).Trim(),
                    PaymentPrice = r.PaymentPrice,
                    CommissionPercent = r.CommissionPercent,
                    SiteShare = r.SiteShare,
                    ProviderShare = r.ProviderShare,
                    StatusLabel = "Complete",
                })
                .ToListAsync();
            items.AddRange(await SettlementSources.ClinicConsultationPurchases(_context, clinicIds)
                .Where(p => !p.Permitted)
                .OrderBy(p => p.Id)
                .Select(p => new ProviderPendingItemVDto
                {
                    VenueId = p.ClinicId.Value,
                    VenueName = p.Clinic.Name,
                    Type = "Consultation",
                    Code = p.PurchaseCode,
                    BookerFullName = (p.User.FirstName + " " + p.User.LastName).Trim(),
                    PaymentPrice = p.PaymentPrice,
                    CommissionPercent = p.CommissionPercent,
                    SiteShare = p.SiteShare,
                    ProviderShare = p.CompanionShare,
                    StatusLabel = "Complete",
                })
                .ToListAsync());
            return items;
        }
        private async Task<List<ProviderPendingItemVDto>> PendingCoachItemsAsync(long providerId)
        {
            var coachIds = await _context.Coaches.AsNoTracking()
                .Where(g => g.ProviderId == providerId)
                .Select(g => g.Id)
                .ToListAsync();
            if (coachIds.Count == 0)
                return new List<ProviderPendingItemVDto>();

            var items = await SettlementSources.CoachReserves(_context, coachIds)
                .Where(r => !r.Permitted)
                .OrderBy(r => r.Id)
                .Select(r => new ProviderPendingItemVDto
                {
                    VenueId = r.CoachId,
                    VenueName = r.Coach.Name,
                    Type = "Coach",
                    Code = r.ReserveCode,
                    BookerFullName = (r.Booker.FirstName + " " + r.Booker.LastName).Trim(),
                    PaymentPrice = r.PaymentPrice,
                    CommissionPercent = r.CommissionPercent,
                    SiteShare = r.SiteShare,
                    ProviderShare = r.ProviderShare,
                    StatusLabel = "Complete",
                })
                .ToListAsync();
            items.AddRange(await SettlementSources.CoachConsultationPurchases(_context, coachIds)
                .Where(p => !p.Permitted)
                .OrderBy(p => p.Id)
                .Select(p => new ProviderPendingItemVDto
                {
                    VenueId = p.CoachId.Value,
                    VenueName = p.Coach.Name,
                    Type = "Consultation",
                    Code = p.PurchaseCode,
                    BookerFullName = (p.User.FirstName + " " + p.User.LastName).Trim(),
                    PaymentPrice = p.PaymentPrice,
                    CommissionPercent = p.CommissionPercent,
                    SiteShare = p.SiteShare,
                    ProviderShare = p.CompanionShare,
                    StatusLabel = "Complete",
                })
                .ToListAsync());
            return items;
        }

        private async Task<List<ProviderPendingItemVDto>> PendingStoreItemsAsync(long providerId)
        {
            var stores = await _context.Stores.AsNoTracking()
                .Where(s => s.ProviderId == providerId)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            // سفارشی که چند فروشگاهِ همین ارائه‌دهنده دارد فقط یک‌بار شمرده می‌شود (همان‌طور که ثبت تسویه می‌پردازد).
            var seen = new HashSet<string>();
            var items = new List<ProviderPendingItemVDto>();
            foreach (var s in stores)
            {
                var finance = _financeStore.Search(new FinanceStoreInputDto { StoreId = s.Id, Permitted = false });
                if (!finance.IsSuccess || finance.Data == null)
                    continue;
                foreach (var o in finance.Data.FinanceProductOrders)
                {
                    if (!seen.Add(o.ProductOrderId))
                        continue;
                    items.Add(new ProviderPendingItemVDto
                    {
                        VenueId = s.Id,
                        VenueName = s.Name,
                        Type = "Order",
                        Code = o.OrderCode,
                        BookerFullName = o.BuyerFullName,
                        PaymentPrice = o.PaymentPrice,
                        CommissionPercent = o.CommissionPercent,
                        SiteShare = o.SiteShare,
                        ProviderShare = o.StoreShare,
                        StatusLabel = o.StatusLabel,
                        NotReceived = o.UserReceived == false,
                    });
                }
            }
            return items;
        }

        // ------------------------------------------------------------------ تجمیع از دیتابیس

        /// <summary>مجموع هر نمایندگی از چهار منبع تسویه (رزرو خدمت، پانسیون، مشاوره، مدرسه).</summary>
        private async Task<Dictionary<long, Totals>> CompanionTotalsAsync(IReadOnlyCollection<long> companionIds)
        {
            var result = new Dictionary<long, Totals>();
            if (companionIds.Count == 0)
                return result;

            void Merge(IEnumerable<GroupRow> rows)
            {
                foreach (var row in rows)
                {
                    if (!result.TryGetValue(row.Key, out var t))
                        result[row.Key] = t = new Totals();
                    t.Count += row.Count;
                    t.UnsettledCount += row.UnsettledCount;
                    t.ProviderShare += row.ProviderShare;
                    t.SiteShare += row.SiteShare;
                    t.SettledShare += row.SettledShare;
                    t.UnsettledShare += row.UnsettledShare;
                }
            }

            Merge(await ProviderFinanceQueries.CompanionReserveGroups(_context, companionIds).ToListAsync());
            Merge(await ProviderFinanceQueries.PansionReserveGroups(_context, companionIds).ToListAsync());
            Merge(await ProviderFinanceQueries.ConsultationGroups(_context, companionIds).ToListAsync());
            Merge(await ProviderFinanceQueries.SchoolReserveGroups(_context, companionIds).ToListAsync());

            return result;
        }

        /// <summary>مجموع ثبت‌نام مدرسه‌های فقط-ارائه‌دهنده (بدون نمایندگی) به‌ازای هر ارائه‌دهنده؛ جزو استخر «نمایندگی» است.</summary>
        private async Task<Dictionary<long, Totals>> ProviderSchoolTotalsAsync(IReadOnlyCollection<long> providerIds)
        {
            var result = new Dictionary<long, Totals>();
            if (providerIds.Count == 0)
                return result;
            foreach (var row in await ProviderFinanceQueries.ProviderSchoolReserveGroups(_context, providerIds).ToListAsync())
            {
                result[row.Key] = new Totals
                {
                    Count = row.Count,
                    UnsettledCount = row.UnsettledCount,
                    ProviderShare = row.ProviderShare,
                    SiteShare = row.SiteShare,
                    SettledShare = row.SettledShare,
                    UnsettledShare = row.UnsettledShare,
                };
            }
            return result;
        }

        /// <summary>مجموع هر آرایشگاه از رزروهای تکمیل‌شده.</summary>
        private async Task<Dictionary<long, Totals>> GroomerTotalsAsync(IReadOnlyCollection<long> groomerIds)
        {
            var result = new Dictionary<long, Totals>();
            if (groomerIds.Count == 0)
                return result;
            foreach (var row in await ProviderFinanceQueries.GroomerReserveGroups(_context, groomerIds).ToListAsync())
            {
                result[row.Key] = new Totals
                {
                    Count = row.Count,
                    UnsettledCount = row.UnsettledCount,
                    ProviderShare = row.ProviderShare,
                    SiteShare = row.SiteShare,
                    SettledShare = row.SettledShare,
                    UnsettledShare = row.UnsettledShare,
                };
            }
            return result;
        }

        private async Task<Dictionary<long, Totals>> ClinicTotalsAsync(IReadOnlyCollection<long> clinicIds)
        {
            var result = new Dictionary<long, Totals>();
            if (clinicIds.Count == 0)
                return result;
            var rows = (await ProviderFinanceQueries.ClinicReserveGroups(_context, clinicIds).ToListAsync())
                .Concat(await ProviderFinanceQueries.ClinicConsultationGroups(_context, clinicIds).ToListAsync());
            foreach (var row in rows)
            {
                if (!result.TryGetValue(row.Key, out var t))
                    result[row.Key] = t = new Totals();
                t.Count += row.Count;
                t.UnsettledCount += row.UnsettledCount;
                t.ProviderShare += row.ProviderShare;
                t.SiteShare += row.SiteShare;
                t.SettledShare += row.SettledShare;
                t.UnsettledShare += row.UnsettledShare;
            }
            return result;
        }
        private async Task<Dictionary<long, Totals>> CoachTotalsAsync(IReadOnlyCollection<long> coachIds)
        {
            var result = new Dictionary<long, Totals>();
            if (coachIds.Count == 0)
                return result;
            var rows = (await ProviderFinanceQueries.CoachReserveGroups(_context, coachIds).ToListAsync())
                .Concat(await ProviderFinanceQueries.CoachConsultationGroups(_context, coachIds).ToListAsync());
            foreach (var row in rows)
            {
                if (!result.TryGetValue(row.Key, out var t))
                    result[row.Key] = t = new Totals();
                t.Count += row.Count;
                t.UnsettledCount += row.UnsettledCount;
                t.ProviderShare += row.ProviderShare;
                t.SiteShare += row.SiteShare;
                t.SettledShare += row.SettledShare;
                t.UnsettledShare += row.UnsettledShare;
            }
            return result;
        }

        private async Task<StoreData> LoadStoreDataAsync(IReadOnlyCollection<long> storeIds)
        {
            var data = new StoreData();
            if (storeIds.Count == 0)
                return data;

            data.Rows = await ProviderFinanceQueries.StoreOrderRows(_context, storeIds).ToListAsync();

            var orderIds = data.Rows.Select(r => r.OrderId).Distinct().ToList();
            if (orderIds.Count == 0)
                return data;

            data.Spans = (await ProviderFinanceQueries.OrderSpans(_context, orderIds).ToListAsync()).ToDictionary(x => x.OrderId);
            return data;
        }

        /// <summary>جمع استخر فروشگاه‌های یک ارائه‌دهنده؛ سفارش چندفروشگاهیِ همان ارائه‌دهنده فقط یک‌بار شمرده می‌شود.</summary>
        private static Totals StorePoolTotals(StoreData data, IReadOnlyDictionary<long, long> storeToProvider, long providerId, out int notReceived, out int shared)
        {
            var totals = new Totals();
            notReceived = 0;
            shared = 0;
            var seen = new HashSet<string>();
            foreach (var row in data.Rows)
            {
                if (!storeToProvider.TryGetValue(row.StoreId, out var owner) || owner != providerId || !seen.Add(row.OrderId))
                    continue;
                totals.Add(row.StoreShare, row.SiteShare, row.Permitted);
                if (row.Permitted)
                    continue;
                if (row.UserReceived == false)
                    notReceived++;
                if (data.Spans.TryGetValue(row.OrderId, out var span) && span.ProviderCount > 1)
                    shared++;
            }
            return totals;
        }

        private async Task<Dictionary<(long ProviderId, SettlementKindEnum Kind), SettlementStat>> SettlementStatsAsync(IReadOnlyCollection<long> providerIds)
        {
            var rows = await ProviderFinanceQueries.SettlementStats(_context, providerIds).ToListAsync();
            return rows.ToDictionary(r => (r.ProviderId, r.Kind), r => new SettlementStat { Paid = r.Paid, Count = r.Count, Last = r.Last });
        }
    }
}
