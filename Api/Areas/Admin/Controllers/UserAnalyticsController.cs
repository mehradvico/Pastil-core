using Application.Common.Dto.Result;
using Application.Services.CommonSrv.UserActivitySrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// آنالیز رفتار کاربر (ورود، روزهای فعال/بیکار، صفحات و کلیک‌ها) برای پنل
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class UserAnalyticsController : ControllerBase
    {
        private readonly IUserActivityService _service;

        public UserAnalyticsController(IUserActivityService service)
        {
            _service = service;
        }

        [HttpGet("{userId:long}")]
        public async Task<IActionResult> Get(long userId, [FromQuery] int days = 30, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            var data = await _service.AnalyzeAsync(userId, days, from, to);
            return Ok(new BaseResultDto<UserAnalyticsDto>(true, data));
        }
    }
}
