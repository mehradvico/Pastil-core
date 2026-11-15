using Application.Common.Dto.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// آمار زنده‌ی تعداد کاربران برای داشبورد پنل (شمارنده‌ی رو به رشد)
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UserStatsController : ControllerBase
    {
        private readonly IDataBaseContext _context;

        public UserStatsController(IDataBaseContext context)
        {
            _context = context;
        }

        public class UserStatsDto
        {
            public int Total { get; set; }
            public int Today { get; set; }
            public int Last24Hours { get; set; }
            public int Last7Days { get; set; }
            // برای پیش‌بینی رشد: ثبت‌نام ۳۰ روز اخیر و ۳۰ روز قبل‌تر
            public int Last30Days { get; set; }
            public int Previous30Days { get; set; }
            // شمارنده‌های نمایش روی تلویزیون (زیر شمارنده‌ی کاربران): سفارش پرداخت‌شده‌ی پت‌شاپ و رزروهای ثبت‌شده‌ی غیرلغوشده
            public int Orders { get; set; }
            public int ClinicReserves { get; set; }
            public int GroomerReserves { get; set; }
            public int CoachReserves { get; set; }
            public int PansionReserves { get; set; }
            public int SchoolReserves { get; set; }
            public System.DateTime ServerNow { get; set; }
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<UserStatsDto>), 200)]
        public async Task<IActionResult> Get()
        {
            var now = DateTime.Now;
            var users = _context.Users.AsNoTracking().Where(u => !u.Deleted);
            var dto = new UserStatsDto
            {
                Total = await users.CountAsync(),
                Today = await users.CountAsync(u => u.CreateDate >= now.Date),
                Last24Hours = await users.CountAsync(u => u.CreateDate >= now.AddHours(-24)),
                Last7Days = await users.CountAsync(u => u.CreateDate >= now.AddDays(-7)),
                Last30Days = await users.CountAsync(u => u.CreateDate >= now.AddDays(-30)),
                Previous30Days = await users.CountAsync(u => u.CreateDate >= now.AddDays(-60) && u.CreateDate < now.AddDays(-30)),
                // Status: 0 = ثبت‌شده (پرداخت‌نشده)، ≥1 = پرداخت‌شده/تکمیل؛ لغوشده‌ها CancelDate دارند
                Orders = await _context.ProductOrders.AsNoTracking().CountAsync(o => !o.Deleted && o.IsPaid),
                ClinicReserves = await _context.ClinicReserves.AsNoTracking().CountAsync(r => r.Status >= 1 && r.CancelDate == null),
                GroomerReserves = await _context.GroomerReserves.AsNoTracking().CountAsync(r => r.Status >= 1 && r.CancelDate == null),
                CoachReserves = await _context.CoachReserves.AsNoTracking().CountAsync(r => r.Status >= 1 && r.CancelDate == null),
                PansionReserves = await _context.PansionReserves.AsNoTracking().CountAsync(r => r.IsReserved && !r.IsCancel),
                SchoolReserves = await _context.SchoolReserves.AsNoTracking().CountAsync(r => r.IsReserved && !r.IsCancel),
                ServerNow = now
            };
            return Ok(new BaseResultDto<UserStatsDto>(true, dto));
        }
    }
}
