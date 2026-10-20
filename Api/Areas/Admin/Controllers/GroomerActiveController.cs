using Application.Common.Dto.Result;
using Application.Services.GroomerSrvs.GroomerSrv.Dto;
using Application.Services.GroomerSrvs.GroomerSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// فعال/غیرفعال کردن آرایشگاه
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerActiveController : ControllerBase
    {
        private readonly IGroomerCoreService _service;
        public GroomerActiveController(IGroomerCoreService service) => _service = service;

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(GroomerActiveDto dto) => Ok(await _service.UpdateActiveAsync(dto, ownerUserId: null));
    }
}
