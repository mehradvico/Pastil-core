using Application.Common.Dto.Result;
using Application.Services.CoachSrvs.CoachSrv.Dto;
using Application.Services.CoachSrvs.CoachSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// فعال/غیرفعال کردن مربی
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachActiveController : ControllerBase
    {
        private readonly ICoachCoreService _service;
        public CoachActiveController(ICoachCoreService service) => _service = service;

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CoachActiveDto dto) => Ok(await _service.UpdateActiveAsync(dto, ownerUserId: null));
    }
}
