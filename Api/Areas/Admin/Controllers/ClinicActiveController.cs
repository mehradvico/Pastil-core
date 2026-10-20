using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicSrv.Dto;
using Application.Services.ClinicSrvs.ClinicSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// فعال/غیرفعال کردن کلینیک
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicActiveController : ControllerBase
    {
        private readonly IClinicCoreService _service;
        public ClinicActiveController(IClinicCoreService service) => _service = service;

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(ClinicActiveDto dto) => Ok(await _service.UpdateActiveAsync(dto, ownerUserId: null));
    }
}
