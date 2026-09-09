using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Dto;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// ویدیوهای دوره‌ی مدرسه‌ی خودِ نماینده
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolCourseVideoController : ControllerBase
    {
        private readonly ISchoolCourseService _schoolCourseService;
        private readonly ICurrentUserHelper _currentUser;
        public SchoolCourseVideoController(ISchoolCourseService schoolCourseService, ICurrentUserHelper currentUser)
        {
            this._schoolCourseService = schoolCourseService;
            this._currentUser = currentUser;
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCourseVideoDto>), 200)]
        public async Task<IActionResult> Post(SchoolCourseVideoDto dto)
        {
            var result = await _schoolCourseService.UpsertVideoAsync(dto, _currentUser.CurrentUser.CompanionId);
            return Ok(result);
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCourseVideoDto>), 200)]
        public async Task<IActionResult> Put(SchoolCourseVideoDto dto)
        {
            var result = await _schoolCourseService.UpsertVideoAsync(dto, _currentUser.CurrentUser.CompanionId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _schoolCourseService.DeleteVideoAsync(id, _currentUser.CurrentUser.CompanionId);
            return Ok(result);
        }
    }
}
