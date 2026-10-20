using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicProfileSrv.Dto;
using Application.Services.ClinicSrvs.ClinicProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پرسنل کلینیک
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicUserController : ControllerBase
    {
        private readonly IClinicProfileService _service;
        public ClinicUserController(IClinicProfileService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ClinicStaffVDto>>), 200)]
        public async Task<IActionResult> Get(long clinicId) => Ok(await _service.ListStaffAsync(clinicId, ownerUserId: null));

        /// <summary>افزودن پرسنل با شماره موبایل (کاربر باید قبلاً ثبت‌نام کرده باشد)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClinicStaffVDto>), 200)]
        public async Task<IActionResult> Post(ClinicStaffAddDto dto) => Ok(await _service.AddStaffAsync(dto, ownerUserId: null));

        [HttpPut("Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Active(long id, bool active) => Ok(await _service.SetStaffActiveAsync(id, active, ownerUserId: null));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.RemoveStaffAsync(id, ownerUserId: null));
    }
}
