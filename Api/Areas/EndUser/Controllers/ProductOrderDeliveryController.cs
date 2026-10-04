using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.ProductOrderSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Api.Areas.EndUser.Controllers
{
    public class ProductOrderDeliveryDto
    {
        [Required]
        public string Id { get; set; }
        // true = «تحویل گرفتم» (سفارش نهایی می‌شود)، false = «تحویل نگرفتم»
        public bool Received { get; set; }
        // توضیح اختیاری (برای «تحویل نگرفتم»؛ حداکثر ۵۰۰ نویسه)
        [MaxLength(500)]
        public string Note { get; set; }
    }

    /// <summary>
    /// تحویل‌گیری سفارش فروشگاهی توسط خود کاربر: «تحویل گرفتم» سفارش را نهایی می‌کند، «تحویل نگرفتم» فقط ثبت می‌شود و به فروشگاه و ادمین نمایش داده می‌شود
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductOrderDeliveryController : ControllerBase
    {
        private readonly IProductOrderService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ProductOrderDeliveryController(IProductOrderService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put([FromBody] ProductOrderDeliveryDto dto)
            => Ok(await _service.ConfirmDeliveryAsync(dto.Id, _currentUser.CurrentUser.UserId, dto.Received, dto.Note));
    }
}
