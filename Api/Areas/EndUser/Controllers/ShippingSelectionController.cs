using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.ShippingSrv.Dto;
using Application.Services.Order.ShippingSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// انتخاب قیمت ارسال برای سبد
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ShippingSelectionController : ControllerBase
    {
        private readonly IShippingQuoteService _shippingQuoteService;
        private readonly ICurrentUserHelper _currentUser;

        public ShippingSelectionController(
            IShippingQuoteService shippingQuoteService,
            ICurrentUserHelper currentUser)
        {
            _shippingQuoteService = shippingQuoteService;
            _currentUser = currentUser;
        }

        /// <summary>
        /// اعمال Quote معتبر روی سبد
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Post(
            ShippingQuoteSelectDto dto,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.CurrentUser?.UserId ?? 0;
            if (userId <= 0)
                return Unauthorized(new BaseResultDto(false, "برای انتخاب روش ارسال وارد حساب کاربری شوید."));

            return Ok(await _shippingQuoteService.SelectQuoteAsync(
                userId,
                dto.QuoteToken,
                cancellationToken));
        }
    }
}
