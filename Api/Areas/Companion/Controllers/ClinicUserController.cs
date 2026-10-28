using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ClinicSrvs.ClinicProfileSrv.Dto;
using Application.Services.ClinicSrvs.ClinicProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// پرسنل کلینیک‌های من
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicUserController : ControllerBase
    {
        private readonly IClinicProfileService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ClinicUserController(IClinicProfileService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ClinicStaffVDto>>), 200)]
        public async Task<IActionResult> Get(long clinicId) => Ok(await _service.ListStaffAsync(clinicId, UserId));

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClinicStaffVDto>), 200)]
        public async Task<IActionResult> Post(ClinicStaffAddDto dto) => Ok(await _service.AddStaffAsync(dto, UserId));

        [HttpPut("Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Active(long id, bool active) => Ok(await _service.SetStaffActiveAsync(id, active, UserId));

        /// <summary>
        /// نقش/تخصص‌های یک عضو تیم (شناسه‌ها از GET api/Expertise?ProviderRole=Clinic؛ حداکثر ۱۰، لیست خالی = پاک کردن). فقط مالک/مدیر مرکز.
        /// </summary>
        [HttpPut("Expertise")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicStaffVDto>), 200)]
        public async Task<IActionResult> Expertise(ClinicStaffExpertiseDto dto) => Ok(await _service.SetStaffExpertiseAsync(dto, UserId));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.RemoveStaffAsync(id, UserId));
    }
}
