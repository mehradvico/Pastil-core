using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ClinicSrvs.ClinicReserveItemSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveItemSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// پکیج‌های یک رزرو و وضعیتشان (در انتظار / تأییدشده / لغوشده)؛ تأیید یا لغوِ تکی، افزودن و حذف (رزرو پرداخت‌نشده)
    /// </summary>
    [Area("Companion")]
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

        private long UserId => _currentUserHelper.CurrentUser?.UserId ?? 0;

        /// <summary>فهرست پکیج‌ها با وضعیت و مبلغ برگشتی</summary>
        [HttpGet("{reserveId}")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicReserveItemsVDto>), 200)]
        public async Task<IActionResult> Get(long reserveId) => Ok(await _service.GetForManagerAsync(reserveId, UserId, false));

        /// <summary>تأیید (Status=2) یا لغو (Status=3 + Reason) یک پکیج؛ لغو، سهم پرداختی را به کیف پول کاربر برمی‌گرداند</summary>
        [HttpPut("{reserveId}/{packageId}")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicReserveItemStatusResultVDto>), 200)]
        public async Task<IActionResult> Put(long reserveId, long packageId, ClinicReserveItemStatusDto dto)
            => Ok(await _service.SetStatusAsync(reserveId, packageId, dto, UserId, false));

        /// <summary>افزودن پکیج به رزرو</summary>
        [HttpPost("{reserveId}")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicReserveItemsVDto>), 200)]
        public async Task<IActionResult> Post(long reserveId, ClinicReserveItemAddDto dto)
            => Ok(await _service.AddAsync(reserveId, dto, UserId, false));

    }
}
