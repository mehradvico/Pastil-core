using Application.Common.Dto.Result;
using Application.Services.Order.ShippingSrv.Dto;
using Application.Services.Order.ShippingSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// ساعت کاری هفتگی فروشگاه (خارج از آن ارسال اکسپرس ارائه نمی‌شود)
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreWorkingHourController : ControllerBase
    {
        private readonly IStoreWorkingHourService _service;

        public StoreWorkingHourController(IStoreWorkingHourService service)
        {
            _service = service;
        }

        /// <summary>
        /// ساعت کاری فروشگاه (روزهای بدون ردیف = تعطیل؛ لیست خالی = بدون محدودیت)
        /// </summary>
        [HttpGet("{storeId:long}")]
        [ProducesResponseType(typeof(BaseResultDto<List<StoreWorkingHourDto>>), 200)]
        public async Task<IActionResult> Get(long storeId, CancellationToken cancellationToken)
            => Ok(await _service.GetAsync(storeId, cancellationToken));

        /// <summary>
        /// جایگزینی کل ساعت کاری هفته
        /// </summary>
        [HttpPut("{storeId:long}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(long storeId, List<StoreWorkingHourDto> hours, CancellationToken cancellationToken)
            => Ok(await _service.SaveAsync(storeId, hours, cancellationToken));
    }
}
