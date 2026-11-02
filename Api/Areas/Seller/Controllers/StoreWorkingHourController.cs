using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.ShippingSrv.Dto;
using Application.Services.Order.ShippingSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// ساعت کاری فروشگاهِ خودِ فروشنده (اپ نماینده)؛ خارج از آن ارسال اکسپرس ارائه نمی‌شود
    /// </summary>
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreWorkingHourController : ControllerBase
    {
        private readonly IStoreWorkingHourService _service;
        private readonly ICurrentUserHelper _currentUser;

        public StoreWorkingHourController(IStoreWorkingHourService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>
        /// ساعت کاری فروشگاه من (لیست خالی = بدون محدودیت)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<StoreWorkingHourDto>>), 200)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var storeId = _currentUser.CurrentUser?.StoreId ?? 0;
            if (storeId <= 0)
                return Ok(new BaseResultDto<List<StoreWorkingHourDto>>(false, Resource.Notification.AccessDenied, null));
            return Ok(await _service.GetAsync(storeId, cancellationToken));
        }

        /// <summary>
        /// جایگزینی کل ساعت کاری هفته‌ی فروشگاه من
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(List<StoreWorkingHourDto> hours, CancellationToken cancellationToken)
        {
            var storeId = _currentUser.CurrentUser?.StoreId ?? 0;
            if (storeId <= 0)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));
            return Ok(await _service.SaveAsync(storeId, hours, cancellationToken));
        }
    }
}
