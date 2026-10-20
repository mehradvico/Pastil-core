using Application.Common.Dto.Result;
using Application.Services.CoachSrvs.CoachProfileSrv.Dto;
using Application.Services.CoachSrvs.CoachProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پرسنل مربی
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachUserController : ControllerBase
    {
        private readonly ICoachProfileService _service;
        public CoachUserController(ICoachProfileService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<CoachStaffVDto>>), 200)]
        public async Task<IActionResult> Get(long coachId) => Ok(await _service.ListStaffAsync(coachId, ownerUserId: null));

        /// <summary>افزودن پرسنل با شماره موبایل (کاربر باید قبلاً ثبت‌نام کرده باشد)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CoachStaffVDto>), 200)]
        public async Task<IActionResult> Post(CoachStaffAddDto dto) => Ok(await _service.AddStaffAsync(dto, ownerUserId: null));

        [HttpPut("Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Active(long id, bool active) => Ok(await _service.SetStaffActiveAsync(id, active, ownerUserId: null));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.RemoveStaffAsync(id, ownerUserId: null));
    }
}
