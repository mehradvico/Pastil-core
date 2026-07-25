using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.DiscountGroupSrv.Dto;
using Application.Services.ProductSrvs.DiscountGroupSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// گروه تخفیف
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class DiscountGroupController : ControllerBase
    {
        private IDiscountGroupService discountGroupService;
        public DiscountGroupController(IDiscountGroupService discountGroupService)
        {
            this.discountGroupService = discountGroupService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<DiscountGroupDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await discountGroupService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<DiscountGroupDto>), 200)]
        public IActionResult Get([FromQuery] BaseInputDto dto)
        {
            var searchDto = discountGroupService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BaseResultDto<DiscountGroupDto>), 200)]
        public async Task<IActionResult> Post(DiscountGroupDto discountGroupDto)
        {
            var dto = await discountGroupService.InsertAsyncDto(discountGroupDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(DiscountGroupDto discountGroupDto)
        {
            var dto = discountGroupService.UpdateDto(discountGroupDto);
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
            var dto = discountGroupService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
