using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.GroomerSrvs.GroomerReserveSrv.Dto;
using Application.Services.GroomerSrvs.GroomerReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// رزروهای آرایشگاه‌های من: لیست، تأیید/رد و تکمیل
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerReserveController : ControllerBase
    {
        private readonly IGroomerReserveService _service;
        private readonly IGroomerReserveApprovalService _approval;
        private readonly ICurrentUserHelper _currentUser;

        public GroomerReserveController(IGroomerReserveService service, IGroomerReserveApprovalService approval, ICurrentUserHelper currentUser)
        {
            _service = service;
            _approval = approval;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        /// <summary>فقط رزروهای پرداخت‌شده‌ی آرایشگاه‌های من</summary>
        [HttpGet]
        [ProducesResponseType(typeof(GroomerReservePage), 200)]
        public IActionResult Get([FromQuery] GroomerReserveSearchInputDto dto)
        {
            dto.OwnerUserId = UserId;
            dto.BookerId = null;
            dto.ProviderId = null;
            dto.OnlyPaid = true;
            return Ok(_service.Search(dto));
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<GroomerReserveVDto>), 200)]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindAsync(id, null, UserId));

        [HttpPut("Approve")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Approve(long id) => Ok(await _approval.ApproveAsync(id, UserId));

        /// <summary>رد رزرو (دلیل الزامی)؛ کل مبلغ به کیف پول مشتری برمی‌گردد</summary>
        [HttpPut("Reject")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Reject(GroomerReserveReasonDto dto) => Ok(await _approval.RejectAsync(dto.Id, dto.Reason, UserId));

        /// <summary>تغییر زمان نوبت (به مشتری پوش و پیامک می‌رود)؛ فقط روی زمان آزاد آرایشگاه</summary>
        [HttpPut("Reschedule")]
        [ProducesResponseType(typeof(BaseResultDto<GroomerReserveVDto>), 200)]
        public async Task<IActionResult> Reschedule(GroomerReserveRescheduleDto dto) => Ok(await _service.RescheduleAsync(dto.Id, dto.NewDoDate, UserId));

        /// <summary>تکمیل رزرو بعد از انجام خدمت</summary>
        [HttpPut("Complete")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Complete(long id) => Ok(await _service.CompleteAsync(id, UserId));
    }
}
