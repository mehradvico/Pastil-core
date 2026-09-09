using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolSrv.Dto;
using Application.Services.SchoolSrvs.SchoolSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدرسه ها
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
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

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<SchoolDto>), 200)]
        public async Task<IActionResult> Post(SchoolDto dto)
        {
            var result = await _schoolService.InsertAsyncDto(dto);
            return Ok(result);
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(SchoolDto dto)
        {
            var school = _schoolService.UpdateDto(dto);
            return Ok(school);
        }
    }
}
