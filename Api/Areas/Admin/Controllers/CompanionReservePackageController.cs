using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionReservePackageItemSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReservePackageItemSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پکیج‌های یک رزرو و وضعیتشان (در انتظار / تأییدشده / لغوشده)؛ تأیید یا لغوِ تکی، افزودن و حذف (رزرو پرداخت‌نشده)
    /// </summary>
    [Area("Admin")]
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

        private long UserId => _currentUserHelper.CurrentUser?.UserId ?? 0;

        /// <summary>فهرست پکیج‌ها با وضعیت و مبلغ برگشتی</summary>
        [HttpGet("{reserveId}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReservePackageItemsVDto>), 200)]
        public async Task<IActionResult> Get(long reserveId) => Ok(await _service.GetForManagerAsync(reserveId, UserId, true));

        /// <summary>تأیید (Status=2) یا لغو (Status=3 + Reason) یک پکیج؛ لغو، سهم پرداختی را به کیف پول کاربر برمی‌گرداند</summary>
        [HttpPut("{reserveId}/{packageId}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReservePackageStatusResultVDto>), 200)]
        public async Task<IActionResult> Put(long reserveId, long packageId, CompanionReservePackageStatusDto dto)
            => Ok(await _service.SetStatusAsync(reserveId, packageId, dto, UserId, true));

        /// <summary>افزودن پکیج به رزرو</summary>
        [HttpPost("{reserveId}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReservePackageItemsVDto>), 200)]
        public async Task<IActionResult> Post(long reserveId, CompanionReservePackageAddDto dto)
            => Ok(await _service.AddAsync(reserveId, dto, UserId, true));

        /// <summary>حذف پکیج از رزرو پرداخت‌نشده (برای پرداخت‌شده از لغو استفاده شود)</summary>
        [HttpDelete("{reserveId}/{packageId}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReservePackageItemsVDto>), 200)]
        public async Task<IActionResult> Delete(long reserveId, long packageId)
            => Ok(await _service.RemoveUnpaidAsync(reserveId, packageId, UserId, true));
    }
}
