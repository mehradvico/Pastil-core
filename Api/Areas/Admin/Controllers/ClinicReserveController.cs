using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicReserveSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// رزروهای کلینیک (ادمین)
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicReserveController : ControllerBase
    {
        private readonly IClinicReserveService _service;
        private readonly IClinicReserveApprovalService _approval;

        public ClinicReserveController(IClinicReserveService service, IClinicReserveApprovalService approval)
        {
            _service = service;
            _approval = approval;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ClinicReservePage), 200)]
        public IActionResult Get([FromQuery] ClinicReserveSearchInputDto dto)
        {
            dto.OwnerUserId = null;
            return Ok(_service.Search(dto));
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicReserveVDto>), 200)]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindAsync(id, null, null));

        [HttpPut("Approve")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Approve(long id) => Ok(await _approval.ApproveAsync(id, null));

        [HttpPut("Reject")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Reject(ClinicReserveReasonDto dto) => Ok(await _approval.RejectAsync(dto.Id, dto.Reason, null));

        [HttpPut("Complete")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Complete(long id) => Ok(await _service.CompleteAsync(id, null));

        /// <summary>تغییر زمان نوبت (به کاربر پوش و پیامک می‌رود)</summary>
        [HttpPut("Reschedule")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicReserveVDto>), 200)]
        public async Task<IActionResult> Reschedule(ClinicReserveRescheduleDto dto) => Ok(await _service.RescheduleAsync(dto.Id, dto.NewDoDate, null));

        /// <summary>لغو توسط ادمین؛ refund=true کل مبلغ پرداخت‌شده را به کیف پول کاربر برمی‌گرداند</summary>
        [HttpPut("Cancel")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Cancel(ClinicReserveReasonDto dto, bool refund = true) =>
            Ok(await _approval.CancelByAdminAsync(dto.Id, dto.Reason, refund));
    }
}
