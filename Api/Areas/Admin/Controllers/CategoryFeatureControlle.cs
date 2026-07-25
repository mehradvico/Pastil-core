using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.FeatureItemSrv.Dto;
using Application.Services.ProductSrvs.FeatureSrv.Dto;
using Application.Services.ProductSrvs.ProductFeatureValueSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// ویژگی ها
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryFeatureController : ControllerBase
    {
        private readonly ICategoryFeatureService _categoryFeatureService;
        /// <summary>
        ///  ویژگی ها
        /// </summary>
        /// 

        public CategoryFeatureController(ICategoryFeatureService categoryFeatureService)
        {
            this._categoryFeatureService = categoryFeatureService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<List<CategoryFeatureDto>>), 200)]
        public IActionResult Get(long id)
        {
            var item = _categoryFeatureService.GetForCategory(id);
            return Ok(item);
        }


        /// <summary>
        /// اضافه و حذف آیتم
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<FeatureDto>), 200)]
        public async Task<IActionResult> Post(CategoryFeatureDto dto)
        {
            var item = await _categoryFeatureService.UpdateAsync(dto);
            return Ok(item);
        }


    }
}
