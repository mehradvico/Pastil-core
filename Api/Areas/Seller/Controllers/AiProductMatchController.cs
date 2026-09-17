using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProductSrvs.AiProductMatchSrv.Dto;
using Application.Services.ProductSrvs.AiProductMatchSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// تطبیق هوشمند محصولات با کمک هوش مصنوعی (Gemini) برای درج سریع موجودی فروشگاه
    /// </summary>
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class AiProductMatchController : ControllerBase
    {
        private readonly IAiProductMatchService _aiProductMatchService;
        private readonly ICurrentUserHelper _currentUser;

        public AiProductMatchController(IAiProductMatchService aiProductMatchService, ICurrentUserHelper currentUser)
        {
            _aiProductMatchService = aiProductMatchService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// وضعیت در دسترس بودن سرویس هوش مصنوعی — بدون افشای هیچ بخشی از کلید
        /// </summary>
        [HttpGet("status")]
        [ProducesResponseType(typeof(BaseResultDto<AiProductMatchStatusDto>), 200)]
        public async Task<IActionResult> Status()
        {
            var result = await _aiProductMatchService.GetStatusAsync();
            return Ok(result);
        }

        /// <summary>
        /// تحلیل تصویر قفسه، اسکرین‌شات جدول نرم‌افزار انبار (سپیدار/دامپزشکیار)، یا داده‌ی جدول (Excel/سپیدار/دامپزشکیار) و تطبیق با کاتالوگ پاستیل
        /// </summary>
        [HttpPost("analyze")]
        // تا ۴۰ اسکرین‌شات جدول (سپیدار/دامپزشکیار) پوشش داده می‌شود، نه فقط ۸ عکس قفسه؛ سقف واقعی هر
        // تصویر همچنان با AiProductMatchOptions.MaxImageSizeBytes کنترل می‌شود.
        [RequestSizeLimit(128 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 128 * 1024 * 1024)]
        [ProducesResponseType(typeof(BaseResultDto<AiProductMatchAnalyzeResultDto>), 200)]
        public async Task<IActionResult> Analyze([FromForm] AiProductMatchAnalyzeInputDto dto)
        {
            var storeId = _currentUser.CurrentUser.StoreId;
            var authorizationHeaderValue = Request.Headers.Authorization.ToString();
            var result = await _aiProductMatchService.AnalyzeAsync(storeId, dto, authorizationHeaderValue, HttpContext.RequestAborted);
            return Ok(result);
        }
    }
}
