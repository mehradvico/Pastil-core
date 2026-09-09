using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Dto;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// دوره‌های مدرسه
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolCourseController : ControllerBase
    {
        private readonly ISchoolCourseService _schoolCourseService;
        public SchoolCourseController(ISchoolCourseService schoolCourseService)
        {
            this._schoolCourseService = schoolCourseService;
        }

        [HttpGet()]
        [ProducesResponseType(typeof(SchoolCourseSearchDto), 200)]
        public IActionResult Get([FromQuery] SchoolCourseInputDto dto)
        {
            var search = _schoolCourseService.Search(dto);
            return Ok(search);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCourseVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var course = await _schoolCourseService.FindAsyncVDto(id);
            return Ok(course);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCourseDto>), 200)]
        public async Task<IActionResult> Post(SchoolCourseDto dto)
        {
            var result = await _schoolCourseService.InsertAsyncDto(dto);
            return Ok(result);
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(SchoolCourseDto dto)
        {
            var course = _schoolCourseService.UpdateDto(dto);
            return Ok(course);
        }

        [HttpPut("active/{id}/{active}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PutActive(long id, bool active)
        {
            var result = await _schoolCourseService.UpdateActiveAsync(id, active);
            return Ok(result);
        }
    }
}
