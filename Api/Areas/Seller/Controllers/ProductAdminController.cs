using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Services.ProductSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using Application.Services.ProductSrvs.ProductSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// مدیریت فروشگاه ها
    /// </summary>
    ///
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductAdminController : ControllerBase
    {
        private readonly IProductService productService;
        private readonly ICurrentUserHelper _currentUserHelper;
        private readonly long _storeId;
        /// <summary>
        /// مدیریت فروشگاه ها
        /// </summary>
        ///
        public ProductAdminController(IProductService productService, ICurrentUserHelper currentUserHelper, IHttpContextAccessor httpContextAccessor)
        {
            this.productService = productService;
            this._currentUserHelper = currentUserHelper;
            _storeId = HttpContextHelper.GetStoreId(httpContextAccessor.HttpContext);
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ProductDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var product = await productService.FindAsyncDto(id, storeId: _storeId);
            return Ok(product);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [ProducesResponseType(typeof(ProductSearchDto), 200)]
        public IActionResult Get([FromQuery] ProductInputDto dto)
        {
            dto.IsAdmin = true;
            dto.CreateStoreId = _storeId;
            var product = productService.Search(dto);
            return Ok(product);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// 

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ProductDto>), 200)]
        public async Task<IActionResult> Post(ProductDto productDto)
        {
            productDto.StoreId = _storeId;
            var item = await productService.InsertAsyncDto(productDto);
            return Ok(item);
        }
        /// <summary>
        /// کپی آیتم
        /// </summary>
        /// <param name="productDuplicateDto"></param>
        ///
        [HttpPost("duplicate")]
        [ProducesResponseType(typeof(BaseResultDto<ProductDto>), 200)]

        public async Task<IActionResult> Duplicate([FromBody] ProductDuplicateDto productDuplicateDto)
        {
            productDuplicateDto.StoreId = _storeId;
            var result = await productService.DuplicateAsyncDto(productDuplicateDto);
            return Ok(result);
        }

        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<ProductDto>), 200)]
        public async Task<IActionResult> Put(ProductDto productDto)
        {
            var item = await productService.UpdateDtoAsync(productDto, storeId: _storeId);
            return Ok(item);
        }

    }

}

