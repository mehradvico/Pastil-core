using Application.Common.Dto.Result;
using Application.Services.GroomerSrvs.GroomerSrv.Dto;
using Application.Services.GroomerSrvs.GroomerSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تأیید/رد آرایشگاه (رد نیاز به دلیل دارد)
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerApproveController : ControllerBase
    {
        private readonly IGroomerCoreService _service;
        public GroomerApproveController(IGroomerCoreService service) => _service = service;

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(GroomerApproveDto dto) => Ok(await _service.UpdateApproveAsync(dto));
    }
}
