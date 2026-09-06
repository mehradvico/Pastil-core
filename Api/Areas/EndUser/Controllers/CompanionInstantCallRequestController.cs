using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionInstantCallRequestSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInstantCallRequestSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// درخواست تماس فوری کاربر با نماینده (خارج از برنامه)
    /// </summary>
    ///
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionInstantCallRequestController : ControllerBase
    {
        private readonly ICompanionInstantCallRequestService _companionInstantCallRequestService;
        private readonly ICurrentUserHelper _currentUserHelper;

        public CompanionInstantCallRequestController(
            ICompanionInstantCallRequestService companionInstantCallRequestService,
            ICurrentUserHelper currentUserHelper)
        {
            this._companionInstantCallRequestService = companionInstantCallRequestService;
            this._currentUserHelper = currentUserHelper;
        }

        /// <summary>
        /// ثبت درخواست تماس فوری - بلافاصله برای نماینده پوش ارسال می‌شود
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(Application.Common.Dto.Result.BaseResultDto<CompanionInstantCallRequestDto>), 200)]
        public async Task<IActionResult> Post(CompanionInstantCallRequestDto dto)
        {
            dto.Id = 0;
            dto.BookerId = _currentUserHelper.CurrentUser.UserId;
            var result = await _companionInstantCallRequestService.InsertAsyncDto(dto);
            return Ok(result);
        }
    }
}
