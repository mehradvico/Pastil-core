using Application.Common.Dto.Result;
using Application.Services.CoachSrvs.CoachReserveSrv.Dto;
using Application.Services.CoachSrvs.CoachReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// رزروهای مربی (ادمین)
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachReserveController : ControllerBase
    {
        private readonly ICoachReserveService _service;
        private readonly ICoachReserveApprovalService _approval;

        public CoachReserveController(ICoachReserveService service, ICoachReserveApprovalService approval)
        {
            _service = service;
            _approval = approval;
        }

        [HttpGet]
        [ProducesResponseType(typeof(CoachReservePage), 200)]
        public IActionResult Get([FromQuery] CoachReserveSearchInputDto dto)
        {
            dto.OwnerUserId = null;
            return Ok(_service.Search(dto));
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<CoachReserveVDto>), 200)]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindAsync(id, null, null));

        [HttpPut("Approve")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Approve(long id) => Ok(await _approval.ApproveAsync(id, null));

        [HttpPut("Reject")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Reject(CoachReserveReasonDto dto) => Ok(await _approval.RejectAsync(dto.Id, dto.Reason, null));

        [HttpPut("Complete")]
        [ProducesResponseType(typeof(BaseResultDto<CoachReserveVDto>), 200)]
        public async Task<IActionResult> Complete(long id) => Ok(await _service.CompleteAsync(id, null));

        /// <summary>تغییر زمان نوبت (به کاربر پوش و پیامک می‌رود)</summary>
        [HttpPut("Reschedule")]
        [ProducesResponseType(typeof(BaseResultDto<CoachReserveVDto>), 200)]
        public async Task<IActionResult> Reschedule(CoachReserveRescheduleDto dto) => Ok(await _service.RescheduleAsync(dto.Id, dto.NewDoDate, null));

        /// <summary>لغو توسط ادمین؛ refund=true کل مبلغ پرداخت‌شده را به کیف پول کاربر برمی‌گرداند</summary>
        [HttpPut("Cancel")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Cancel(CoachReserveReasonDto dto, bool refund = true) =>
            Ok(await _approval.CancelByAdminAsync(dto.Id, dto.Reason, refund));
    }
}
