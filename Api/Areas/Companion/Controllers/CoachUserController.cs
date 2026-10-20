using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CoachSrvs.CoachProfileSrv.Dto;
using Application.Services.CoachSrvs.CoachProfileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// پرسنل مربی‌های من
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachUserController : ControllerBase
    {
        private readonly ICoachProfileService _service;
        private readonly ICurrentUserHelper _currentUser;

        public CoachUserController(ICoachProfileService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<CoachStaffVDto>>), 200)]
        public async Task<IActionResult> Get(long coachId) => Ok(await _service.ListStaffAsync(coachId, UserId));

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CoachStaffVDto>), 200)]
        public async Task<IActionResult> Post(CoachStaffAddDto dto) => Ok(await _service.AddStaffAsync(dto, UserId));

        [HttpPut("Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Active(long id, bool active) => Ok(await _service.SetStaffActiveAsync(id, active, UserId));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.RemoveStaffAsync(id, UserId));
    }
}
