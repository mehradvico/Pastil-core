using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CoachSrvs.CoachReserveSrv.Dto;
using Application.Services.CoachSrvs.CoachReserveSrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Application.Services.Order.PaymentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// پرداخت رزرو مربی، کد تخفیف و کیف پول
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachReservePaymentController : ControllerBase
    {
        private readonly ICoachReserveService _service;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IPaymentService _paymentService;

        public CoachReservePaymentController(ICoachReserveService service, ICurrentUserHelper currentUser, IPaymentService paymentService)
        {
            _service = service;
            _currentUser = currentUser;
            _paymentService = paymentService;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        /// <summary>شروع پرداخت رزرو (CoachReserveId در بدنه)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PaymentStartDto>), 200)]
        public async Task<IActionResult> Post(PaymentStartDto payment)
        {
            payment.UserId = UserId;
            payment.User = new Application.Services.Dto.UserMinVDto { Email = _currentUser.CurrentUser.Email, Mobile = _currentUser.CurrentUser.Mobile, Id = UserId };
            return Ok(await _paymentService.InsertCoachReservePaymentAsyncDto(payment));
        }

        /// <summary>اعمال کد تخفیف روی مبلغ آنلاین رزرو</summary>
        [HttpPut("Rebate")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> SetRebate(CoachReserveRebateCodeDto dto) => Ok(await _service.SetRebateCodeAsync(dto.Id, dto.RebateCode, UserId));

        [HttpDelete("Rebate")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> RemoveRebate(long id) => Ok(await _service.ClearRebateAsync(id, UserId));

        /// <summary>پرداخت از کیف پول (کل یا بخشی از مبلغ)</summary>
        [HttpPut("Wallet")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> SetWallet(CoachReserveWalletDto dto) => Ok(await _service.SetWalletAsync(dto.Id, dto.FromWallet, UserId));
    }
}
