using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolInsightSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// نظر/امتیاز دوره‌های مدرسه (۲۰۲۶-۱۰-۲۸): امتیاز همان لحظه حساب می‌شود؛ ادمین با رد کردن، نظر را پنهان و از میانگین خارج می‌کند.
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolCourseRateController : ControllerBase
    {
        private readonly ISchoolInsightService _insight;

        public SchoolCourseRateController(ISchoolInsightService insight) => _insight = insight;

        [HttpGet("{courseId:long}")]
        [ProducesResponseType(typeof(BaseResultDto<List<SchoolCourseRateVDto>>), 200)]
        public async Task<IActionResult> Get(long courseId) => Ok(await _insight.CourseRatesAsync(courseId, 200));

        /// <summary>تأیید (accept=true) یا رد نظر</summary>
        [HttpPut("{id:long}/Status")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Status(long id, [FromQuery] bool accept) => Ok(await _insight.SetRateStatusAsync(id, accept));

        /// <summary>داشبورد یک مدرسه (همان داشبورد ارائه‌دهنده)</summary>
        [HttpGet("Dashboard/{schoolId:long}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolDashboardVDto>), 200)]
        public async Task<IActionResult> Dashboard(long schoolId) => Ok(await _insight.DashboardAsync(schoolId));
    }
}
