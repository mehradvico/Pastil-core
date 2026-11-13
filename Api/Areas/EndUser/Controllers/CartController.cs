using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Iface;
using Application.Services.Order.CartSrv.Dto;
using Application.Services.Order.CartSrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// سبد خرید
    /// </summary>
    ///
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    //[Authorize]
    public class CartController : ControllerBase
    {
        private ICartService CartService;
        private readonly IProductService ProductService;
        private readonly ICurrentUserHelper CurrentUserHelper;
        /// <summary>
        /// سبد خرید
        /// </summary>
        ///
        public CartController(ICartService CartService, IProductService ProductService, ICurrentUserHelper CurrentUserHelper)
        {
            this.CartService = CartService;
            this.ProductService = ProductService;
            this.CurrentUserHelper = CurrentUserHelper;
        }
        /// <summary>
        /// ویرایش Code 1: کاربر باید لاگین کند
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CartVDto>), 200)]
        public async Task<IActionResult> Post(CartUpdateDto cartUpdate)
        {
            var dto = await CartService.CartUpdateAsync(cartUpdate);
            return Ok(dto);
        }

        /// <summary>
        /// «چیزی را فراموش نکردید؟»: پیشنهاد مکمل برای کل سبد (کاربر لاگین‌شده از روی توکن؛ مهمان با uniqueId).
        /// فقط کالای موجود و مرتبط با همان حیوان؛ اگر پیشنهاد مطمئنی نباشد لیست خالی است.
        /// </summary>
        /// <param name="uniqueId">شناسه‌ی سبد مهمان (برای کاربر لاگین‌شده لازم نیست)</param>
        /// <param name="storeId">فروشگاهی که مشتری الان از آن ادامه می‌دهد: فقط اقلام همان فروشگاه نامزدکننده‌اند و فقط کالاهای موجودِ همان فروشگاه پیشنهاد می‌شوند</param>
        /// <param name="take">تعداد (پیش‌فرض ۶، حداکثر ۱۲)</param>
        [HttpGet("Suggestions")]
        [ProducesResponseType(typeof(CartSuggestionsVDto), 200)]
        public async Task<IActionResult> Suggestions([FromQuery] string uniqueId, [FromQuery] long? storeId, [FromQuery] int take = 6)
        {
            var userId = CurrentUserHelper.CurrentUser?.UserId;
            return Ok(await ProductService.CartSuggestionsAsync(userId, uniqueId, storeId, take));
        }

        /// <summary>
        /// خواندن سبد (GetCart پیش‌فرض، یا GetCount/GetDelivery) با GET؛ برای خواندن دیگر لازم نیست POST فرستاده شود.
        /// هر نوع تغییر‌دهنده‌ای این‌جا GetCart حساب می‌شود.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<CartVDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] CartUpdateDto cartUpdate)
        {
            cartUpdate ??= new CartUpdateDto();
            if (cartUpdate.CartUpdateType != Application.Common.Enumerable.CartUpdateEnum.GetCount &&
                cartUpdate.CartUpdateType != Application.Common.Enumerable.CartUpdateEnum.GetDelivery)
                cartUpdate.CartUpdateType = Application.Common.Enumerable.CartUpdateEnum.GetCart;
            var dto = await CartService.CartUpdateAsync(cartUpdate);
            return Ok(dto);
        }

    }


}
