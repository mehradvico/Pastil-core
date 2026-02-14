using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.BrandSrv.Dto;
using Application.Services.ProductSrvs.BrandSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت برندها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class BrandController : ControllerBase
    {
        private IBrandService brandService;
        /// <summary>
        /// مدیریت برند ها
        /// </summary>
        ///
        public BrandController(IBrandService brandService)
        {
            this.brandService = brandService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<BrandVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await brandService.FindAsyncVDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BrandSearchDto), 200)]
        public IActionResult Get([FromQuery] BrandInputDto dto)
        {
            var searchDto = brandService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BaseResultDto<BrandDto>), 200)]
        public async Task<IActionResult> Post(BrandDto brandDto)
        {
            var dto = await brandService.InsertAsyncDto(brandDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(BrandDto brandDto)
        {
            var dto = brandService.UpdateDto(brandDto);
            return Ok(dto);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = brandService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
