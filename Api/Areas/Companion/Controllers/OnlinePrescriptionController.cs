using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PrescriptionSrv.Dto;
using Application.Services.PrescriptionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// نسخه‌ی پزشک برای رزرو آنلاین / مشاوره (نماینده و همکار)
    /// </summary>
    [Area("Companion")]
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

        /// <summary>خواندن نسخه‌ی یک رزرو/مشاوره (data=null یعنی هنوز ثبت نشده). دقیقاً یکی از companionReserveId، consultationPurchaseId، onlineSessionId</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<PrescriptionVDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] PrescriptionTargetDto target)
            => Ok(await _service.GetForCompanionAsync(_currentUser.CurrentUser.UserId, target));

        /// <summary>ثبت یا ویرایش نسخه (متن حداکثر ۴۰۰۰ کاراکتر، حداکثر ۱۰ تصویر)</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<PrescriptionVDto>), 200)]
        public async Task<IActionResult> Put(PrescriptionUpsertDto dto)
            => Ok(await _service.UpsertAsync(_currentUser.CurrentUser.UserId, dto));
    }
}
