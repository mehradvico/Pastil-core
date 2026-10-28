using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolInsightSrv;
using Application.Services.SchoolSrvs.SchoolOwnershipSrv;
using Application.Services.SchoolSrvs.SchoolTeachingSrv;
using Application.Services.SchoolSrvs.SchoolWaitlistSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت آموزشی دوره‌های مدرسه برای ارائه‌دهنده و مدرس‌ها (۲۰۲۶-۱۰-۲۸): مدرس‌ها، حضور و غیاب، مهارت‌ها و کارنامه،
    /// گواهی، لیست انتظار، امتیازها و داشبورد مدرسه.
    /// </summary>
    /// <remarks>
    /// «مالک» = کاربرِ ارائه‌دهنده (یا مالک نمایندگیِ مدرسه‌ی قدیمی). «مدرس» = مالک یا مدرس فعالِ همان دوره.
    /// مدیریت مدرس‌ها، لیست انتظار و داشبورد فقط برای مالک؛ حضور و غیاب، کارنامه و گواهی برای مدرس هم.
    /// </remarks>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolTeachingController : ControllerBase
    {
        private readonly ISchoolTeachingService _teaching;
        private readonly ISchoolOwnershipService _ownership;
        private readonly ISchoolWaitlistService _waitlist;
        private readonly ISchoolInsightService _insight;
        private readonly ICurrentUserHelper _currentUser;

        public SchoolTeachingController(ISchoolTeachingService teaching, ISchoolOwnershipService ownership, ISchoolWaitlistService waitlist,
            ISchoolInsightService insight, ICurrentUserHelper currentUser)
        {
            _teaching = teaching;
            _ownership = ownership;
            _waitlist = waitlist;
            _insight = insight;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;
        private async Task<bool> OwnsCourseAsync(long courseId) => await _ownership.SchoolOfCourseAsync(courseId, UserId) != null;
        private static IActionResult Denied() => new OkObjectResult(new BaseResultDto(false, Resource.Notification.AccessDenied));

        // ---------------------------------------------------------------- مدرس‌ها (مالک)

        /// <summary>مدرس‌های دوره</summary>
        [HttpGet("Instructors")]
        [ProducesResponseType(typeof(BaseResultDto<List<SchoolInstructorVDto>>), 200)]
        public async Task<IActionResult> Instructors([FromQuery] long courseId) =>
            await _teaching.CanTeachCourseAsync(courseId, UserId) ? Ok(await _teaching.InstructorsAsync(courseId)) : Denied();

        /// <summary>افزودن مدرس با موبایل (IsLead = مدرس اصلی؛ فقط یکی)</summary>
        [HttpPost("Instructors")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolInstructorVDto>), 200)]
        public async Task<IActionResult> AddInstructor(SchoolInstructorAddDto dto) =>
            dto != null && await OwnsCourseAsync(dto.SchoolCourseId) ? Ok(await _teaching.AddInstructorAsync(dto)) : Denied();

        [HttpDelete("Instructors")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> RemoveInstructor([FromQuery] long courseId, [FromQuery] long id) =>
            await OwnsCourseAsync(courseId) ? Ok(await _teaching.RemoveInstructorAsync(courseId, id)) : Denied();

        /// <summary>شناسه‌ی دوره‌هایی که من مدرسشان هستم (مدرسِ غیرمالک از این‌جا دوره‌هایش را پیدا می‌کند)</summary>
        [HttpGet("MyCourses")]
        [ProducesResponseType(typeof(BaseResultDto<List<long>>), 200)]
        public async Task<IActionResult> MyCourses() => Ok(new BaseResultDto<List<long>>(true, await _teaching.CoursesOfInstructorAsync(UserId)));

        // ---------------------------------------------------------------- حضور و غیاب (مدرس)

        /// <summary>لیست حاضرین یک جلسه با وضعیت حضور ثبت‌شده</summary>
        [HttpGet("Roster")]
        [ProducesResponseType(typeof(BaseResultDto<List<SchoolRosterItemVDto>>), 200)]
        public async Task<IActionResult> Roster([FromQuery] long sessionId)
        {
            var courseId = await _teaching.CourseOfSessionAsync(sessionId);
            return courseId.HasValue && await _teaching.CanTeachCourseAsync(courseId.Value, UserId) ? Ok(await _teaching.RosterAsync(sessionId)) : Denied();
        }

        /// <summary>ثبت/ویرایش حضور و غیاب یک جلسه؛ غایب‌ها یک‌بار پوش «جلسه را از دست دادید» می‌گیرند</summary>
        [HttpPut("Attendance")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Attendance(SchoolAttendanceSaveDto dto)
        {
            var courseId = dto == null ? null : await _teaching.CourseOfSessionAsync(dto.SchoolCourseSessionId);
            return courseId.HasValue && await _teaching.CanTeachCourseAsync(courseId.Value, UserId) ? Ok(await _teaching.SaveAttendanceAsync(dto, UserId)) : Denied();
        }

        // ---------------------------------------------------------------- مهارت‌ها و کارنامه (مدرس)

        [HttpGet("Skills")]
        [ProducesResponseType(typeof(BaseResultDto<List<SchoolSkillDto>>), 200)]
        public async Task<IActionResult> Skills([FromQuery] long courseId) =>
            await _teaching.CanTeachCourseAsync(courseId, UserId) ? Ok(await _teaching.SkillsAsync(courseId)) : Denied();

        /// <summary>افزودن (Id = 0) یا ویرایش مهارت دوره</summary>
        [HttpPost("Skills")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolSkillDto>), 200)]
        public async Task<IActionResult> UpsertSkill(SchoolSkillDto dto) =>
            dto != null && await _teaching.CanTeachCourseAsync(dto.SchoolCourseId, UserId) ? Ok(await _teaching.UpsertSkillAsync(dto)) : Denied();

        [HttpDelete("Skills")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> DeleteSkill([FromQuery] long courseId, [FromQuery] long id) =>
            await _teaching.CanTeachCourseAsync(courseId, UserId) ? Ok(await _teaching.DeleteSkillAsync(courseId, id)) : Denied();

        /// <summary>ثبت نمره‌ی مهارت‌ها (۱ تا ۵) برای یک ثبت‌نام؛ اختیاری برای یک جلسه‌ی مشخص</summary>
        [HttpPost("Progress")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Progress(SchoolProgressSaveDto dto)
        {
            var courseId = dto == null ? null : await _teaching.CourseOfReserveAsync(dto.SchoolReserveId);
            return courseId.HasValue && await _teaching.CanTeachCourseAsync(courseId.Value, UserId) ? Ok(await _teaching.SaveProgressAsync(dto, UserId)) : Denied();
        }

        /// <summary>کارنامه‌ی یک ثبت‌نام (حضور، مهارت‌ها، گواهی)</summary>
        [HttpGet("Report")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolReportVDto>), 200)]
        public async Task<IActionResult> Report([FromQuery] long reserveId)
        {
            var courseId = await _teaching.CourseOfReserveAsync(reserveId);
            return courseId.HasValue && await _teaching.CanTeachCourseAsync(courseId.Value, UserId) ? Ok(await _teaching.ReportAsync(reserveId)) : Denied();
        }

        /// <summary>صدور گواهی پایان دوره (ثبت‌نام «کامل‌شده» در دوره‌ی دارای گواهی؛ تکرار = همان گواهی)</summary>
        [HttpPost("Certificate")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCertificateVDto>), 200)]
        public async Task<IActionResult> Certificate([FromQuery] long reserveId)
        {
            var courseId = await _teaching.CourseOfReserveAsync(reserveId);
            return courseId.HasValue && await _teaching.CanTeachCourseAsync(courseId.Value, UserId) ? Ok(await _teaching.IssueCertificateAsync(reserveId, UserId)) : Denied();
        }

        // ---------------------------------------------------------------- لیست انتظار، امتیاز، داشبورد (مالک)

        [HttpGet("Waitlist")]
        [ProducesResponseType(typeof(BaseResultDto<List<SchoolWaitlistVDto>>), 200)]
        public async Task<IActionResult> Waitlist([FromQuery] long courseId) =>
            await OwnsCourseAsync(courseId) ? Ok(await _waitlist.CourseAsync(courseId)) : Denied();

        [HttpGet("Rates")]
        [ProducesResponseType(typeof(BaseResultDto<List<SchoolCourseRateVDto>>), 200)]
        public async Task<IActionResult> Rates([FromQuery] long courseId) =>
            await _teaching.CanTeachCourseAsync(courseId, UserId) ? Ok(await _insight.CourseRatesAsync(courseId, 200)) : Denied();

        /// <summary>داشبورد مدرسه: ثبت‌نام، پر شدن ظرفیت، لیست انتظار، درآمد، درصد حضور و امتیاز هر دوره</summary>
        [HttpGet("Dashboard")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolDashboardVDto>), 200)]
        public async Task<IActionResult> Dashboard([FromQuery] long schoolId) =>
            await _ownership.SchoolOfSchoolAsync(schoolId, UserId, approvedOnly: false) != null ? Ok(await _insight.DashboardAsync(schoolId)) : Denied();
    }
}
