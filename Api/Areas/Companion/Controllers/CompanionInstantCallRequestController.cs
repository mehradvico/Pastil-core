using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionInstantCallRequestSrv.Dto;
using Application.Services.CompanionSrvs.CompanionInstantCallRequestSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مشاهده‌ی درخواست‌های تماس فوری برای نماینده
    /// </summary>
    ///
    [Area("Companion")]
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
        /// اطلاعات یک درخواست تماس فوری (برای صفحه‌ای که با کلیک روی پوش باز می‌شود)
        /// </summary>
        /// <param name="id">شناسه درخواست</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionInstantCallRequestVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var item = await _companionInstantCallRequestService.FindAsyncVDto(id);
            if (!item.IsSuccess || item.Data?.CompanionAssistancePackageOnlineSelection?.CompanionAssistancePackage?.CompanionAssistance?.CompanionId != _currentUserHelper.CurrentUser.CompanionId)
                return Ok(new BaseResultDto<CompanionInstantCallRequestVDto>(false, Resource.Notification.AccessDenied, item.Data));

            return Ok(item);
        }
    }
}
