using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolCoursePetSrv.Dto;
using Application.Services.SchoolSrvs.SchoolCoursePetSrv.Iface;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت پت‌های مورد پذیرش هر دوره (توسط خودِ نماینده)
    /// </summary>
    ///
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolCoursePetController : ControllerBase
    {
        private readonly ISchoolCoursePetService _SchoolCoursePetService;
        private readonly ISchoolCourseService _SchoolCourseService;
        private readonly ICurrentUserHelper _currentUser;

        public SchoolCoursePetController(ISchoolCoursePetService SchoolCoursePetService, ISchoolCourseService SchoolCourseService, ICurrentUserHelper currentUser)
        {
            this._SchoolCoursePetService = SchoolCoursePetService;
            this._SchoolCourseService = SchoolCourseService;
            this._currentUser = currentUser;
        }

        private async Task<bool> OwnsCourseAsync(long courseId)
        {
            var course = await _SchoolCourseService.FindAsyncVDto(courseId);
            return course.IsSuccess && _currentUser.CurrentUser.CompanionId.HasValue &&
                course.Data?.School?.CompanionId == _currentUser.CurrentUser.CompanionId.Value;
        }

        [HttpGet()]
        [ProducesResponseType(typeof(SchoolCoursePetSearchDto), 200)]
        public IActionResult Get([FromQuery] SchoolCoursePetInputDto dto)
        {
            dto.CompanionId = _currentUser.CurrentUser.CompanionId;
            var search = _SchoolCoursePetService.Search(dto);
            return Ok(search);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCoursePetVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var item = await _SchoolCoursePetService.FindAsyncVDto(id);
            return Ok(item);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCoursePetDto>), 200)]
        public async Task<IActionResult> Post(SchoolCoursePetDto dto)
        {
            if (!await OwnsCourseAsync(dto.SchoolCourseId))
                return Ok(new BaseResultDto<SchoolCoursePetDto>(false, Resource.Notification.AccessDenied, dto));

            var result = await _SchoolCoursePetService.InsertAsyncDto(dto);
            return Ok(result);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await _SchoolCoursePetService.FindAsyncVDto(id);
            if (!existing.IsSuccess || !await OwnsCourseAsync(existing.Data.SchoolCourseId))
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));

            var dto = _SchoolCoursePetService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
