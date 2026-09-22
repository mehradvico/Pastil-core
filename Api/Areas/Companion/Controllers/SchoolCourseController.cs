using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Dto;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Iface;
using Application.Services.SchoolSrvs.SchoolSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت دوره‌های مدرسه‌ی خودِ نماینده
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolCourseController : ControllerBase
    {
        private readonly ISchoolCourseService _schoolCourseService;
        private readonly ISchoolService _schoolService;
        private readonly ICurrentUserHelper _currentUser;
        public SchoolCourseController(ISchoolCourseService schoolCourseService, ISchoolService schoolService, ICurrentUserHelper currentUser)
        {
            this._schoolCourseService = schoolCourseService;
            this._schoolService = schoolService;
            this._currentUser = currentUser;
        }

        private async Task<bool> OwnsSchoolAsync(long schoolId)
        {
            var school = await _schoolService.FindAsyncVDto(schoolId);
            return school.IsSuccess && _currentUser.CurrentUser.CompanionId.HasValue &&
                school.Data?.CompanionId == _currentUser.CurrentUser.CompanionId.Value;
        }

        [HttpGet()]
        [ProducesResponseType(typeof(SchoolCourseSearchDto), 200)]
        public async Task<IActionResult> Get([FromQuery] SchoolCourseInputDto dto)
        {
            if (dto.SchoolId.HasValue && !await OwnsSchoolAsync(dto.SchoolId.Value))
                return Forbid();
            var search = _schoolCourseService.Search(dto);
            return Ok(search);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCourseVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var course = await _schoolCourseService.FindAsyncVDto(id);
            if (!course.IsSuccess || !await OwnsSchoolAsync(course.Data.SchoolId))
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(course);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCourseDto>), 200)]
        public async Task<IActionResult> Post(SchoolCourseDto dto)
        {
            if (!await OwnsSchoolAsync(dto.SchoolId))
                return Forbid();
            // کمیسیون را نماینده تعیین نمی‌کند: دوره‌ی جدید با کمیسیون پیش‌فرض ۰ ساخته می‌شود و فقط ادمین تغییرش می‌دهد
            dto.Id = 0;
            dto.CommissionPercent = 0;
            var result = await _schoolCourseService.InsertAsyncDto(dto);
            return Ok(result);
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(SchoolCourseDto dto)
        {
            // مالکیت روی دوره‌ی ذخیره‌شده بررسی می‌شود، نه SchoolId بدنه (وگرنه با SchoolId خودش می‌شد دوره‌ی دیگری را ویرایش/منتقل کرد)
            var existing = await _schoolCourseService.FindAsyncVDto(dto.Id);
            if (!existing.IsSuccess || existing.Data == null || !await OwnsSchoolAsync(existing.Data.SchoolId))
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));

            dto.SchoolId = existing.Data.SchoolId;
            // کمیسیون فقط ادمین؛ مقدار ذخیره‌شده حفظ می‌شود
            dto.CommissionPercent = existing.Data.CommissionPercent;
            var result = _schoolCourseService.UpdateDto(dto);
            return Ok(result);
        }

        [HttpPut("active/{id}/{active}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PutActive(long id, bool active)
        {
            var result = await _schoolCourseService.UpdateActiveAsync(id, active, _currentUser.CurrentUser.CompanionId);
            return Ok(result);
        }
    }
}
