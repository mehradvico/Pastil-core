using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.PetMicrochipSrv.Dto;
using Application.Services.Accounting.PetMicrochipSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers
{
    /// <summary>
    /// جستجوی پت با کد میکروچیپ (بخش «ارتباط با ما») و درخواست پیگیری توسط پاستیل؛ بدون هیچ مشخصات مالک
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    [EnableRateLimiting("MicrochipLookup")]
    public class PetMicrochipController : ControllerBase
    {
        private readonly IPetMicrochipService _service;
        private readonly ICurrentUserHelper _currentUserHelper;

        public PetMicrochipController(IPetMicrochipService service, ICurrentUserHelper currentUserHelper)
        {
            _service = service;
            _currentUserHelper = currentUserHelper;
        }

        /// <summary>جستجوی پت با میکروچیپ؛ پروفایل عمومی پت (بدون اطلاعات مالک) + توکن درخواست پیگیری</summary>
        [HttpPost("Search")]
        [ProducesResponseType(typeof(BaseResultDto<PetMicrochipSearchResultVDto>), 200)]
        [ProducesResponseType(429)]
        public async Task<IActionResult> Search(PetMicrochipSearchDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            return Ok(await _service.SearchAsync(dto, _currentUserHelper.CurrentUser?.UserId, ip));
        }

        /// <summary>دکمه‌ی «درخواست پیگیری توسط پاستیل»</summary>
        [HttpPost("FollowUp")]
        [ProducesResponseType(typeof(BaseResultDto<PetMicrochipFollowUpResultVDto>), 200)]
        [ProducesResponseType(429)]
        public async Task<IActionResult> FollowUp(PetMicrochipFollowUpDto dto)
        {
            return Ok(await _service.RequestFollowUpAsync(dto, _currentUserHelper.CurrentUser?.UserId));
        }
    }
}
