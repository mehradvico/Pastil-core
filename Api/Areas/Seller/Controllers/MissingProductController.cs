using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProductSrvs.MissingProductSrv.Dto;
using Application.Services.ProductSrvs.MissingProductSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// محصولات ثبت‌نشده در کاتالوگ — فروشنده محصولی را که در کاتالوگ پاستیل نیست پیشنهاد می‌دهد و پس از
    /// تأیید ادمین، هم در کاتالوگ ساخته می‌شود و هم به همین فروشگاه اضافه می‌شود. StoreId همیشه از توکن گرفته می‌شود.
    /// </summary>
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class MissingProductController : ControllerBase
    {
        private readonly IMissingProductService _service;
        private readonly ICurrentUserHelper _currentUser;

        public MissingProductController(IMissingProductService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long StoreId => _currentUser.CurrentUser.StoreId;

        /// <summary>ثبت دسته‌ای (از اسکن قفسه) — تکراری‌ها (نام نرمال‌شده در همین فروشگاه) ساخته نمی‌شوند</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<MissingProductBatchResultDto>), 200)]
        public async Task<IActionResult> Create([FromBody] MissingProductBatchInputDto dto)
            => Ok(await _service.CreateBatchAsync(StoreId, dto));

        /// <summary>فهرست محصولات ثبت‌نشدهٔ همین فروشگاه، جدیدترین اول</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<MissingProductListDto>), 200)]
        public async Task<IActionResult> List([FromQuery] int pageSize = 200)
            => Ok(await _service.ListAsync(StoreId, pageSize));

        /// <summary>ویرایش (فقط draft/rejected؛ ویرایش rejected آن را به draft برمی‌گرداند)</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<MissingProductDto>), 200)]
        public async Task<IActionResult> Update([FromBody] MissingProductUpdateDto dto)
            => Ok(await _service.UpdateAsync(StoreId, dto));

        /// <summary>افزودن یک تصویر (فیلد multipart: image؛ jpg/png/webp تا ۸MB؛ حداکثر ۵ تصویر برای هر درخواست)</summary>
        [HttpPost("{id:long}/picture")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        [ProducesResponseType(typeof(BaseResultDto<MissingProductDto>), 200)]
        public async Task<IActionResult> AddPicture(long id, IFormFile image)
        {
            var authorizationHeaderValue = Request.Headers.Authorization.ToString();
            return Ok(await _service.AddPictureAsync(StoreId, id, image, authorizationHeaderValue, HttpContext.RequestAborted));
        }

        /// <summary>حذف یک تصویر از درخواست (فقط draft/rejected)</summary>
        [HttpDelete("{id:long}/picture/{pictureId:long}")]
        [ProducesResponseType(typeof(BaseResultDto<MissingProductDto>), 200)]
        public async Task<IActionResult> RemovePicture(long id, long pictureId)
            => Ok(await _service.RemovePictureAsync(StoreId, id, pictureId));

        /// <summary>ارسال برای بررسی ادمین (فقط از draft؛ نام و تصویر لازم است)</summary>
        [HttpPost("{id:long}/submit")]
        [ProducesResponseType(typeof(BaseResultDto<MissingProductDto>), 200)]
        public async Task<IActionResult> Submit(long id)
            => Ok(await _service.SubmitAsync(StoreId, id));

        /// <summary>حذف (فقط draft/rejected)</summary>
        [HttpDelete("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
            => Ok(await _service.DeleteAsync(StoreId, id));
    }
}
