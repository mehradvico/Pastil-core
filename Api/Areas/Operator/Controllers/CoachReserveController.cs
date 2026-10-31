using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CoachSrvs.CoachReserveSrv.Dto;
using Application.Services.CoachSrvs.CoachReserveSrv.Iface;
using Application.Services.ReserveCompleteSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.Areas.Operator.Controllers
{
    /// <summary>
    /// پنل اپراتور: رزروهای مربی که به من (پرسنل) سپرده شده
    /// </summary>
    [Area("Operator")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachReserveController : ControllerBase
    {
        private readonly ICoachReserveService _service;
        private readonly ICurrentUserHelper _currentUser;

        public CoachReserveController(ICoachReserveService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        /// <summary>فقط رزروهای پرداخت‌شده‌ای که به من سپرده شده (تا وقتی عضو فعال مرکز هستم)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(CoachReservePage), 200)]
        public IActionResult Get([FromQuery] CoachReserveSearchInputDto dto)
        {
            dto.AssigneeUserId = UserId;
            dto.OwnerUserId = null;
            dto.BookerId = null;
            dto.ProviderId = null;
            dto.Unassigned = null;
            dto.OnlyPaid = true;
            var page = _service.Search(dto);
            foreach (var item in page.List)
            {
                item.ProviderShare = 0;
                item.SiteShare = 0;
                item.CommissionPercent = 0;
            }
            return Ok(page);
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<CoachReserveVDto>), 200)]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindForAssigneeAsync(id, UserId));

        /// <summary>تکمیل رزرو بعد از انجام خدمت (همان قواعد مالک؛ فقط پرسنل همین رزرو)</summary>
        /// <remarks>بدنه اختیاری: finalAmount، receivedAtVenue، notReceived، detail. طراحی: backend/Docs/RESERVE_COMPLETE_FINAL_AMOUNT_NEMAYANDE_APP_FA.md</remarks>
        [HttpPut("Complete")]
        [ProducesResponseType(typeof(BaseResultDto<CoachReserveVDto>), 200)]
        public async Task<IActionResult> Complete(long id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] ReserveCompleteDto dto = null)
            => Ok(await _service.CompleteAsync(id, UserId, dto));
    }
}
