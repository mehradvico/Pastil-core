using Application.Services.ConsultationSrvs.ConsultationOwnerSrv;
using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Services.ConsultationSrvs.ConsultationAdminSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationAdminSrv.Iface;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Iface;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.ConsultationSrvs.ConsultationAdminSrv
{
    // طراحی: backend/Docs/ONLINE_CONSULTATION_PACKAGES_FA.md §۷ (پنل ادمین، فقط خواندن)
    public class ConsultationAdminService : IConsultationAdminService
    {
        private const int MaxPageSize = 100;
        private const int MaxClinics = 200;

        private readonly IDataBaseContext _context;
        private readonly IConsultationPackageService _packageService;

        public ConsultationAdminService(IDataBaseContext context, IConsultationPackageService packageService)
        {
            _context = context;
            _packageService = packageService;
        }

        public async Task<BaseResultDto<ConsultationPurchaseAdminSearchDto>> SearchPurchasesAsync(ConsultationPurchaseAdminInputDto dto)
        {
            try
            {
                dto ??= new ConsultationPurchaseAdminInputDto();
                var query = ApplyFilters(_context.ConsultationPurchases.AsNoTracking(), dto);

                var pageSize = Math.Clamp(dto.PageSize, 1, MaxPageSize);
                var pageIndex = Math.Max(1, dto.PageIndex);
                var total = await query.CountAsync();
                var rows = await query
                    .OrderByDescending(s => s.Id)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new Row
                    {
                        Purchase = s,
                        CompanionName = s.Companion != null ? s.Companion.Name : s.Clinic != null ? s.Clinic.Name : s.Coach.Name,
                        FirstName = s.User.FirstName,
                        LastName = s.User.LastName,
                        Mobile = s.User.Mobile,
                        AgentFirst = s.AgentUser.FirstName,
                        AgentLast = s.AgentUser.LastName,
                        CallSeconds = s.OnlineSession == null ? 0 : s.OnlineSession.CallSeconds,
                        CallStart = s.OnlineSession == null ? null : s.OnlineSession.CallStartDate,
                        CallEnd = s.OnlineSession == null ? null : s.OnlineSession.CallEndDate,
                        MessageCount = s.OnlineSession == null ? 0 : s.OnlineSession.Messages.Count()
                    })
                    .ToListAsync();

                return new BaseResultDto<ConsultationPurchaseAdminSearchDto>(true, new ConsultationPurchaseAdminSearchDto
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalCount = total,
                    List = rows.Select(ToVDto).ToList()
                });
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ConsultationPurchaseAdminSearchDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<ConsultationPurchaseAdminVDto>> GetPurchaseAsync(long id)
        {
            try
            {
                var row = await _context.ConsultationPurchases.AsNoTracking()
                    .Where(s => s.Id == id)
                    .Select(s => new Row
                    {
                        Purchase = s,
                        CompanionName = s.Companion != null ? s.Companion.Name : s.Clinic != null ? s.Clinic.Name : s.Coach.Name,
                        FirstName = s.User.FirstName,
                        LastName = s.User.LastName,
                        Mobile = s.User.Mobile,
                        AgentFirst = s.AgentUser.FirstName,
                        AgentLast = s.AgentUser.LastName,
                        CallSeconds = s.OnlineSession == null ? 0 : s.OnlineSession.CallSeconds,
                        CallStart = s.OnlineSession == null ? null : s.OnlineSession.CallStartDate,
                        CallEnd = s.OnlineSession == null ? null : s.OnlineSession.CallEndDate,
                        MessageCount = s.OnlineSession == null ? 0 : s.OnlineSession.Messages.Count()
                    })
                    .FirstOrDefaultAsync();
                if (row == null)
                    return new BaseResultDto<ConsultationPurchaseAdminVDto>(false, Resource.Notification.NothingFound, null);
                return new BaseResultDto<ConsultationPurchaseAdminVDto>(true, ToVDto(row));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ConsultationPurchaseAdminVDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<ConsultationPurchaseAdminSummaryDto>> GetPurchaseSummaryAsync(ConsultationPurchaseAdminInputDto dto)
        {
            try
            {
                dto ??= new ConsultationPurchaseAdminInputDto();
                var query = ApplyFilters(_context.ConsultationPurchases.AsNoTracking(), dto);

                // یک کوئری گروه‌بندی‌شده روی (وضعیت، تسویه‌شده) و جمع‌بندی در حافظه با قواعد خالص گزارش
                var groups = await query
                    .GroupBy(s => new { s.Status, s.Permitted })
                    .Select(g => new
                    {
                        g.Key.Status,
                        g.Key.Permitted,
                        Count = g.Count(),
                        Price = g.Sum(x => x.Price),
                        Rebate = g.Sum(x => x.RebatePrice),
                        Paid = g.Sum(x => x.PaymentPrice),
                        CompanionShare = g.Sum(x => x.CompanionShare),
                        SiteShare = g.Sum(x => x.SiteShare),
                        Minutes = g.Sum(x => x.DurationMinutes)
                    })
                    .ToListAsync();

                var talkSeconds = await query
                    .Where(s => s.OnlineSessionId != null)
                    .SumAsync(s => (long?)s.OnlineSession.CallSeconds) ?? 0;

                var summary = ConsultationAdminReport.BuildSummary(groups.Select(g => new ConsultationAdminReport.StatusGroup(
                    g.Status, g.Permitted, g.Count, g.Price, g.Rebate, g.Paid, g.CompanionShare, g.SiteShare, g.Minutes)));
                summary.TalkSeconds = talkSeconds;
                return new BaseResultDto<ConsultationPurchaseAdminSummaryDto>(true, summary);
            }
            catch (Exception ex)
            {
                return new BaseResultDto<ConsultationPurchaseAdminSummaryDto>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        private static IQueryable<ConsultationPurchase> ApplyFilters(IQueryable<ConsultationPurchase> query, ConsultationPurchaseAdminInputDto dto)
        {
            if (dto.CompanionId.HasValue) query = query.Where(s => s.CompanionId == dto.CompanionId.Value);
            if (dto.ClinicId.HasValue) query = query.Where(s => s.ClinicId == dto.ClinicId.Value);
            if (dto.CoachId.HasValue) query = query.Where(s => s.CoachId == dto.CoachId.Value);
            if (dto.UserId.HasValue) query = query.Where(s => s.UserId == dto.UserId.Value);
            if (dto.Status.HasValue) query = query.Where(s => s.Status == dto.Status.Value);
            if (dto.ChannelId.HasValue) query = query.Where(s => s.ChannelId == dto.ChannelId.Value);
            if (dto.FromDate.HasValue) query = query.Where(s => s.CreateDate >= dto.FromDate.Value.Date);
            if (dto.ToDate.HasValue) query = query.Where(s => s.CreateDate < dto.ToDate.Value.Date.AddDays(1));
            if (!string.IsNullOrWhiteSpace(dto.Q))
            {
                var q = dto.Q.Trim();
                query = query.Where(s => s.PurchaseCode.Contains(q) || s.PackageName.Contains(q) || s.User.Mobile.Contains(q) ||
                                         s.User.FirstName.Contains(q) || s.User.LastName.Contains(q) || s.Companion.Name.Contains(q) ||
                                         s.Clinic.Name.Contains(q) || s.Coach.Name.Contains(q));
            }
            return query;
        }

        public async Task<BaseResultDto<List<ConsultationClinicAdminVDto>>> GetClinicsAsync(string q)
        {
            try
            {
                var companions = _context.Companions.AsNoTracking().Where(c => !c.Deleted && c.Active && c.Approved);
                if (!string.IsNullOrWhiteSpace(q))
                {
                    var term = q.Trim();
                    companions = companions.Where(c => c.Name.Contains(term));
                }

                var clinics = await companions
                    .OrderBy(c => c.Name)
                    .Take(MaxClinics)
                    .Select(c => new ConsultationClinicAdminVDto { CompanionId = c.Id, CompanionName = c.Name })
                    .ToListAsync();

                var ids = clinics.Select(c => c.CompanionId).ToList();
                var packages = await _context.ConsultationPackages.AsNoTracking()
                    .Where(s => s.CompanionId != null && ids.Contains(s.CompanionId.Value) && !s.Deleted && s.Active)
                    .Select(s => new { CompanionId = s.CompanionId.Value, s.Price })
                    .ToListAsync();
                var counts = await _context.ConsultationPurchases.AsNoTracking()
                    .Where(s => s.CompanionId != null && ids.Contains(s.CompanionId.Value))
                    .GroupBy(s => s.CompanionId.Value)
                    .Select(g => new { CompanionId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.CompanionId, x => x.Count);

                foreach (var c in clinics)
                {
                    var mine = packages.Where(p => p.CompanionId == c.CompanionId).ToList();
                    c.ActivePackages = mine.Count;
                    c.MinPrice = mine.Count > 0 ? mine.Min(p => p.Price) : 0;
                    c.MaxPrice = mine.Count > 0 ? mine.Max(p => p.Price) : 0;
                    c.PurchasesCount = counts.TryGetValue(c.CompanionId, out var n) ? n : 0;
                }

                // کلینیک‌های دارای بسته‌ی فعال اول
                return new BaseResultDto<List<ConsultationClinicAdminVDto>>(true,
                    clinics.OrderByDescending(c => c.ActivePackages).ThenBy(c => c.CompanionName).ToList());
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationClinicAdminVDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<List<ConsultationPackageAdminVDto>>> GetClinicPackagesAsync(long companionId)
        {
            try
            {
                var name = await GetClinicNameAsync(companionId);
                if (name == null)
                    return new BaseResultDto<List<ConsultationPackageAdminVDto>>(false, Resource.Notification.NothingFound, null);

                var list = await _packageService.GetListAsync(ConsultationOwner.Companion(companionId));
                if (!list.IsSuccess)
                    return new BaseResultDto<List<ConsultationPackageAdminVDto>>(false, Resource.Notification.Unsuccess, null);
                return new BaseResultDto<List<ConsultationPackageAdminVDto>>(true, list.Data.Select(i => ToAdminItem(i, companionId, name)).ToList());
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationPackageAdminVDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<ConsultationPackageAdminVDto>> CreateClinicPackageAsync(long companionId, ConsultationPackageItemDto dto)
        {
            var name = await GetClinicNameAsync(companionId);
            if (name == null)
                return new BaseResultDto<ConsultationPackageAdminVDto>(false, Resource.Notification.NothingFound, null);

            // اعتبارسنجی و ذخیره همان سرویس نماینده است
            var saved = await _packageService.CreateAsync(ConsultationOwner.Companion(companionId), dto);
            return saved.IsSuccess
                ? new BaseResultDto<ConsultationPackageAdminVDto>(true, ToAdminItem(saved.Data, companionId, name))
                : new BaseResultDto<ConsultationPackageAdminVDto>(false, saved.Messages, null);
        }

        public async Task<BaseResultDto<ConsultationPackageAdminVDto>> UpdateClinicPackageAsync(long companionId, ConsultationPackageItemDto dto)
        {
            var name = await GetClinicNameAsync(companionId);
            if (name == null)
                return new BaseResultDto<ConsultationPackageAdminVDto>(false, Resource.Notification.NothingFound, null);

            var saved = await _packageService.UpdateAsync(ConsultationOwner.Companion(companionId), dto);
            return saved.IsSuccess
                ? new BaseResultDto<ConsultationPackageAdminVDto>(true, ToAdminItem(saved.Data, companionId, name))
                : new BaseResultDto<ConsultationPackageAdminVDto>(false, saved.Messages, null);
        }

        public Task<BaseResultDto> DeleteClinicPackageAsync(long companionId, long id) =>
            _packageService.DeleteAsync(ConsultationOwner.Companion(companionId), id);

        private static ConsultationOwner? ParseVenue(string kind, long venueId) =>
            (kind ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "clinic" => ConsultationOwner.Clinic(venueId),
                "coach" => ConsultationOwner.Coach(venueId),
                _ => null,
            };

        public async Task<BaseResultDto<List<ConsultationVenueAdminVDto>>> GetVenuesAsync(string kind, string q)
        {
            try
            {
                var probe = ParseVenue(kind, 0);
                if (!probe.HasValue)
                    return new BaseResultDto<List<ConsultationVenueAdminVDto>>(false, Resource.Notification.InvalidData, null);
                var term = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

                List<ConsultationVenueAdminVDto> venues;
                if (probe.Value.Kind == ConsultationOwnerKind.Clinic)
                {
                    var query = _context.Clinics.AsNoTracking().Where(c => !c.Deleted);
                    if (term != null) query = query.Where(c => c.Name.Contains(term));
                    venues = await query.OrderBy(c => c.Name).Take(MaxClinics)
                        .Select(c => new ConsultationVenueAdminVDto { Kind = "clinic", VenueId = c.Id, VenueName = c.Name }).ToListAsync();
                }
                else
                {
                    var query = _context.Coaches.AsNoTracking().Where(c => !c.Deleted);
                    if (term != null) query = query.Where(c => c.Name.Contains(term));
                    venues = await query.OrderBy(c => c.Name).Take(MaxClinics)
                        .Select(c => new ConsultationVenueAdminVDto { Kind = "coach", VenueId = c.Id, VenueName = c.Name }).ToListAsync();
                }

                var ids = venues.Select(v => v.VenueId).ToList();
                var isClinic = probe.Value.Kind == ConsultationOwnerKind.Clinic;
                var packages = await _context.ConsultationPackages.AsNoTracking()
                    .Where(s => !s.Deleted && s.Active && (isClinic
                        ? s.ClinicId != null && ids.Contains(s.ClinicId.Value)
                        : s.CoachId != null && ids.Contains(s.CoachId.Value)))
                    .Select(s => new { VenueId = isClinic ? s.ClinicId.Value : s.CoachId.Value, s.Price, s.Approved })
                    .ToListAsync();
                var counts = await _context.ConsultationPurchases.AsNoTracking()
                    .Where(s => isClinic
                        ? s.ClinicId != null && ids.Contains(s.ClinicId.Value)
                        : s.CoachId != null && ids.Contains(s.CoachId.Value))
                    .GroupBy(s => isClinic ? s.ClinicId.Value : s.CoachId.Value)
                    .Select(g => new { VenueId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.VenueId, x => x.Count);

                foreach (var v in venues)
                {
                    var mine = packages.Where(p => p.VenueId == v.VenueId).ToList();
                    v.ActivePackages = mine.Count;
                    v.PendingPackages = mine.Count(p => !p.Approved);
                    v.MinPrice = mine.Count > 0 ? mine.Min(p => p.Price) : 0;
                    v.MaxPrice = mine.Count > 0 ? mine.Max(p => p.Price) : 0;
                    v.PurchasesCount = counts.TryGetValue(v.VenueId, out var n) ? n : 0;
                }
                return new BaseResultDto<List<ConsultationVenueAdminVDto>>(true,
                    venues.OrderByDescending(v => v.ActivePackages).ThenBy(v => v.VenueName).ToList());
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationVenueAdminVDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<List<ConsultationVenuePackageAdminVDto>>> GetVenuePackagesAsync(string kind, long venueId)
        {
            try
            {
                var owner = ParseVenue(kind, venueId);
                if (!owner.HasValue || !await ConsultationOwners.ExistsAsync(_context, owner.Value))
                    return new BaseResultDto<List<ConsultationVenuePackageAdminVDto>>(false, Resource.Notification.NothingFound, null);

                var name = await ConsultationOwners.NameAsync(_context, owner.Value);
                var rows = await _context.ConsultationPackages.AsNoTracking()
                    .ForOwner(owner.Value).Where(s => !s.Deleted)
                    .OrderBy(s => s.ChannelId).ThenBy(s => s.SortOrder).ThenBy(s => s.DurationMinutes).ThenBy(s => s.Id)
                    .ToListAsync();
                var kindName = owner.Value.Kind == ConsultationOwnerKind.Clinic ? "clinic" : "coach";
                return new BaseResultDto<List<ConsultationVenuePackageAdminVDto>>(true, rows.Select(s => new ConsultationVenuePackageAdminVDto
                {
                    Id = s.Id, Kind = kindName, VenueId = venueId, VenueName = name, ChannelId = s.ChannelId,
                    DurationMinutes = s.DurationMinutes, Price = s.Price, Active = s.Active, Approved = s.Approved, Bookable = s.Bookable,
                    Name = s.Name, Description = s.Description, CommissionPercent = s.CommissionPercent, SortOrder = s.SortOrder,
                }).ToList());
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationVenuePackageAdminVDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto> DeleteVenuePackageAsync(string kind, long venueId, long id)
        {
            var owner = ParseVenue(kind, venueId);
            if (!owner.HasValue || !await ConsultationOwners.ExistsAsync(_context, owner.Value))
                return new BaseResultDto(false, Resource.Notification.NothingFound);
            return await _packageService.DeleteAsync(owner.Value, id);
        }

        public async Task<BaseResultDto> ApproveVenuePackageAsync(string kind, long venueId, long id, bool approved)
        {
            try
            {
                var owner = ParseVenue(kind, venueId);
                if (!owner.HasValue)
                    return new BaseResultDto(false, Resource.Notification.NothingFound);
                var affected = await _context.ConsultationPackages
                    .ForOwner(owner.Value).Where(s => s.Id == id && !s.Deleted)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.Approved, approved)
                        .SetProperty(s => s.UpdateDate, (DateTime?)DateTime.Now));
                return affected == 0
                    ? new BaseResultDto(false, Resource.Notification.NothingFound)
                    : new BaseResultDto(true, Resource.Notification.Success);
            }
            catch (Exception ex)
            {
                return new BaseResultDto(false, ExceptionResultHelper.ToClientMessage(ex));
            }
        }

        private Task<string> GetClinicNameAsync(long companionId) =>
            _context.Companions.AsNoTracking()
                .Where(c => c.Id == companionId && !c.Deleted)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();

        private static ConsultationPackageAdminVDto ToAdminItem(ConsultationPackageItemDto i, long companionId, string name) => new()
        {
            Id = i.Id,
            CompanionId = companionId,
            CompanionName = name,
            ChannelId = i.ChannelId,
            DurationMinutes = i.DurationMinutes,
            Price = i.Price,
            Active = i.Active,
            Name = i.Name,
            Description = i.Description,
            PictureId = i.PictureId,
            Picture = i.Picture,
            SortOrder = i.SortOrder
        };

        private sealed class Row
        {
            public ConsultationPurchase Purchase { get; set; }
            public string CompanionName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Mobile { get; set; }
            public string AgentFirst { get; set; }
            public string AgentLast { get; set; }
            public int CallSeconds { get; set; }
            public DateTime? CallStart { get; set; }
            public DateTime? CallEnd { get; set; }
            public int MessageCount { get; set; }
        }

        private static ConsultationPurchaseAdminVDto ToVDto(Row r)
        {
            var s = r.Purchase;
            return new ConsultationPurchaseAdminVDto
            {
                Id = s.Id,
                PurchaseCode = s.PurchaseCode,
                CompanionId = s.CompanionId,
                ClinicId = s.ClinicId,
                CoachId = s.CoachId,
                CompanionName = r.CompanionName,
                UserId = s.UserId,
                UserFullName = $"{r.FirstName} {r.LastName}".Trim(),
                UserMobile = r.Mobile,
                PackageId = s.ConsultationPackageId,
                PackageName = s.PackageName,
                ChannelId = s.ChannelId,
                DurationMinutes = s.DurationMinutes,
                Price = s.Price,
                RebatePrice = s.RebatePrice,
                WalletPrice = s.WalletPrice,
                PaymentPrice = s.PaymentPrice,
                CommissionPercent = s.CommissionPercent,
                CompanionShare = s.CompanionShare,
                SiteShare = s.SiteShare,
                NetPaid = ConsultationAdminReport.NetPaid(s.Status, s.PaymentPrice),
                RefundedAmount = ConsultationAdminReport.RefundedAmount(s.Status, s.PaymentPrice),
                Permitted = s.Permitted,
                Status = s.Status,
                CreateDate = s.CreateDate,
                PaidDate = s.PaidDate,
                StartDeadline = s.StartDeadline,
                StartDate = s.StartDate,
                ExpireDate = s.ExpireDate,
                CallSeconds = r.CallSeconds,
                CallStartDate = r.CallStart,
                CallEndDate = r.CallEnd,
                MessageCount = r.MessageCount,
                CancelDate = s.CancelDate,
                CancelReason = s.CancelReason,
                RefundDate = s.RefundDate,
                AgentUserId = s.AgentUserId,
                AgentFullName = $"{r.AgentFirst} {r.AgentLast}".Trim(),
                OnlineSessionId = s.OnlineSessionId,
                PaymentId = s.PaymentId
            };
        }
    }
}
