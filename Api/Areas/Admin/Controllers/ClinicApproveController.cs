using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicSrv.Dto;
using Application.Services.ClinicSrvs.ClinicSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تأیید/رد کلینیک (رد نیاز به دلیل دارد)
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicApproveController : ControllerBase
    {
        private readonly IClinicCoreService _service;
        public ClinicApproveController(IClinicCoreService service) => _service = service;

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(ClinicApproveDto dto) => Ok(await _service.UpdateApproveAsync(dto));
    }
}
