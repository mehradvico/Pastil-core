using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.ProductOrderOrderSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Dto;
using Application.Services.Order.ProductOrderSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت سفارش ها
    /// </summary>
    ///
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductOrderController : ControllerBase
    {
        private IProductOrderService productOrderService;
        private ICurrentUserHelper _currentUserHelper;

        /// <summary>
        /// مدیریت سفارش ها
        /// </summary>
        ///
        public ProductOrderController(IProductOrderService productOrder, ICurrentUserHelper currentUserHelper)
        {
            productOrderService = productOrder;
            _currentUserHelper = currentUserHelper;
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
            var productOrder = await productOrderService.FindAsyncVDto(
                id,
                _currentUserHelper.CurrentUser.UserId);
            return Ok(productOrder);
        }
        /// <summary>
        ///  جستجو
        /// </summary>

        /// <returns></returns> 

        [HttpGet()]
        [ProducesResponseType(typeof(BaseSearchDto<ProductOrderDto>), 200)]
        public IActionResult Get([FromQuery] ProductOrderInputDto dto)
        {
            dto.UserId = _currentUserHelper.CurrentUser.UserId;
            var productOrders = productOrderService.Search(dto);
            return Ok(productOrders);
        }

    }
}
