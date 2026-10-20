using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ClinicSrvs.ClinicReserveSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveSrv.Iface;
using Application.Services.Order.PaymentSrv.Dto;
using Application.Services.Order.PaymentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// پرداخت رزرو کلینیک، کد تخفیف و کیف پول
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicReservePaymentController : ControllerBase
    {
        private readonly IClinicReserveService _service;
        private readonly ICurrentUserHelper _currentUser;
        private readonly IPaymentService _paymentService;

        public ClinicReservePaymentController(IClinicReserveService service, ICurrentUserHelper currentUser, IPaymentService paymentService)
        {
            _service = service;
            _currentUser = currentUser;
            _paymentService = paymentService;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        /// <summary>شروع پرداخت رزرو (ClinicReserveId در بدنه)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PaymentStartDto>), 200)]
        public async Task<IActionResult> Post(PaymentStartDto payment)
        {
            payment.UserId = UserId;
            payment.User = new Application.Services.Dto.UserMinVDto { Email = _currentUser.CurrentUser.Email, Mobile = _currentUser.CurrentUser.Mobile, Id = UserId };
            return Ok(await _paymentService.InsertClinicReservePaymentAsyncDto(payment));
        }

        /// <summary>اعمال کد تخفیف روی مبلغ آنلاین رزرو</summary>
        [HttpPut("Rebate")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> SetRebate(ClinicReserveRebateCodeDto dto) => Ok(await _service.SetRebateCodeAsync(dto.Id, dto.RebateCode, UserId));

        [HttpDelete("Rebate")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> RemoveRebate(long id) => Ok(await _service.ClearRebateAsync(id, UserId));

        /// <summary>پرداخت از کیف پول (کل یا بخشی از مبلغ)</summary>
        [HttpPut("Wallet")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> SetWallet(ClinicReserveWalletDto dto) => Ok(await _service.SetWalletAsync(dto.Id, dto.FromWallet, UserId));
    }
}
