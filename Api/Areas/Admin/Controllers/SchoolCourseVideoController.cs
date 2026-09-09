using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Dto;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// ویدیوهای دوره‌ی مدرسه
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolCourseVideoController : ControllerBase
    {
        private readonly ISchoolCourseService _schoolCourseService;
        public SchoolCourseVideoController(ISchoolCourseService schoolCourseService)
        {
            this._schoolCourseService = schoolCourseService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCourseVideoDto>), 200)]
        public async Task<IActionResult> Post(SchoolCourseVideoDto dto)
        {
            var result = await _schoolCourseService.UpsertVideoAsync(dto);
            return Ok(result);
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCourseVideoDto>), 200)]
        public async Task<IActionResult> Put(SchoolCourseVideoDto dto)
        {
            var result = await _schoolCourseService.UpsertVideoAsync(dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _schoolCourseService.DeleteVideoAsync(id);
            return Ok(result);
        }
    }
}
