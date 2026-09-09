using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolSrv.Dto;
using Application.Services.SchoolSrvs.SchoolSrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مشاهده‌ی عمومی مدرسه‌ها
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolController : ControllerBase
    {
        private readonly ISchoolService _schoolService;
        public SchoolController(ISchoolService schoolService)
        {
            this._schoolService = schoolService;
        }

        [HttpGet()]
        [ProducesResponseType(typeof(SchoolSearchDto), 200)]
        public IActionResult Get([FromQuery] SchoolInputDto dto)
        {
            dto.Approve = true;
            dto.Available = true;
            dto.ShowToSite = true;
            var search = _schoolService.Search(dto);
            return Ok(search);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var school = await _schoolService.FindAsyncVDto(id);
            return Ok(school);
        }
    }
}
