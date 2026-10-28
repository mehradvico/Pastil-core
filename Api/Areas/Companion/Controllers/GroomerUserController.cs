using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Dto;
using Application.Services.GroomerSrvs.GroomerProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// پرسنل آرایشگاه‌های من
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerUserController : ControllerBase
    {
        private readonly IGroomerProfileService _service;
        private readonly ICurrentUserHelper _currentUser;

        public GroomerUserController(IGroomerProfileService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<GroomerStaffVDto>>), 200)]
        public async Task<IActionResult> Get(long groomerId) => Ok(await _service.ListStaffAsync(groomerId, UserId));

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<GroomerStaffVDto>), 200)]
        public async Task<IActionResult> Post(GroomerStaffAddDto dto) => Ok(await _service.AddStaffAsync(dto, UserId));

        [HttpPut("Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Active(long id, bool active) => Ok(await _service.SetStaffActiveAsync(id, active, UserId));

        /// <summary>
        /// نقش/تخصص‌های یک عضو تیم (شناسه‌ها از GET api/Expertise?ProviderRole=Groomer؛ حداکثر ۱۰، لیست خالی = پاک کردن). فقط مالک/مدیر مرکز.
        /// </summary>
        [HttpPut("Expertise")]
        [ProducesResponseType(typeof(BaseResultDto<GroomerStaffVDto>), 200)]
        public async Task<IActionResult> Expertise(GroomerStaffExpertiseDto dto) => Ok(await _service.SetStaffExpertiseAsync(dto, UserId));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.RemoveStaffAsync(id, UserId));
    }
}
