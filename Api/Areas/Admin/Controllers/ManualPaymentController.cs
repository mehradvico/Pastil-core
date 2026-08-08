using Application.Common.Dto.Result;
using Application.Services.Order.PaymentSrv.Dto;
using Application.Services.Order.PaymentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت پرداخت‌های دستی
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ManualPaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        /// <summary>
        /// مدیریت پرداخت‌های دستی
        /// </summary>
        public ManualPaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// ثبت پرداخت دستی و اعمال نتیجه آن روی آیتم مربوطه
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ManualPaymentVDto>), 200)]
        public async Task<IActionResult> Post(ManualPaymentDto dto)
        {
            var result = await _paymentService.InsertManualPaymentAsync(dto);
            return Ok(result);
        }
    }
}
