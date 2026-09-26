using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PrescriptionSrv.Dto;
using Application.Services.PrescriptionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// نسخه‌ی پزشک برای کاربر (فقط خواندنِ نسخه‌ی رزرو یا مشاوره‌ی خودش)
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class OnlinePrescriptionController : ControllerBase
    {
        private readonly IOnlinePrescriptionService _service;
        private readonly ICurrentUserHelper _currentUser;

        public OnlinePrescriptionController(IOnlinePrescriptionService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<PrescriptionVDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] PrescriptionTargetDto target)
            => Ok(await _service.GetForBookerAsync(_currentUser.CurrentUser.UserId, target));
    }
}
