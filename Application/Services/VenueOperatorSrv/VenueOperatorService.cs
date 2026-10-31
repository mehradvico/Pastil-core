using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.VenueOperatorSrv
{
    /// <summary>بدنه‌ی سپردن رزرو کلینیک/آرایشگاه/مربی به پرسنل؛ AssigneeUserId خالی = برداشتن پرسنل.</summary>
    public class VenueReserveAssignDto
    {
        public long Id { get; set; }
        public long? AssigneeUserId { get; set; }
    }

    /// <summary>یک مرکز که کاربر در آن پرسنل (یا مالک) است. Kind: clinic | groomer | coach.</summary>
    public class VenueWorkplaceDto
    {
        public string Kind { get; set; }
        public long VenueId { get; set; }
        public string Name { get; set; }
        public bool IsOwner { get; set; }
        /// <summary>رزروهای پرداخت‌شده، لغونشده و تکمیل‌نشده‌ای که الان به این کاربر سپرده شده.</summary>
        public int OpenAssignedCount { get; set; }
    }

    public interface IVenueOperatorService
    {
        /// <summary>مرکزهایی که کاربر مالک یا عضو پذیرفته‌شده‌ی فعال آن‌هاست؛ لیست خالی = منوی پنل اپراتور مراکز را نشان ندهید.</summary>
        Task<List<VenueWorkplaceDto>> GetWorkplacesAsync(long userId);
    }

    /// <summary>
    /// پنل اپراتور برای پرسنل کلینیک/آرایشگاه/مربی (۲۰۲۶-۱۰-۳۱). قبلاً پنل اپراتور فقط رزرو نمایندگی را می‌شناخت
    /// و پرسنل این مرکزها با این‌که روی رزرو می‌نشستند هیچ لیستی نداشتند. طراحی: backend/Docs/VENUE_STAFF_OPERATOR_PANEL_FA.md
    /// </summary>
    public class VenueOperatorService : IVenueOperatorService
    {
        private readonly IDataBaseContext _context;

        public VenueOperatorService(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<List<VenueWorkplaceDto>> GetWorkplacesAsync(long userId)
        {
            var result = new List<VenueWorkplaceDto>();

            var clinics = await _context.Clinics.AsNoTracking()
                .Where(c => !c.Deleted && (c.OwnerId == userId || c.ClinicUsers.Any(u => u.UserId == userId && !u.Deleted && u.Active && u.UserAccept == true)))
                .Select(c => new VenueWorkplaceDto
                {
                    Kind = "clinic", VenueId = c.Id, Name = c.Name, IsOwner = c.OwnerId == userId,
                    OpenAssignedCount = _context.ClinicReserves.Count(r => r.ClinicId == c.Id && r.AssigneeUserId == userId && r.IsReserved && !r.IsCancel && r.Status != (int)ClinicSrvs.ClinicReserveSrv.ClinicReserveStatusEnum.Complete),
                }).ToListAsync();
            result.AddRange(clinics);

            var groomers = await _context.Groomers.AsNoTracking()
                .Where(c => !c.Deleted && (c.OwnerId == userId || c.GroomerUsers.Any(u => u.UserId == userId && !u.Deleted && u.Active && u.UserAccept == true)))
                .Select(c => new VenueWorkplaceDto
                {
                    Kind = "groomer", VenueId = c.Id, Name = c.Name, IsOwner = c.OwnerId == userId,
                    OpenAssignedCount = _context.GroomerReserves.Count(r => r.GroomerId == c.Id && r.AssigneeUserId == userId && r.IsReserved && !r.IsCancel && r.Status != (int)GroomerSrvs.GroomerReserveSrv.GroomerReserveStatusEnum.Complete),
                }).ToListAsync();
            result.AddRange(groomers);

            var coaches = await _context.Coaches.AsNoTracking()
                .Where(c => !c.Deleted && (c.OwnerId == userId || c.CoachUsers.Any(u => u.UserId == userId && !u.Deleted && u.Active && u.UserAccept == true)))
                .Select(c => new VenueWorkplaceDto
                {
                    Kind = "coach", VenueId = c.Id, Name = c.Name, IsOwner = c.OwnerId == userId,
                    OpenAssignedCount = _context.CoachReserves.Count(r => r.CoachId == c.Id && r.AssigneeUserId == userId && r.IsReserved && !r.IsCancel && r.Status != (int)CoachSrvs.CoachReserveSrv.CoachReserveStatusEnum.Complete),
                }).ToListAsync();
            result.AddRange(coaches);

            return result;
        }
    }
}
