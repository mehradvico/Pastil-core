using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolSrv.Dto;
using Application.Services.SchoolSrvs.SchoolSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تایید درخواست مدرسه
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolApproveController : ControllerBase
    {
        private readonly ISchoolService _schoolService;
        public SchoolApproveController(ISchoolService schoolService)
        {
            this._schoolService = schoolService;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(SchoolApproveDto dto)
        {
            var school = await _schoolService.UpdateSchoolApproveAsyncDto(dto);
            return Ok(school);
        }
    }
}
