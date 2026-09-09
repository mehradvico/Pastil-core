using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Dto;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Iface;
using Application.Services.SchoolSrvs.SchoolReserveSrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مشاهده‌ی عمومی دوره‌های مدرسه (شامل ظرفیت باقی‌مانده)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolCourseController : ControllerBase
    {
        private readonly ISchoolCourseService _schoolCourseService;
        private readonly ISchoolReserveService _schoolReserveService;
        public SchoolCourseController(ISchoolCourseService schoolCourseService, ISchoolReserveService schoolReserveService)
        {
            this._schoolCourseService = schoolCourseService;
            this._schoolReserveService = schoolReserveService;
        }

        [HttpGet()]
        [ProducesResponseType(typeof(SchoolCourseSearchDto), 200)]
        public IActionResult Get([FromQuery] SchoolCourseInputDto dto)
        {
            dto.Available = true;
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

        [HttpGet("remainingCapacity/{id}")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> GetRemainingCapacity(long id)
        {
            var remaining = await _schoolReserveService.GetRemainingCapacityAsync(id);
            return Ok(remaining);
        }
    }
}
