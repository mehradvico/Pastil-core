using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.VarietyItemSrv.Dto;
using Application.Services.ProductSrvs.VarietyItemSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// آیتم های تنوع
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class varietyItemController : ControllerBase
    {
        private IVarietyItemService varietyItemService;
        public varietyItemController(IVarietyItemService varietyItemService)
        {
            this.varietyItemService = varietyItemService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<VarietyItemDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await varietyItemService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<VarietyItemSearchDto>), 200)]
        public IActionResult Get([FromQuery] VarietyItemInputDto dto)
        {
            var searchDto = varietyItemService.SearchDto(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<VarietyItemDto>), 200)]
        public async Task<IActionResult> Post(VarietyItemDto varietyItemDto)
        {

            var dto = await varietyItemService.InsertAsyncDto(varietyItemDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(VarietyItemDto varietyItemDto)
        {
            var dto = varietyItemService.UpdateDto(varietyItemDto);
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
            var dto = varietyItemService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
