using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolSrv.Dto;
using Application.Services.SchoolSrvs.SchoolSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// فعال‌سازی مدرسه
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolActiveController : ControllerBase
    {
        private readonly ISchoolService _schoolService;
        public SchoolActiveController(ISchoolService schoolService)
        {
            this._schoolService = schoolService;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(SchoolActiveDto dto)
        {
            var school = _schoolService.UpdateSchoolActiveDto(dto);
            return Ok(school);
        }
    }
}
