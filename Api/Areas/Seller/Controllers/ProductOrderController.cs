using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Services.Order.ProductOrderOrderSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// مدیریت سفارش ها
    /// </summary>
    ///
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductOrderController : ControllerBase
    {
        private IProductOrderService productOrderService;
        private readonly ICurrentUserHelper _currentUser;
        /// <summary>
        /// مدیریت سفارش ها
        /// </summary>
        ///
        public ProductOrderController(IProductOrderService productOrder, ICurrentUserHelper currentUser)
        {
            productOrderService = productOrder;
            this._currentUser = currentUser;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ProductOrderVDto>), 200)]
        public async Task<IActionResult> Get(string id)
        {
            var productOrder = await productOrderService.FindAsyncVDto(id);
            if (productOrder is BaseResultDto<ProductOrderVDto> typed && typed.IsSuccess &&
                (typed.Data?.ProductOrderStores == null || !typed.Data.ProductOrderStores.Any(s => s.StoreId == _currentUser.CurrentUser.StoreId)))
                return Ok(new BaseResultDto<ProductOrderVDto>(false, Resource.Notification.AccessDenied, default));
            return Ok(productOrder);
        }
        /// <summary>
        ///  جستجو
        /// </summary>

        /// <returns></returns> 

        [HttpGet()]
        [ProducesResponseType(typeof(BaseSearchDto<ProductOrderVDto>), 200)]
        public IActionResult Get([FromQuery] ProductOrderInputDto dto)
        {
            dto.StoreId = _currentUser.CurrentUser.StoreId;
            var productOrders = productOrderService.Search(dto);
            return Ok(productOrders);
        }

    }
}
