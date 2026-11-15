using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProductSrvs.ProductImageSuggestionSrv.Dto;
using Application.Services.ProductSrvs.ProductImageSuggestionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// تصویر پیشنهادی فروشگاه برای محصول کاتالوگ — فروشنده هنگام ثبت قیمت/موجودی عکس خودش را هم می‌فرستد؛
    /// پس از تأیید ادمین، همان عکس تصویر اصلی محصول می‌شود. StoreId همیشه از توکن گرفته می‌شود.
    /// </summary>
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductImageSuggestionController : ControllerBase
    {
        private readonly IProductImageSuggestionService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ProductImageSuggestionController(IProductImageSuggestionService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long StoreId => _currentUser.CurrentUser.StoreId;

        /// <summary>
        /// ارسال تصویر پیشنهادی برای یک محصول (multipart: فیلد productId + فایل image؛ jpg/png/webp تا ۸MB).
        /// محصول باید در همین فروشگاه ثبت شده باشد؛ اگر از قبل پیشنهادِ در انتظارِ همین فروشگاه برای همین محصول هست، همان عوض می‌شود.
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("ProductImageSuggestion")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        [ProducesResponseType(typeof(BaseResultDto<ProductImageSuggestionDto>), 200)]
        public async Task<IActionResult> Submit([FromForm] long productId, IFormFile image)
        {
            var authorizationHeaderValue = Request.Headers.Authorization.ToString();
            return Ok(await _service.SubmitAsync(
                StoreId, _currentUser.CurrentUser.UserId, productId, image, authorizationHeaderValue, HttpContext.RequestAborted));
        }

        /// <summary>فهرست پیشنهادهای همین فروشگاه، جدیدترین اول (productId اختیاری)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<ProductImageSuggestionListDto>), 200)]
        public async Task<IActionResult> List([FromQuery] long? productId = null, [FromQuery] int pageSize = 100)
            => Ok(await _service.ListAsync(StoreId, productId, pageSize));

        /// <summary>حذف پیشنهاد (فقط در انتظار بررسی یا ردشده)</summary>
        [HttpDelete("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
            => Ok(await _service.DeleteAsync(StoreId, id));
    }
}
