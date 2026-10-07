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
    /// مدیریت بازه‌های زمانی تحویل (روز هفته، ساعت، ظرفیت)
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ShippingSlotController : ControllerBase
    {
        private readonly IShippingSlotService _slotService;

        public ShippingSlotController(IShippingSlotService slotService)
        {
            _slotService = slotService;
        }

        /// <summary>
        /// همه‌ی بازه‌ها (به‌ترتیب روز هفته و ساعت)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ShippingSlotDto>>), 200)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
            => Ok(await _slotService.GetAllAsync(cancellationToken));

        /// <summary>
        /// ثبت بازه‌ی جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Post(ShippingSlotDto dto, CancellationToken cancellationToken)
        {
            dto.Id = 0;
            return Ok(await _slotService.SaveAsync(dto, cancellationToken));
        }

        /// <summary>
        /// ویرایش بازه
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(ShippingSlotDto dto, CancellationToken cancellationToken)
            => Ok(await _slotService.SaveAsync(dto, cancellationToken));

        /// <summary>
        /// حذف بازه (حذف نرم؛ سفارش‌های قبلی تأثیر نمی‌پذیرند)
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
            => Ok(await _slotService.DeleteAsync(id, cancellationToken));
    }
}
