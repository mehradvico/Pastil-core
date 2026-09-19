using Api.Authorization;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProductSrvs.MissingProductSrv.Dto;
using Application.Services.ProductSrvs.MissingProductSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// صف «درخواست افزودن به کاتالوگ» — بررسی محصولاتی که فروشنده‌ها در کاتالوگ پیدا نکرده‌اند
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    // AdminOnly عمدی: تأیید یک درخواست محصول واقعی در کاتالوگ می‌سازد و نباید برای هر کاربر لاگین‌شده (مثل فروشنده) باز باشد.
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public class MissingProductController : ControllerBase
    {
        private readonly IMissingProductService _service;
        private readonly ICurrentUserHelper _currentUser;

        public MissingProductController(IMissingProductService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>فهرست درخواست‌ها. status خالی = همه به‌جز پیش‌نویس‌ها؛ status=submitted قدیمی‌ترین اول</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<MissingProductAdminListDto>), 200)]
        public async Task<IActionResult> Search([FromQuery] MissingProductAdminSearchDto dto)
            => Ok(await _service.SearchAsync(dto));

        /// <summary>جزئیات یک درخواست همراه «محصولات مشابه در کاتالوگ»</summary>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<MissingProductAdminDto>), 200)]
        public async Task<IActionResult> Get(long id)
            => Ok(await _service.GetAsync(id, HttpContext.RequestAborted));

        /// <summary>
        /// تأیید: محصول واقعی در کاتالوگ ساخته می‌شود و اگر فروشنده قیمت داده بود، ProductItem همان فروشگاه هم ساخته می‌شود
        /// </summary>
        [HttpPost("approve")]
        [ProducesResponseType(typeof(BaseResultDto<MissingProductApproveResultDto>), 200)]
        public async Task<IActionResult> Approve([FromBody] MissingProductApproveDto dto)
            => Ok(await _service.ApproveAsync(_currentUser.CurrentUser.UserId, dto));

        /// <summary>رد با دلیل متنی (فروشنده می‌تواند اصلاح و دوباره بفرستد)</summary>
        [HttpPost("reject")]
        [ProducesResponseType(typeof(BaseResultDto<MissingProductAdminDto>), 200)]
        public async Task<IActionResult> Reject([FromBody] MissingProductRejectDto dto)
            => Ok(await _service.RejectAsync(_currentUser.CurrentUser.UserId, dto));
    }
}
