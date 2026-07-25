using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.BrandCategorySrv.Dto;
using Application.Services.ProductSrvs.BrandCategorySrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    ///  دسته بندی های برند
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[Controller]")]
    [ApiController]
    [Authorize]
    public class BrandCategory : ControllerBase
    {
        private IBrandCategoryService brandCategoryService;
        /// <summary>
        ///  دسته بندی های برند
        /// </summary>
        public BrandCategory(IBrandCategoryService brandCategoryService)
        {
            this.brandCategoryService = brandCategoryService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>  
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Get(long brandId)
        {
            var dto = await brandCategoryService.FindAsyncDto(brandId);
            return Ok(dto);
        }
        /// <summary>
        /// اضافه و ویرایش دسته بندی برند
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Post(BrandCategoryDto dto)
        {
            var result = await brandCategoryService.InsertAndUpdateAsyncDto(dto);
            return Ok(result);
        }
    }
}
