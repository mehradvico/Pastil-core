using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionReservePackageItemSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReservePackageItemSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// پکیج‌های رزرو من و وضعیتشان (در انتظار تأیید نماینده / تأییدشده / لغوشده + مبلغ برگشتی به کیف پول)
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReservePackageController : ControllerBase
    {
        private readonly ICompanionReservePackageItemService _service;
        private readonly ICurrentUserHelper _currentUserHelper;

        public CompanionReservePackageController(ICompanionReservePackageItemService service, ICurrentUserHelper currentUserHelper)
        {
            _service = service;
            _currentUserHelper = currentUserHelper;
        }

        [HttpGet("{reserveId}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReservePackageItemsVDto>), 200)]
        public async Task<IActionResult> Get(long reserveId)
            => Ok(await _service.GetForBookerAsync(reserveId, _currentUserHelper.CurrentUser?.UserId ?? 0));
    }
}
