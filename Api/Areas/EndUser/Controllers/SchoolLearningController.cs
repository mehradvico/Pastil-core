using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolInsightSrv;
using Application.Services.SchoolSrvs.SchoolTeachingSrv;
using Application.Services.SchoolSrvs.SchoolWaitlistSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدرسه برای مالک پت (۲۰۲۶-۱۰-۲۸): لیست انتظار دوره‌ی پر، کارنامه‌ی پیشرفت/حضور/گواهی ثبت‌نام خودم، امتیاز به دوره.
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolLearningController : ControllerBase
    {
        private readonly ISchoolWaitlistService _waitlist;
        private readonly ISchoolTeachingService _teaching;
        private readonly ISchoolInsightService _insight;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IDataBaseContext _context;

        public SchoolLearningController(ISchoolWaitlistService waitlist, ISchoolTeachingService teaching, ISchoolInsightService insight,
            ICurrentUserHelper currentUser, IDataBaseContext context)
        {
            _waitlist = waitlist;
            _teaching = teaching;
            _insight = insight;
            _currentUser = currentUser;
            _context = context;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        /// <summary>لیست انتظارهای من (با جایگاه در صف)</summary>
        [HttpGet("Waitlist")]
        [ProducesResponseType(typeof(BaseResultDto<List<SchoolWaitlistVDto>>), 200)]
        public async Task<IActionResult> Waitlist() => Ok(await _waitlist.MineAsync(UserId));

        /// <summary>عضویت در لیست انتظار دوره‌ی پر (فقط وقتی ظرفیت پر است)</summary>
        [HttpPost("Waitlist")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolWaitlistVDto>), 200)]
        public async Task<IActionResult> JoinWaitlist(SchoolWaitlistJoinDto dto) => Ok(await _waitlist.JoinAsync(UserId, dto));

        [HttpDelete("Waitlist")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> LeaveWaitlist([FromQuery] long id) => Ok(await _waitlist.LeaveAsync(UserId, id));

        /// <summary>کارنامه‌ی ثبت‌نام خودم: حضور و غیاب، نمره‌ی مهارت‌ها و گواهی</summary>
        [HttpGet("Report")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolReportVDto>), 200)]
        public async Task<IActionResult> Report([FromQuery] long reserveId)
        {
            if (!await _context.SchoolReserves.AnyAsync(r => r.Id == reserveId && r.BookerId == UserId))
                return Ok(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(await _teaching.ReportAsync(reserveId));
        }

        /// <summary>امتیاز و نظر به دوره (فقط ثبت‌نام «کامل‌شده»؛ ارسال دوباره = ویرایش)</summary>
        [HttpPost("Rate")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCourseRateVDto>), 200)]
        public async Task<IActionResult> Rate(SchoolCourseRateDto dto) => Ok(await _insight.RateAsync(UserId, dto));
    }
}
