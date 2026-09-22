using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProductSrvs.ProductImageEnhanceSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.Tasks;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// پردازش تصویر محصول قبل از آپلود — فعلاً فقط سفیدسازی پس‌زمینه با هوش مصنوعی
    /// </summary>
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductImageController : ControllerBase
    {
        private readonly IProductImageEnhanceService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ProductImageController(IProductImageEnhanceService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>
        /// تصویر خام محصول را به تصویر کاتالوگی (پس‌زمینه‌ی سفید، بوم مربع، حاشیه‌ی یکسان) تبدیل می‌کند.
        /// پاسخ موفق بدنه‌ی خودِ تصویر است (image/png)، نه JSON. هیچ تصویری ذخیره نمی‌شود؛
        /// آپلود نهایی همان مسیر همیشگی MissingProduct/{id}/picture است.
        /// </summary>
        [HttpPost("Enhance")]
        [EnableRateLimiting("AiProductMatch")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Enhance(IFormFile image, [FromForm] string background = "white")
        {
            var currentUser = _currentUser.CurrentUser;
            if (currentUser is null || currentUser.StoreId <= 0)
                return Forbid();

            var result = await _service.EnhanceAsync(image, HttpContext.RequestAborted);

            return result.Status switch
            {
                ProductImageEnhanceStatus.Success => File(result.Content, result.ContentType),
                ProductImageEnhanceStatus.TooLarge => StatusCode(StatusCodes.Status413PayloadTooLarge, new BaseResultDto(false, result.Message, 1)),
                ProductImageEnhanceStatus.InvalidImage => BadRequest(new BaseResultDto(false, result.Message, 1)),
                ProductImageEnhanceStatus.SubjectNotFound => StatusCode(StatusCodes.Status422UnprocessableEntity, new BaseResultDto(false, result.Message, 3)),
                _ => StatusCode(StatusCodes.Status503ServiceUnavailable, new BaseResultDto(false, result.Message, 5))
            };
        }
    }
}
