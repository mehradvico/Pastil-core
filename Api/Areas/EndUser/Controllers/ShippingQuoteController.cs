using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.ShippingSrv.Dto;
using Application.Services.Order.ShippingSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// قیمت لحظه‌ای روش‌های ارسال
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ShippingQuoteController : ControllerBase
    {
        private readonly IShippingQuoteService _shippingQuoteService;
        private readonly ICurrentUserHelper _currentUser;

        public ShippingQuoteController(
            IShippingQuoteService shippingQuoteService,
            ICurrentUserHelper currentUser)
        {
            _shippingQuoteService = shippingQuoteService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// دریافت قیمت روش‌های ارسال فروشگاه فعال در سبد
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<List<ShippingQuoteVDto>>), 200)]
        public async Task<IActionResult> Post(
            ShippingQuoteCreateDto dto,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.CurrentUser?.UserId ?? 0;
            if (userId <= 0)
                return Unauthorized(new BaseResultDto(false, "برای دریافت قیمت ارسال وارد حساب کاربری شوید."));

            return Ok(await _shippingQuoteService.CreateQuotesAsync(
                userId,
                dto.StoreId,
                cancellationToken));
        }
    }
}
