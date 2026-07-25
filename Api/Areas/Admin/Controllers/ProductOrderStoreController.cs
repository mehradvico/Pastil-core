using Application.Common.Dto.Result;
using Application.Services.Order.ProductOrderStoreOrderSrv.Dto;
using Application.Services.Order.ProductOrderStoreSrv.Dto;
using Application.Services.Order.ProductOrderStoreSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    ///  فروشنده آیتم سفارش
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductOrderStoreController : ControllerBase
    {
        private IProductOrderStoreService _productOrderStoreService;
        public ProductOrderStoreController(IProductOrderStoreService productOrderStoreService)
        {
            this._productOrderStoreService = productOrderStoreService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ProductOrderStoreDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var productOrder = await _productOrderStoreService.FindAsyncDto(id);
            return Ok(productOrder);
        }
        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(ProductOrderStoreDto dto)
        {
            var productOrder = _productOrderStoreService.UpdateDto(dto);
            return Ok(productOrder);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(ProductOrderStoreSearchDto), 200)]
        public IActionResult Get([FromQuery] ProductOrderStoreInputDto dto)
        {
            var productOrderItems = _productOrderStoreService.Search(dto);
            return Ok(productOrderItems);
        }

    }
}
