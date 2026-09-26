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
                        CompanionName = s.Companion.Name,
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
                        CompanionName = s.Companion.Name,
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
            if (dto.UserId.HasValue) query = query.Where(s => s.UserId == dto.UserId.Value);
            if (dto.Status.HasValue) query = query.Where(s => s.Status == dto.Status.Value);
            if (dto.ChannelId.HasValue) query = query.Where(s => s.ChannelId == dto.ChannelId.Value);
            if (dto.FromDate.HasValue) query = query.Where(s => s.CreateDate >= dto.FromDate.Value.Date);
            if (dto.ToDate.HasValue) query = query.Where(s => s.CreateDate < dto.ToDate.Value.Date.AddDays(1));
            if (!string.IsNullOrWhiteSpace(dto.Q))
            {
                var q = dto.Q.Trim();
                query = query.Where(s => s.PurchaseCode.Contains(q) || s.PackageName.Contains(q) || s.User.Mobile.Contains(q) ||
                                         s.User.FirstName.Contains(q) || s.User.LastName.Contains(q) || s.Companion.Name.Contains(q));
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
                    .Where(s => ids.Contains(s.CompanionId) && !s.Deleted && s.Active)
                    .Select(s => new { s.CompanionId, s.Price })
                    .ToListAsync();
                var counts = await _context.ConsultationPurchases.AsNoTracking()
                    .Where(s => ids.Contains(s.CompanionId))
                    .GroupBy(s => s.CompanionId)
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

                var list = await _packageService.GetListAsync(companionId);
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
            var saved = await _packageService.CreateAsync(companionId, dto);
            return saved.IsSuccess
                ? new BaseResultDto<ConsultationPackageAdminVDto>(true, ToAdminItem(saved.Data, companionId, name))
                : new BaseResultDto<ConsultationPackageAdminVDto>(false, saved.Messages, null);
        }

        public async Task<BaseResultDto<ConsultationPackageAdminVDto>> UpdateClinicPackageAsync(long companionId, ConsultationPackageItemDto dto)
        {
            var name = await GetClinicNameAsync(companionId);
            if (name == null)
                return new BaseResultDto<ConsultationPackageAdminVDto>(false, Resource.Notification.NothingFound, null);

            var saved = await _packageService.UpdateAsync(companionId, dto);
            return saved.IsSuccess
                ? new BaseResultDto<ConsultationPackageAdminVDto>(true, ToAdminItem(saved.Data, companionId, name))
                : new BaseResultDto<ConsultationPackageAdminVDto>(false, saved.Messages, null);
        }

        public Task<BaseResultDto> DeleteClinicPackageAsync(long companionId, long id) =>
            _packageService.DeleteAsync(companionId, id);

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
