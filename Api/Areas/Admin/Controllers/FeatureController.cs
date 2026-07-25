using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.FeatureSrv.Dto;
using Application.Services.ProductSrvs.FeatureSrv.Iface;
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
    public class FeatureController : ControllerBase
    {
        private readonly IFeatureService featureService;
        public FeatureController(IFeatureService featureService)
        {
            this.featureService = featureService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<FeatureDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var item = await featureService.FindAsyncDto(id);
            return Ok(item);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(FeatureSearchDto), 200)]
        public IActionResult Get([FromQuery] FeatureInputDto dto)
        {
            var item = featureService.Search(dto);
            return Ok(item);
        }

        /// <summary>
        /// اضافه کردن آیتم
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<FeatureDto>), 200)]
        public async Task<IActionResult> Post(FeatureDto dto)
        {
            var item = await featureService.InsertAsyncDto(dto);
            return Ok(item);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(FeatureDto dto)
        {
            var item = featureService.UpdateDto(dto);
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
            var dto = featureService.DeleteDto(id);
            return Ok(dto);
        }

    }
}
