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
                var name = await _context.Companions.AsNoTracking()
                    .Where(c => c.Id == companionId && !c.Deleted)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync();
                if (name == null)
                    return new BaseResultDto<List<ConsultationPackageAdminVDto>>(false, Resource.Notification.NothingFound, null);

                var matrix = await _packageService.GetMatrixAsync(companionId);
                if (!matrix.IsSuccess)
                    return new BaseResultDto<List<ConsultationPackageAdminVDto>>(false, Resource.Notification.Unsuccess, null);
                return new BaseResultDto<List<ConsultationPackageAdminVDto>>(true, ToAdminList(matrix.Data, companionId, name));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationPackageAdminVDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        public async Task<BaseResultDto<List<ConsultationPackageAdminVDto>>> SaveClinicPackagesAsync(long companionId, ConsultationPackageSaveDto dto)
        {
            try
            {
                var name = await _context.Companions.AsNoTracking()
                    .Where(c => c.Id == companionId && !c.Deleted)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync();
                if (name == null)
                    return new BaseResultDto<List<ConsultationPackageAdminVDto>>(false, Resource.Notification.NothingFound, null);

                // اعتبارسنجی و upsert همان سرویس نماینده است (قیمت ≥ ۰؛ فعال فقط با قیمت > ۰؛ مدت فقط ۳۰/۶۰)
                var saved = await _packageService.SaveMatrixAsync(companionId, dto);
                if (!saved.IsSuccess)
                    return new BaseResultDto<List<ConsultationPackageAdminVDto>>(false, saved.Messages, null);
                return new BaseResultDto<List<ConsultationPackageAdminVDto>>(true, ToAdminList(saved.Data, companionId, name));
            }
            catch (Exception ex)
            {
                return new BaseResultDto<List<ConsultationPackageAdminVDto>>(false, ExceptionResultHelper.ToClientMessage(ex), null);
            }
        }

        private static List<ConsultationPackageAdminVDto> ToAdminList(IEnumerable<ConsultationPackageItemDto> items, long companionId, string name) =>
            items.OrderBy(i => i.ChannelId).ThenBy(i => i.DurationMinutes)
                .Select(i => new ConsultationPackageAdminVDto
                {
                    Id = i.Id,
                    CompanionId = companionId,
                    CompanionName = name,
                    ChannelId = i.ChannelId,
                    DurationMinutes = i.DurationMinutes,
                    Price = i.Price,
                    Active = i.Active
                }).ToList();

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
