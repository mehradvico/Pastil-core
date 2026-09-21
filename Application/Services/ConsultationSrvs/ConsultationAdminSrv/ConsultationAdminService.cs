using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Services.ConsultationSrvs.ConsultationAdminSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationAdminSrv.Iface;
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

        private readonly IDataBaseContext _context;

        public ConsultationAdminService(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<BaseResultDto<ConsultationPurchaseAdminSearchDto>> SearchPurchasesAsync(ConsultationPurchaseAdminInputDto dto)
        {
            try
            {
                dto ??= new ConsultationPurchaseAdminInputDto();
                var query = _context.ConsultationPurchases.AsNoTracking().AsQueryable();
                if (dto.CompanionId.HasValue) query = query.Where(s => s.CompanionId == dto.CompanionId.Value);
                if (dto.UserId.HasValue) query = query.Where(s => s.UserId == dto.UserId.Value);
                if (dto.Status.HasValue) query = query.Where(s => s.Status == dto.Status.Value);
                if (dto.ChannelId.HasValue) query = query.Where(s => s.ChannelId == dto.ChannelId.Value);
                if (dto.FromDate.HasValue) query = query.Where(s => s.CreateDate >= dto.FromDate.Value.Date);
                if (dto.ToDate.HasValue) query = query.Where(s => s.CreateDate < dto.ToDate.Value.Date.AddDays(1));
                if (!string.IsNullOrWhiteSpace(dto.Q))
                {
                    var q = dto.Q.Trim();
                    query = query.Where(s => s.PurchaseCode.Contains(q) || s.User.Mobile.Contains(q) ||
                                             s.User.FirstName.Contains(q) || s.User.LastName.Contains(q) || s.Companion.Name.Contains(q));
                }

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
                        AgentLast = s.AgentUser.LastName
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
                        AgentLast = s.AgentUser.LastName
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

        public async Task<BaseResultDto<List<ConsultationClinicAdminVDto>>> GetClinicsAsync()
        {
            try
            {
                var clinics = await _context.ConsultationPackages.AsNoTracking()
                    .Where(s => !s.Deleted && s.Active && !s.Companion.Deleted)
                    .GroupBy(s => new { s.CompanionId, s.Companion.Name })
                    .Select(g => new ConsultationClinicAdminVDto
                    {
                        CompanionId = g.Key.CompanionId,
                        CompanionName = g.Key.Name,
                        ActivePackages = g.Count(),
                        MinPrice = g.Min(x => x.Price),
                        MaxPrice = g.Max(x => x.Price)
                    })
                    .OrderBy(s => s.CompanionName)
                    .ToListAsync();

                var counts = await _context.ConsultationPurchases.AsNoTracking()
                    .GroupBy(s => s.CompanionId)
                    .Select(g => new { CompanionId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.CompanionId, x => x.Count);
                foreach (var c in clinics)
                    c.PurchasesCount = counts.TryGetValue(c.CompanionId, out var n) ? n : 0;

                return new BaseResultDto<List<ConsultationClinicAdminVDto>>(true, clinics);
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
                var rows = await _context.ConsultationPackages.AsNoTracking()
                    .Where(s => s.CompanionId == companionId && !s.Deleted)
                    .OrderBy(s => s.ChannelId).ThenBy(s => s.DurationMinutes)
                    .Select(s => new ConsultationPackageAdminVDto
                    {
                        Id = s.Id,
                        CompanionId = s.CompanionId,
                        CompanionName = s.Companion.Name,
                        ChannelId = s.ChannelId,
                        DurationMinutes = s.DurationMinutes,
                        Price = s.Price,
                        Active = s.Active
                    })
                    .ToListAsync();
                return new BaseResultDto<List<ConsultationPackageAdminVDto>>(true, rows);
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationPackageAdminVDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        private sealed class Row
        {
            public ConsultationPurchase Purchase { get; set; }
            public string CompanionName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Mobile { get; set; }
            public string AgentFirst { get; set; }
            public string AgentLast { get; set; }
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
                ChannelId = s.ChannelId,
                DurationMinutes = s.DurationMinutes,
                Price = s.Price,
                RebatePrice = s.RebatePrice,
                WalletPrice = s.WalletPrice,
                PaymentPrice = s.PaymentPrice,
                CompanionShare = s.CompanionShare,
                SiteShare = s.SiteShare,
                Status = s.Status,
                CreateDate = s.CreateDate,
                PaidDate = s.PaidDate,
                StartDeadline = s.StartDeadline,
                StartDate = s.StartDate,
                ExpireDate = s.ExpireDate,
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
