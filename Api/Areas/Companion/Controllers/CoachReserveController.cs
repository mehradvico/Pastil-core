using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CoachSrvs.CoachReserveSrv.Dto;
using Application.Services.CoachSrvs.CoachReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// رزروهای مربی‌های من: لیست، تأیید/رد و تکمیل
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachReserveController : ControllerBase
    {
        private readonly ICoachReserveService _service;
        private readonly ICoachReserveApprovalService _approval;
        private readonly ICurrentUserHelper _currentUser;

        public CoachReserveController(ICoachReserveService service, ICoachReserveApprovalService approval, ICurrentUserHelper currentUser)
        {
            _service = service;
            _approval = approval;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        /// <summary>فقط رزروهای پرداخت‌شده‌ی مربی‌های من</summary>
        [HttpGet]
        [ProducesResponseType(typeof(CoachReservePage), 200)]
        public IActionResult Get([FromQuery] CoachReserveSearchInputDto dto)
        {
            dto.OwnerUserId = UserId;
            dto.BookerId = null;
            dto.ProviderId = null;
            dto.OnlyPaid = true;
            return Ok(_service.Search(dto));
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<CoachReserveVDto>), 200)]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindAsync(id, null, UserId));

        [HttpPut("Approve")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Approve(long id) => Ok(await _approval.ApproveAsync(id, UserId));

        /// <summary>رد رزرو (دلیل الزامی)؛ کل مبلغ به کیف پول مشتری برمی‌گردد</summary>
        [HttpPut("Reject")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Reject(CoachReserveReasonDto dto) => Ok(await _approval.RejectAsync(dto.Id, dto.Reason, UserId));

        /// <summary>تغییر زمان نوبت (به مشتری پوش و پیامک می‌رود)؛ فقط روی زمان آزاد مربی</summary>
        [HttpPut("Reschedule")]
        [ProducesResponseType(typeof(BaseResultDto<CoachReserveVDto>), 200)]
        public async Task<IActionResult> Reschedule(CoachReserveRescheduleDto dto) => Ok(await _service.RescheduleAsync(dto.Id, dto.NewDoDate, UserId));

        /// <summary>تکمیل رزرو بعد از انجام خدمت</summary>
        [HttpPut("Complete")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Complete(long id) => Ok(await _service.CompleteAsync(id, UserId));
    }
}
