using Application.Common.Dto.Result;
using Application.Services.CoachSrvs.CoachSrv.Dto;
using Application.Services.CoachSrvs.CoachSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تأیید/رد مربی (رد نیاز به دلیل دارد)
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachApproveController : ControllerBase
    {
        private readonly ICoachCoreService _service;
        public CoachApproveController(ICoachCoreService service) => _service = service;

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CoachApproveDto dto) => Ok(await _service.UpdateApproveAsync(dto));
    }
}
