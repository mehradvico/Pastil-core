using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ClinicSrvs.ClinicReserveSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// رزروهای کلینیک‌های من: لیست، تأیید/رد و تکمیل
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicReserveController : ControllerBase
    {
        private readonly IClinicReserveService _service;
        private readonly IClinicReserveApprovalService _approval;
        private readonly ICurrentUserHelper _currentUser;

        public ClinicReserveController(IClinicReserveService service, IClinicReserveApprovalService approval, ICurrentUserHelper currentUser)
        {
            _service = service;
            _approval = approval;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        /// <summary>فقط رزروهای پرداخت‌شده‌ی کلینیک‌های من</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ClinicReservePage), 200)]
        public IActionResult Get([FromQuery] ClinicReserveSearchInputDto dto)
        {
            dto.OwnerUserId = UserId;
            dto.BookerId = null;
            dto.ProviderId = null;
            dto.OnlyPaid = true;
            return Ok(_service.Search(dto));
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicReserveVDto>), 200)]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindAsync(id, null, UserId));

        [HttpPut("Approve")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Approve(long id) => Ok(await _approval.ApproveAsync(id, UserId));

        /// <summary>رد رزرو (دلیل الزامی)؛ کل مبلغ به کیف پول مشتری برمی‌گردد</summary>
        [HttpPut("Reject")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Reject(ClinicReserveReasonDto dto) => Ok(await _approval.RejectAsync(dto.Id, dto.Reason, UserId));

        /// <summary>تغییر زمان نوبت (به مشتری پوش و پیامک می‌رود)؛ فقط روی زمان آزاد کلینیک</summary>
        [HttpPut("Reschedule")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicReserveVDto>), 200)]
        public async Task<IActionResult> Reschedule(ClinicReserveRescheduleDto dto) => Ok(await _service.RescheduleAsync(dto.Id, dto.NewDoDate, UserId));

        /// <summary>تکمیل رزرو بعد از انجام خدمت</summary>
        [HttpPut("Complete")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Complete(long id) => Ok(await _service.CompleteAsync(id, UserId));
    }
}
