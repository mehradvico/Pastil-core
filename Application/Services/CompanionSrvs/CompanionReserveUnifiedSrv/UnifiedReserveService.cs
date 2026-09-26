using Application.Services.CompanionSrvs.CompanionReserveUnifiedSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveUnifiedSrv.Iface;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveUnifiedSrv
{
    /// <summary>
    /// لیست یکپارچه‌ی «رزروهای نماینده» برای پنل ادمین: رزروهای خدمت (CompanionReserve) و خریدهای بسته‌ی مشاوره آنلاین
    /// (ConsultationPurchase) دو موجودیت جدا هستند؛ اینجا در یک شکل مشترک و با یک صفحه‌بندی (UNION در دیتابیس) برمی‌گردند.
    /// </summary>
    public class UnifiedReserveService : IUnifiedReserveService
    {
        private readonly IDataBaseContext _context;

        public UnifiedReserveService(IDataBaseContext context)
        {
            _context = context;
        }

        public static IQueryable<UnifiedReserveVDto> ServiceRows(IQueryable<Entities.Entities.CompanionReserve> services) =>
            services.Select(s => new UnifiedReserveVDto
            {
                Kind = UnifiedReserveKinds.Service,
                Id = s.Id,
                Code = s.ReserveCode,
                BookerName = ((s.Booker.FirstName ?? "") + " " + (s.Booker.LastName ?? "")).Trim(),
                BookerMobile = s.Booker.Mobile,
                CompanionId = s.CompanionAssistance.CompanionId,
                CompanionName = s.CompanionAssistance.Companion.Name,
                Title = s.CompanionAssistance.Assistance.Name,
                ChannelId = null,
                DurationMinutes = null,
                DoDate = s.DoDate,
                PaymentPrice = s.PaymentPrice,
                IsCancel = s.IsCancel,
                StatusName = s.State.Name,
                ConsultationStatus = null,
                CreateDate = s.CreateDate
            });

        public static IQueryable<UnifiedReserveVDto> ConsultationRows(IQueryable<Entities.Entities.ConsultationPurchase> consultations) =>
            consultations.Select(c => new UnifiedReserveVDto
            {
                Kind = UnifiedReserveKinds.Consultation,
                Id = c.Id,
                Code = c.PurchaseCode,
                BookerName = ((c.User.FirstName ?? "") + " " + (c.User.LastName ?? "")).Trim(),
                BookerMobile = c.User.Mobile,
                CompanionId = c.CompanionId,
                CompanionName = c.Companion.Name,
                Title = c.PackageName,
                ChannelId = c.ChannelId,
                DurationMinutes = c.DurationMinutes,
                DoDate = c.StartDate ?? c.CreateDate,
                PaymentPrice = c.PaymentPrice,
                IsCancel = c.Status == 6 || c.Status == 7,
                StatusName = null,
                ConsultationStatus = c.Status,
                CreateDate = c.CreateDate
            });

        // ترکیب دو منبع در یک کوئری (UNION ALL)؛ جدا شده تا ترجمه‌ی SQL آن در تست بررسی شود
        public static IQueryable<UnifiedReserveVDto> BuildMergedQuery(
            string type,
            IQueryable<UnifiedReserveVDto> serviceRows,
            IQueryable<UnifiedReserveVDto> consultationRows) => type switch
        {
            "service" => serviceRows,
            "consultation" => consultationRows,
            _ => serviceRows.Concat(consultationRows)
        };

        public async Task<UnifiedReserveListDto> SearchAsync(UnifiedReserveInputDto dto)
        {
            dto ??= new UnifiedReserveInputDto();
            var pageIndex = Math.Max(1, dto.PageIndex);
            var pageSize = Math.Clamp(dto.PageSize, 1, 100);
            var type = (dto.Type ?? "all").Trim().ToLowerInvariant();
            var q = string.IsNullOrWhiteSpace(dto.Q) ? null : dto.Q.Trim();

            var services = _context.CompanionReserves.AsNoTracking().AsQueryable();
            var consultations = _context.ConsultationPurchases.AsNoTracking().AsQueryable();

            if (dto.CompanionId.HasValue)
            {
                var companionId = dto.CompanionId.Value;
                services = services.Where(s => s.CompanionAssistance.CompanionId == companionId);
                consultations = consultations.Where(c => c.CompanionId == companionId);
            }
            if (dto.BookerId.HasValue)
            {
                var bookerId = dto.BookerId.Value;
                services = services.Where(s => s.BookerId == bookerId);
                consultations = consultations.Where(c => c.UserId == bookerId);
            }
            if (q != null)
            {
                services = services.Where(s => s.ReserveCode == q || s.Booker.FirstName.Contains(q) ||
                                               s.Booker.LastName.Contains(q) || s.Booker.Mobile.Contains(q));
                consultations = consultations.Where(c => c.PurchaseCode == q || c.User.FirstName.Contains(q) ||
                                                         c.User.LastName.Contains(q) || c.User.Mobile.Contains(q));
            }

            var serviceCount = type == "consultation" ? 0 : await services.CountAsync();
            var consultationCount = type == "service" ? 0 : await consultations.CountAsync();

            var serviceRows = ServiceRows(services);

            var consultationRows = ConsultationRows(consultations);

            var result = new UnifiedReserveListDto
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                ServiceCount = serviceCount,
                ConsultationCount = consultationCount,
                TotalCount = serviceCount + consultationCount
            };

            if (result.TotalCount == 0)
                return result;

            var merged = BuildMergedQuery(type, serviceRows, consultationRows);

            result.List = await merged
                .OrderByDescending(r => r.CreateDate)
                .ThenByDescending(r => r.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return result;
        }
    }
}
