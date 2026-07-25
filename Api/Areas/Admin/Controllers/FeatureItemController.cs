using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.FeatureItemSrv.Dto;
using Application.Services.ProductSrvs.FeatureSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// آیتم ویژگی ها
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class FeatureItemController : ControllerBase
    {
        private readonly IFeatureItemService featureItemService;
        public FeatureItemController(IFeatureItemService featureItemService)
        {
            this.featureItemService = featureItemService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<FeatureItemDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var item = await featureItemService.FindAsyncDto(id);
            return Ok(item);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(FeatureItemSearchDto), 200)]
        public IActionResult Get([FromQuery] FeatureItemInputDto dto)
        {
            var item = featureItemService.Search(dto);
            return Ok(item);
        }

        /// <summary>
        /// اضافه کردن آیتم
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<FeatureItemDto>), 200)]
        public async Task<IActionResult> Post(FeatureItemDto dto)
        {
            var item = await featureItemService.InsertAsyncDto(dto);
            return Ok(item);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(FeatureItemDto dto)
        {
            var item = featureItemService.UpdateDto(dto);
            return Ok(item);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var dto = featureItemService.DeleteDto(id);
            return Ok(dto);
        }

    }
}
