using Application.Common.Dto.Result;
using Application.Services.ConsultationSrvs.ConsultationAdminSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationAdminSrv.Iface;
using Application.Services.ConsultationSrvs.ConsultationPurchaseSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// خریدهای مشاوره آنلاین
    /// </summary>
    /// <remarks>خواندن + لغو/تکمیل دستی؛ دسترسی با RolePermission ناحیه‌ی Admin</remarks>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsultationPurchaseController : ControllerBase
    {
        private readonly IConsultationAdminService _service;
        private readonly IConsultationPurchaseService _purchaseService;

        public ConsultationPurchaseController(IConsultationAdminService service, IConsultationPurchaseService purchaseService)
        {
            _service = service;
            _purchaseService = purchaseService;
        }

        /// <summary>جستجوی خریدها (کلینیک، کاربر، وضعیت، روش، بازه‌ی تاریخ، متن)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationPurchaseAdminSearchDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] ConsultationPurchaseAdminInputDto dto)
            => Ok(await _service.SearchPurchasesAsync(dto));

        /// <summary>خلاصه‌ی مالی و مدت برای همان فیلترهای جستجو: تعداد به تفکیک وضعیت، فروش، تخفیف، بازپرداخت، سهم‌ها و وضعیت تسویه، مدت خریداری‌شده و مدت واقعی تماس</summary>
        [HttpGet("Summary")]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationPurchaseAdminSummaryDto>), 200)]
        public async Task<IActionResult> Summary([FromQuery] ConsultationPurchaseAdminInputDto dto)
            => Ok(await _service.GetPurchaseSummaryAsync(dto));

        /// <summary>جزئیات یک خرید</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationPurchaseAdminVDto>), 200)]
        public async Task<IActionResult> Get(long id)
            => Ok(await _service.GetPurchaseAsync(id));

        /// <summary>لغو دستی توسط ادمین + بازپرداخت کامل به کیف پول کاربر (فقط پرداخت‌شده یا در جریان)</summary>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Cancel(long id, [FromBody] ConsultationAdminCancelDto dto)
            => Ok(await _purchaseService.AdminCancelAsync(id, dto?.Reason));

        /// <summary>تکمیل دستی مشاوره‌ی در جریان توسط ادمین</summary>
        [HttpPost("{id}/complete")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Complete(long id)
            => Ok(await _purchaseService.AdminCompleteAsync(id));
    }
}
