using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.FeatureItemSrv.Dto;
using Application.Services.ProductSrvs.ProductFeatureValueSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// ویژگی های محصول
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductFeatureValueController : ControllerBase
    {
        private readonly IProductFeatureValueService ProductFeatureValueService;
        public ProductFeatureValueController(IProductFeatureValueService ProductFeatureValueService)
        {
            this.ProductFeatureValueService = ProductFeatureValueService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ProductFeatureValueAddDto>), 200)]
        public IActionResult Get(long id)
        {
            var item = ProductFeatureValueService.GetForProduct(id);
            return Ok(item);
        }
        /// <summary>
        /// اضافه و حذف
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(ProductFeatureValueAddDto dto)
        {
            var item = ProductFeatureValueService.SetForProduct(dto);
            return Ok(item);
        }


    }
}
