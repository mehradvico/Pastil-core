using Application.Common.Dto.Result;
using Application.Services.ConsultationSrvs.ConsultationAdminSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationAdminSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// خریدهای مشاوره آنلاین
    /// </summary>
    /// <remarks>فقط خواندن؛ دسترسی با RolePermission ناحیه‌ی Admin</remarks>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsultationPurchaseController : ControllerBase
    {
        private readonly IConsultationAdminService _service;

        public ConsultationPurchaseController(IConsultationAdminService service)
        {
            _service = service;
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
    }
}
