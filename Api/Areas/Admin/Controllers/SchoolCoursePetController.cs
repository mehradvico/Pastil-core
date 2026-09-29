using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolCoursePetSrv.Dto;
using Application.Services.SchoolSrvs.SchoolCoursePetSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پت‌های مورد پذیرش هر دوره‌ی مدرسه
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolCoursePetController : ControllerBase
    {
        private readonly ISchoolCoursePetService _SchoolCoursePetService;
        public SchoolCoursePetController(ISchoolCoursePetService SchoolCoursePetService)
        {
            this._SchoolCoursePetService = SchoolCoursePetService;
        }

        [HttpGet()]
        [ProducesResponseType(typeof(SchoolCoursePetSearchDto), 200)]
        public IActionResult Get([FromQuery] SchoolCoursePetInputDto dto)
        {
            var search = _SchoolCoursePetService.Search(dto);
            return Ok(search);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCoursePetDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var item = await _SchoolCoursePetService.FindAsyncDto(id);
            return Ok(item);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCoursePetDto>), 200)]
        public async Task<IActionResult> Post(SchoolCoursePetDto dto)
        {
            var result = await _SchoolCoursePetService.InsertAsyncDto(dto);
            return Ok(result);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = _SchoolCoursePetService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
