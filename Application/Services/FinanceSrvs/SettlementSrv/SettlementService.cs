using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Service;
using Application.Services.FinanceSrvs.SettlementSrv.Dto;
using Application.Services.FinanceSrvs.SettlementSrv.Iface;
using AutoMapper;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.SettlementSrv
{
    /// <summary>
    /// تسویه‌ی ارائه‌دهنده‌محور: هر تسویه برای یک ارائه‌دهنده و یک استخر است (نمایندگی‌ها یا فروشگاه‌ها) و همه‌ی ردیف‌های
    /// تسویه‌نشده‌ی آن استخر در تمام نمایندگی‌ها/فروشگاه‌های همان ارائه‌دهنده را یک‌جا می‌پردازد.
    /// درصدها هنوز روی خدمت/نمایندگی/فروشگاه تنظیم می‌شوند؛ اینجا فقط سهم‌های محاسبه‌شده جمع و پرداخت می‌شود.
    /// </summary>
    public class SettlementService : CommonSrv<Settlement, SettlementDto>, ISettlementService
    {
        private const int MarkBatchSize = 500;

        private readonly IDataBaseContext _context;
        private readonly IMapper mapper;

        public SettlementService(IDataBaseContext _context, IMapper mapper) : base(_context, mapper)
        {
            this._context = _context;
            this.mapper = mapper;
        }

        /// <summary>خطای قاعده‌ی کسب‌وکار؛ پیامش مستقیم به ادمین برمی‌گردد و تراکنش برگردانده می‌شود.</summary>
        private sealed class SettlementRuleException : Exception
        {
            public SettlementRuleException(string message) : base(message) { }
        }

        private readonly record struct PoolResult(double PaidPrice, long ItemCount);

        public async Task<BaseResultDto<SettlementVDto>> FindAsyncVDto(long id)
        {
            var item = await WithDetails().FirstOrDefaultAsync(s => s.Id == id);
            return new BaseResultDto<SettlementVDto>(item != null, mapper.Map<SettlementVDto>(item));
        }

        public async Task<BaseResultDto<List<SettlementItemVDto>>> GetItemsAsync(long settlementId)
        {
            var kind = await _context.Settlements.AsNoTracking().Where(s => s.Id == settlementId)
                .Select(s => (SettlementKindEnum?)s.Kind).FirstOrDefaultAsync();
            if (kind == null)
                return new BaseResultDto<List<SettlementItemVDto>>(false, Resource.Notification.NothingFound, null);

            var items = new List<SettlementItemVDto>();
            switch (kind.Value)
            {
                case SettlementKindEnum.Groomer:
                    items.AddRange(await _context.SettlementGroomers.AsNoTracking()
                        .Where(l => l.SettlementId == settlementId)
                        .OrderBy(l => l.Id)
                        .Select(l => new SettlementItemVDto
                        {
                            Type = "Groomer",
                            Code = l.GroomerReserve.ReserveCode,
                            VenueName = l.GroomerReserve.Groomer.Name,
                            BookerFullName = (l.GroomerReserve.Booker.FirstName + " " + l.GroomerReserve.Booker.LastName).Trim(),
                            PaymentPrice = l.GroomerReserve.PaymentPrice,
                            SiteShare = l.GroomerReserve.SiteShare,
                            ProviderShare = l.GroomerReserve.ProviderShare,
                        }).ToListAsync());
                    break;

                case SettlementKindEnum.Clinic:
                    items.AddRange(await _context.SettlementClinics.AsNoTracking()
                        .Where(l => l.SettlementId == settlementId)
                        .OrderBy(l => l.Id)
                        .Select(l => new SettlementItemVDto
                        {
                            Type = "Clinic",
                            Code = l.ClinicReserve.ReserveCode,
                            VenueName = l.ClinicReserve.Clinic.Name,
                            BookerFullName = (l.ClinicReserve.Booker.FirstName + " " + l.ClinicReserve.Booker.LastName).Trim(),
                            PaymentPrice = l.ClinicReserve.PaymentPrice,
                            SiteShare = l.ClinicReserve.SiteShare,
                            ProviderShare = l.ClinicReserve.ProviderShare,
                        }).ToListAsync());
                    break;

                case SettlementKindEnum.Coach:
                    items.AddRange(await _context.SettlementCoaches.AsNoTracking()
                        .Where(l => l.SettlementId == settlementId)
                        .OrderBy(l => l.Id)
                        .Select(l => new SettlementItemVDto
                        {
                            Type = "Coach",
                            Code = l.CoachReserve.ReserveCode,
                            VenueName = l.CoachReserve.Coach.Name,
                            BookerFullName = (l.CoachReserve.Booker.FirstName + " " + l.CoachReserve.Booker.LastName).Trim(),
                            PaymentPrice = l.CoachReserve.PaymentPrice,
                            SiteShare = l.CoachReserve.SiteShare,
                            ProviderShare = l.CoachReserve.ProviderShare,
                        }).ToListAsync());
                    break;

                case SettlementKindEnum.Store:
                    items.AddRange(await _context.SettlementStores.AsNoTracking()
                        .Where(l => l.SettlementId == settlementId)
                        .OrderBy(l => l.Id)
                        .Select(l => new SettlementItemVDto
                        {
                            Type = "Order",
                            Code = l.ProductOrder.OrderCode,
                            VenueName = null,
                            BookerFullName = null,
                            PaymentPrice = l.ProductOrder.PaymentPrice,
                            SiteShare = l.ProductOrder.SiteShare,
                            ProviderShare = l.ProductOrder.StoreShare,
                        }).ToListAsync());
                    break;

                default:
                    var links = _context.SettlementCompanions.AsNoTracking().Where(l => l.SettlementId == settlementId);
                    items.AddRange(await links.Where(l => l.CompanionReserveId != null).Select(l => new SettlementItemVDto
                    {
                        Type = "Reserve",
                        Code = l.CompanionReserve.ReserveCode,
                        VenueName = l.CompanionReserve.CompanionAssistance.Companion.Name,
                        BookerFullName = (l.CompanionReserve.Booker.FirstName + " " + l.CompanionReserve.Booker.LastName).Trim(),
                        PaymentPrice = l.CompanionReserve.PaymentPrice,
                        SiteShare = l.CompanionReserve.SiteShare,
                        ProviderShare = l.CompanionReserve.CompanionShare,
                    }).ToListAsync());
                    items.AddRange(await links.Where(l => l.PansionReserveId != null).Select(l => new SettlementItemVDto
                    {
                        Type = "Pansion",
                        Code = l.PansionReserve.ReserveCode,
                        VenueName = l.PansionReserve.Pansion.Name,
                        BookerFullName = (l.PansionReserve.Booker.FirstName + " " + l.PansionReserve.Booker.LastName).Trim(),
                        PaymentPrice = l.PansionReserve.PaymentPrice,
                        SiteShare = l.PansionReserve.SiteShare,
                        ProviderShare = l.PansionReserve.CompanionShare,
                    }).ToListAsync());
                    items.AddRange(await links.Where(l => l.ConsultationPurchaseId != null).Select(l => new SettlementItemVDto
                    {
                        Type = "Consultation",
                        Code = l.ConsultationPurchase.PurchaseCode,
                        VenueName = null,
                        BookerFullName = null,
                        PaymentPrice = l.ConsultationPurchase.PaymentPrice,
                        SiteShare = l.ConsultationPurchase.SiteShare,
                        ProviderShare = l.ConsultationPurchase.CompanionShare,
                    }).ToListAsync());
                    items.AddRange(await links.Where(l => l.SchoolReserveId != null).Select(l => new SettlementItemVDto
                    {
                        Type = "School",
                        Code = l.SchoolReserve.ReserveCode,
                        VenueName = null,
                        BookerFullName = (l.SchoolReserve.Booker.FirstName + " " + l.SchoolReserve.Booker.LastName).Trim(),
                        PaymentPrice = l.SchoolReserve.PaymentPrice,
                        SiteShare = l.SchoolReserve.SiteShare,
                        ProviderShare = l.SchoolReserve.CompanionShare,
                    }).ToListAsync());
                    break;
            }
            return new BaseResultDto<List<SettlementItemVDto>>(true, items);
        }

        public SettlementSearchDto Search(SettlementInputDto baseSearchDto)
        {
            var model = WithDetails();

            if (baseSearchDto.ProviderId.HasValue)
                model = model.Where(s => s.ProviderId == baseSearchDto.ProviderId.Value);
            if (baseSearchDto.Kind.HasValue)
                model = model.Where(s => s.Kind == baseSearchDto.Kind.Value);
            if (baseSearchDto.StoreId.HasValue)
                model = model.Where(s => s.StoreId == baseSearchDto.StoreId.Value);
            if (baseSearchDto.CompanionId.HasValue)
                model = model.Where(s => s.CompanionId == baseSearchDto.CompanionId.Value);

            model = baseSearchDto.SortBy == Common.Enumerable.SortEnum.Old
                ? model.OrderBy(s => s.Id)
                : model.OrderByDescending(s => s.Id);
            return new SettlementSearchDto(baseSearchDto, model, mapper);
        }

        private IQueryable<Settlement> WithDetails()
        {
            return _context.Settlements
                .Include(s => s.UserBankCard)
                .Include(s => s.Companion)
                .Include(s => s.Store)
                .Include(s => s.Provider).ThenInclude(p => p.User);
        }

        public async Task<SettlementSearchDto> SearchForVenueAsync(SettlementInputDto input, SettlementKindEnum kind, long? venueId)
        {
            // فیلترهایی که کلاینت فرستاده نادیده گرفته می‌شود؛ دامنه فقط از هویت کاربر می‌آید.
            input.StoreId = null;
            input.CompanionId = null;
            input.ProviderId = null;
            input.Kind = kind;

            if (!venueId.HasValue)
            {
                input.ProviderId = -1;
                return Search(input);
            }

            var providerId = await VenueProviderIdAsync(kind, venueId.Value);
            if (providerId.HasValue)
            {
                input.ProviderId = providerId;
                return Search(input);
            }

            // مکانِ بدون ارائه‌دهنده (نباید پیش بیاید): فقط ردیف‌های قدیمیِ خودش.
            if (kind == SettlementKindEnum.Groomer)
            {
                // آرایشگاه ستون قدیمی ندارد؛ بدون ارائه‌دهنده چیزی برای نمایش نیست.
                input.ProviderId = -1;
                return Search(input);
            }

            if (kind == SettlementKindEnum.Clinic)
            {
                // کلینیک ستون قدیمی ندارد؛ بدون ارائه‌دهنده چیزی برای نمایش نیست.
                input.ProviderId = -1;
                return Search(input);
            }
            if (kind == SettlementKindEnum.Coach)
            {
                // مربی ستون قدیمی ندارد؛ بدون ارائه‌دهنده چیزی برای نمایش نیست.
                input.ProviderId = -1;
                return Search(input);
            }
            if (kind == SettlementKindEnum.Companion)
                input.CompanionId = venueId;
            else
                input.StoreId = venueId;
            return Search(input);
        }

        public async Task<SettlementSearchDto> SearchForProviderUserAsync(SettlementInputDto input, SettlementKindEnum kind, long userId)
        {
            input.StoreId = null;
            input.CompanionId = null;
            input.Kind = kind;
            var providerId = await _context.Providers.AsNoTracking()
                .Where(p => p.UserId == userId && !p.Deleted).Select(p => (long?)p.Id).FirstOrDefaultAsync();
            input.ProviderId = providerId ?? -1;
            return Search(input);
        }

        public async Task<bool> BelongsToProviderUserAsync(long settlementId, SettlementKindEnum kind, long userId)
        {
            return await _context.Settlements.AsNoTracking()
                .AnyAsync(s => s.Id == settlementId && s.Kind == kind && s.Provider.UserId == userId && !s.Provider.Deleted);
        }

        public async Task<bool> BelongsToVenueAsync(long settlementId, SettlementKindEnum kind, long? venueId)
        {
            if (!venueId.HasValue)
                return false;

            var settlement = await _context.Settlements.AsNoTracking()
                .Where(x => x.Id == settlementId)
                .Select(x => new { x.Kind, x.ProviderId, x.CompanionId, x.StoreId })
                .FirstOrDefaultAsync();
            if (settlement == null || settlement.Kind != kind)
                return false;

            var legacyVenueId = kind == SettlementKindEnum.Companion ? settlement.CompanionId : kind == SettlementKindEnum.Store ? settlement.StoreId : null;
            if (legacyVenueId.HasValue && legacyVenueId.Value == venueId.Value)
                return true;

            var providerId = await VenueProviderIdAsync(kind, venueId.Value);
            return providerId.HasValue && settlement.ProviderId == providerId;
        }

        private async Task<long?> VenueProviderIdAsync(SettlementKindEnum kind, long venueId)
        {
            return kind switch
            {
                SettlementKindEnum.Companion => await _context.Companions.AsNoTracking().Where(c => c.Id == venueId).Select(c => c.ProviderId).FirstOrDefaultAsync(),
                SettlementKindEnum.Groomer => await _context.Groomers.AsNoTracking().Where(g => g.Id == venueId).Select(g => g.ProviderId).FirstOrDefaultAsync(),
                SettlementKindEnum.Clinic => await _context.Clinics.AsNoTracking().Where(g => g.Id == venueId).Select(g => g.ProviderId).FirstOrDefaultAsync(),
                SettlementKindEnum.Coach => await _context.Coaches.AsNoTracking().Where(g => g.Id == venueId).Select(g => g.ProviderId).FirstOrDefaultAsync(),
                _ => await _context.Stores.AsNoTracking().Where(s => s.Id == venueId).Select(s => s.ProviderId).FirstOrDefaultAsync(),
            };
        }

        public override async Task<BaseResultDto<SettlementDto>> InsertAsyncDto(SettlementDto dto)
        {
            try
            {
                var modelCheker = ModelHelper<SettlementDto>.ModelErrors(dto);
                if (!modelCheker.IsSuccess)
                    return modelCheker;

                if ((dto.StoreId ?? 0) != 0 || (dto.CompanionId ?? 0) != 0 || (dto.ProviderId ?? 0) <= 0)
                    return new BaseResultDto<SettlementDto>(false, Resource.Notification.SettlementRequiresProvider, dto);

                if (!dto.Kind.HasValue || !Enum.IsDefined(dto.Kind.Value))
                    return new BaseResultDto<SettlementDto>(false, Resource.Notification.SettlementKindInvalid, dto);

                var provider = await _context.Providers.AsNoTracking()
                    .Where(x => x.Id == dto.ProviderId.Value && !x.Deleted)
                    .Select(x => new { x.Id, x.UserId })
                    .FirstOrDefaultAsync();
                if (provider == null)
                    return new BaseResultDto<SettlementDto>(false, Resource.Notification.ProviderNotFound, dto);

                // پول فقط به کارت تأییدشده‌ی خودِ ارائه‌دهنده می‌رود؛ کارت کاربر دیگر پذیرفته نمی‌شود.
                var cardIsValid = await _context.UserBankCards.AsNoTracking()
                    .AnyAsync(c => c.Id == dto.UserBankCardId && c.UserId == provider.UserId && c.Approved && !c.Deleted);
                if (!cardIsValid)
                    return new BaseResultDto<SettlementDto>(false, Resource.Notification.SettlementBankCardInvalid, dto);

                // یا همه‌ی ردیف‌ها تسویه می‌شوند یا هیچ‌کدام؛ Serializable تا دو ادمین هم‌زمان یک ردیف را دوبار نپردازند.
                await using var transaction = await _context.BeginTransactionAsync(IsolationLevel.Serializable);

                var item = new Settlement
                {
                    ProviderId = provider.Id,
                    Kind = dto.Kind.Value,
                    UserBankCardId = dto.UserBankCardId,
                    TrackingCode = string.IsNullOrWhiteSpace(dto.TrackingCode) ? null : dto.TrackingCode.Trim(),
                    CreateDate = DateTime.Now,
                    PaidPrice = 0,
                    ItemCount = 0,
                };
                await _context.Settlements.AddAsync(item);
                await _context.SaveChangesAsync();

                var result = dto.Kind.Value switch
                {
                    SettlementKindEnum.Companion => await SettleCompanionPoolAsync(item.Id, provider.Id),
                    SettlementKindEnum.Groomer => await SettleGroomerPoolAsync(item.Id, provider.Id),
                    SettlementKindEnum.Clinic => await SettleClinicPoolAsync(item.Id, provider.Id),
                    SettlementKindEnum.Coach => await SettleCoachPoolAsync(item.Id, provider.Id),
                    _ => await SettleStorePoolAsync(item.Id, provider.Id),
                };

                if (result.ItemCount == 0)
                    throw new SettlementRuleException(Resource.Notification.SettlementNothingToSettle);

                item.PaidPrice = result.PaidPrice;
                item.ItemCount = result.ItemCount;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return new BaseResultDto<SettlementDto>(true, mapper.Map<SettlementDto>(item));
            }
            catch (SettlementRuleException ex)
            {
                return new BaseResultDto<SettlementDto>(false, ex.Message, dto);
            }
            catch (Exception ex)
            {
                return new BaseResultDto<SettlementDto>(false, ExceptionResultHelper.ToClientMessage(ex), dto);
            }
        }

        /// <summary>رزرو خدمت، پانسیون، مشاوره‌ی تکمیل‌شده و دوره‌ی مدرسه‌ی همه‌ی نمایندگی‌های ارائه‌دهنده.</summary>
        private async Task<PoolResult> SettleCompanionPoolAsync(long settlementId, long providerId)
        {
            // نمایندگی حذف‌شده هم شامل است: پولِ ثبت‌شده‌ی آن هنوز بدهی به ارائه‌دهنده است.
            var companionIds = await _context.Companions.AsNoTracking()
                .Where(c => c.ProviderId == providerId)
                .Select(c => c.Id)
                .ToListAsync();
            if (companionIds.Count == 0)
                return new PoolResult(0, 0);

            var reserves = await SettlementSources.CompanionReserves(_context, companionIds)
                .Where(r => !r.Permitted).Select(r => new { r.Id, r.CompanionShare }).ToListAsync();
            var pansions = await SettlementSources.PansionReserves(_context, companionIds)
                .Where(r => !r.Permitted).Select(r => new { r.Id, r.CompanionShare }).ToListAsync();
            var consultations = await SettlementSources.ConsultationPurchases(_context, companionIds)
                .Where(p => !p.Permitted).Select(p => new { p.Id, p.CompanionShare }).ToListAsync();
            var schools = await SettlementSources.SchoolReserves(_context, companionIds)
                .Where(r => !r.Permitted).Select(r => new { r.Id, r.CompanionShare }).ToListAsync();

            var completed = (int)Common.Enumerable.ConsultationPurchaseStatusEnum.Completed;

            // علامت‌گذاری اتمی: اگر ردیفی بین خواندن و نوشتن تغییر کرده باشد (لغو یا تسویه‌ی هم‌زمان) کل تسویه لغو می‌شود.
            await MarkAsync(reserves.Select(x => x.Id), ids => _context.CompanionReserves
                .Where(r => ids.Contains(r.Id) && !r.Permitted && r.IsReserved && !r.IsCancel)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Permitted, true)));
            await MarkAsync(pansions.Select(x => x.Id), ids => _context.PansionReserves
                .Where(r => ids.Contains(r.Id) && !r.Permitted && r.IsReserved && !r.IsCancel)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Permitted, true)));
            await MarkAsync(consultations.Select(x => x.Id), ids => _context.ConsultationPurchases
                .Where(p => ids.Contains(p.Id) && !p.Permitted && p.Status == completed)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Permitted, true)));
            await MarkAsync(schools.Select(x => x.Id), ids => _context.SchoolReserves
                .Where(r => ids.Contains(r.Id) && !r.Permitted && r.IsReserved && !r.IsCancel)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Permitted, true)));

            var links = new List<SettlementCompanion>();
            links.AddRange(reserves.Select(x => new SettlementCompanion { SettlementId = settlementId, CompanionReserveId = x.Id }));
            links.AddRange(pansions.Select(x => new SettlementCompanion { SettlementId = settlementId, PansionReserveId = x.Id }));
            links.AddRange(consultations.Select(x => new SettlementCompanion { SettlementId = settlementId, ConsultationPurchaseId = x.Id }));
            links.AddRange(schools.Select(x => new SettlementCompanion { SettlementId = settlementId, SchoolReserveId = x.Id }));
            await _context.SettlementCompanions.AddRangeAsync(links);
            await _context.SaveChangesAsync();

            var paid = reserves.Sum(x => x.CompanionShare) + pansions.Sum(x => x.CompanionShare)
                + consultations.Sum(x => x.CompanionShare) + schools.Sum(x => x.CompanionShare);
            return new PoolResult(paid, links.Count);
        }

        /// <summary>رزروهای تکمیل‌شده‌ی تسویه‌نشده‌ی همه‌ی آرایشگاه‌های ارائه‌دهنده (حذف‌شده‌ها هم: پولشان هنوز بدهی است).</summary>
        private async Task<PoolResult> SettleGroomerPoolAsync(long settlementId, long providerId)
        {
            var groomerIds = await _context.Groomers.AsNoTracking()
                .Where(g => g.ProviderId == providerId)
                .Select(g => g.Id)
                .ToListAsync();
            if (groomerIds.Count == 0)
                return new PoolResult(0, 0);

            var reserves = await SettlementSources.GroomerReserves(_context, groomerIds)
                .Where(r => !r.Permitted).Select(r => new { r.Id, r.ProviderShare }).ToListAsync();

            var complete = (int)Application.Services.GroomerSrvs.GroomerReserveSrv.GroomerReserveStatusEnum.Complete;
            await MarkAsync(reserves.Select(x => x.Id), ids => _context.GroomerReserves
                .Where(r => ids.Contains(r.Id) && !r.Permitted && r.IsReserved && !r.IsCancel && r.Status == complete)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Permitted, true)));

            var links = reserves.Select(x => new SettlementGroomer { SettlementId = settlementId, GroomerReserveId = x.Id }).ToList();
            await _context.SettlementGroomers.AddRangeAsync(links);
            await _context.SaveChangesAsync();

            return new PoolResult(reserves.Sum(x => x.ProviderShare), links.Count);
        }

        private async Task<PoolResult> SettleClinicPoolAsync(long settlementId, long providerId)
        {
            var clinicIds = await _context.Clinics.AsNoTracking()
                .Where(g => g.ProviderId == providerId)
                .Select(g => g.Id)
                .ToListAsync();
            if (clinicIds.Count == 0)
                return new PoolResult(0, 0);

            var reserves = await SettlementSources.ClinicReserves(_context, clinicIds)
                .Where(r => !r.Permitted).Select(r => new { r.Id, r.ProviderShare }).ToListAsync();

            var complete = (int)Application.Services.ClinicSrvs.ClinicReserveSrv.ClinicReserveStatusEnum.Complete;
            await MarkAsync(reserves.Select(x => x.Id), ids => _context.ClinicReserves
                .Where(r => ids.Contains(r.Id) && !r.Permitted && r.IsReserved && !r.IsCancel && r.Status == complete)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Permitted, true)));

            var links = reserves.Select(x => new SettlementClinic { SettlementId = settlementId, ClinicReserveId = x.Id }).ToList();
            await _context.SettlementClinics.AddRangeAsync(links);
            await _context.SaveChangesAsync();

            return new PoolResult(reserves.Sum(x => x.ProviderShare), links.Count);
        }
        private async Task<PoolResult> SettleCoachPoolAsync(long settlementId, long providerId)
        {
            var coachIds = await _context.Coaches.AsNoTracking()
                .Where(g => g.ProviderId == providerId)
                .Select(g => g.Id)
                .ToListAsync();
            if (coachIds.Count == 0)
                return new PoolResult(0, 0);

            var reserves = await SettlementSources.CoachReserves(_context, coachIds)
                .Where(r => !r.Permitted).Select(r => new { r.Id, r.ProviderShare }).ToListAsync();

            var complete = (int)Application.Services.CoachSrvs.CoachReserveSrv.CoachReserveStatusEnum.Complete;
            await MarkAsync(reserves.Select(x => x.Id), ids => _context.CoachReserves
                .Where(r => ids.Contains(r.Id) && !r.Permitted && r.IsReserved && !r.IsCancel && r.Status == complete)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Permitted, true)));

            var links = reserves.Select(x => new SettlementCoach { SettlementId = settlementId, CoachReserveId = x.Id }).ToList();
            await _context.SettlementCoaches.AddRangeAsync(links);
            await _context.SaveChangesAsync();

            return new PoolResult(reserves.Sum(x => x.ProviderShare), links.Count);
        }

        /// <summary>سفارش‌های تسویه‌نشده‌ی همه‌ی فروشگاه‌های ارائه‌دهنده.</summary>
        private async Task<PoolResult> SettleStorePoolAsync(long settlementId, long providerId)
        {
            var storeIds = await _context.Stores.AsNoTracking()
                .Where(s => s.ProviderId == providerId)
                .Select(s => s.Id)
                .ToListAsync();
            if (storeIds.Count == 0)
                return new PoolResult(0, 0);

            var orders = await SettlementSources.StoreOrders(_context, storeIds)
                .Where(o => !o.Permitted).Select(o => new { o.Id, o.StoreShare }).ToListAsync();

            await MarkAsync(orders.Select(x => x.Id), ids => _context.ProductOrders
                .Where(o => ids.Contains(o.Id) && !o.Permitted && !o.Deleted)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.Permitted, true)));

            var links = orders.Select(x => new SettlementStore { SettlementId = settlementId, ProductOrderId = x.Id }).ToList();
            await _context.SettlementStores.AddRangeAsync(links);
            await _context.SaveChangesAsync();

            return new PoolResult(orders.Sum(x => x.StoreShare), links.Count);
        }

        /// <summary>ردیف‌ها را دسته‌دسته Permitted می‌کند؛ اگر تعداد تغییر‌یافته با تعداد خواسته‌شده نخواند، تسویه لغو می‌شود.</summary>
        private static async Task MarkAsync<TId>(IEnumerable<TId> ids, Func<List<TId>, Task<int>> mark)
        {
            foreach (var batch in ids.Chunk(MarkBatchSize))
            {
                var batchIds = batch.ToList();
                var affected = await mark(batchIds);
                if (affected != batchIds.Count)
                    throw new SettlementRuleException(Resource.Notification.SettlementItemsChanged);
            }
        }
    }
}
