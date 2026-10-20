using Application.Common.Dto.Result;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Dto;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پرسنل آرایشگاه
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerUserController : ControllerBase
    {
        private readonly IGroomerProfileService _service;
        public GroomerUserController(IGroomerProfileService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<GroomerStaffVDto>>), 200)]
        public async Task<IActionResult> Get(long groomerId) => Ok(await _service.ListStaffAsync(groomerId, ownerUserId: null));

        /// <summary>افزودن پرسنل با شماره موبایل (کاربر باید قبلاً ثبت‌نام کرده باشد)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<GroomerStaffVDto>), 200)]
        public async Task<IActionResult> Post(GroomerStaffAddDto dto) => Ok(await _service.AddStaffAsync(dto, ownerUserId: null));

        [HttpPut("Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Active(long id, bool active) => Ok(await _service.SetStaffActiveAsync(id, active, ownerUserId: null));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.RemoveStaffAsync(id, ownerUserId: null));
    }
}
