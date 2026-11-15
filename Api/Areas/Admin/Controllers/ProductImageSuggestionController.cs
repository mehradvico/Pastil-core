using Api.Authorization;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProductSrvs.ProductImageSuggestionSrv.Dto;
using Application.Services.ProductSrvs.ProductImageSuggestionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// عکس های پیشنهادی محصولات
    /// </summary>
    /// <remarks>صف بررسی تصویرهایی که فروشگاه‌ها برای محصولات کاتالوگ فرستاده‌اند</remarks>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    // AdminOnly عمدی: تأیید، تصویر اصلی یک محصول کاتالوگ (که برای همهٔ فروشگاه‌ها و مشتری‌ها دیده می‌شود) را عوض می‌کند.
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public class ProductImageSuggestionController : ControllerBase
    {
        private readonly IProductImageSuggestionService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ProductImageSuggestionController(IProductImageSuggestionService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>فهرست پیشنهادها. status: pending (پیش‌فرض، قدیمی‌ترین اول) | approved | rejected | all</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<ProductImageSuggestionAdminListDto>), 200)]
        public async Task<IActionResult> Search([FromQuery] ProductImageSuggestionAdminSearchDto dto)
            => Ok(await _service.SearchAsync(dto));

        /// <summary>
        /// تأیید: تصویر پیشنهادی تصویر اصلی محصول می‌شود. اگر محصول از قبل تصویر دارد، بدون confirmReplace=true
        /// پاسخ isSuccess=false با code=9 برمی‌گردد (پنل مودال «مطمئنید؟» نشان می‌دهد).
        /// </summary>
        [HttpPost("approve")]
        [ProducesResponseType(typeof(BaseResultDto<ProductImageSuggestionApproveResultDto>), 200)]
        public async Task<IActionResult> Approve([FromBody] ProductImageSuggestionApproveDto dto)
            => Ok(await _service.ApproveAsync(_currentUser.CurrentUser.UserId, dto));

        /// <summary>رد با دلیل متنی (فروشنده می‌تواند عکس دیگری بفرستد)</summary>
        [HttpPost("reject")]
        [ProducesResponseType(typeof(BaseResultDto<ProductImageSuggestionAdminDto>), 200)]
        public async Task<IActionResult> Reject([FromBody] ProductImageSuggestionRejectDto dto)
            => Ok(await _service.RejectAsync(_currentUser.CurrentUser.UserId, dto));
    }
}
