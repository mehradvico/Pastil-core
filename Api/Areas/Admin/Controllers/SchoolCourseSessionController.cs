using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Dto;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// جلسات زنده‌ی دوره‌ی مدرسه
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolCourseSessionController : ControllerBase
    {
        private readonly ISchoolCourseService _schoolCourseService;
        public SchoolCourseSessionController(ISchoolCourseService schoolCourseService)
        {
            this._schoolCourseService = schoolCourseService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCourseSessionDto>), 200)]
        public async Task<IActionResult> Post(SchoolCourseSessionDto dto)
        {
            var result = await _schoolCourseService.UpsertSessionAsync(dto);
            return Ok(result);
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCourseSessionDto>), 200)]
        public async Task<IActionResult> Put(SchoolCourseSessionDto dto)
        {
            var result = await _schoolCourseService.UpsertSessionAsync(dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _schoolCourseService.DeleteSessionAsync(id);
            return Ok(result);
        }
    }
}
