using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.VarietySrv.Dto;
using Application.Services.ProductSrvs.VarietySrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تنوع ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class varietyController : ControllerBase
    {
        private IVarietyService varietyService;
        public varietyController(IVarietyService varietyService)
        {
            this.varietyService = varietyService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<VarietyDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await varietyService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<VarietyDto>), 200)]
        public IActionResult Get([FromQuery] BaseInputDto dto)
        {
            var searchDto = varietyService.SearchDto(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<VarietyDto>), 200)]
        public async Task<IActionResult> Post(VarietyDto varietyDto)
        {

            var dto = await varietyService.InsertAsyncDto(varietyDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(VarietyDto varietyDto)
        {
            var dto = varietyService.UpdateDto(varietyDto);
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
            var dto = varietyService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
