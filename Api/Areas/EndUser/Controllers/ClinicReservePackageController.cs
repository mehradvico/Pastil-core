using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ClinicSrvs.ClinicReserveItemSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveItemSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// پکیج‌های رزرو من و وضعیتشان (در انتظار تأیید کلینیک / تأییدشده / لغوشده + مبلغ برگشتی به کیف پول)
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicReservePackageController : ControllerBase
    {
        private readonly IClinicReserveItemService _service;
        private readonly ICurrentUserHelper _currentUserHelper;

        public ClinicReservePackageController(IClinicReserveItemService service, ICurrentUserHelper currentUserHelper)
        {
            _service = service;
            _currentUserHelper = currentUserHelper;
        }

        [HttpGet("{reserveId}")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicReserveItemsVDto>), 200)]
        public async Task<IActionResult> Get(long reserveId)
            => Ok(await _service.GetForBookerAsync(reserveId, _currentUserHelper.CurrentUser?.UserId ?? 0));
    }
}
