using Application.Services.WeekDaySrv.WeekDaySrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// فهرست ثابت روزهای هفته — برای کاربر نهایی (مثلاً انتخاب برنامه‌ی هفتگی سرویس پت‌رسان)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class WeekDayController : ControllerBase
    {
        private readonly IWeekDayService _weekDayService;
        public WeekDayController(IWeekDayService weekDayService)
        {
            _weekDayService = weekDayService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_weekDayService.GetWeekDays());
        }
    }
}
