using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.DiscountSrv.Dto;
using Application.Services.ProductSrvs.DiscountSrv.IFace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تخفیف ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class DiscountController : ControllerBase
    {
        private IDiscountService discountService;
        public DiscountController(IDiscountService discountService)
        {
            this.discountService = discountService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<DiscountDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {

            var role = await discountService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(DiscountSearchDto), 200)]
        public IActionResult Get([FromQuery] DiscountInputDto dto)
        {
            var searchDto = discountService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BaseResultDto<DiscountDto>), 200)]
        public async Task<IActionResult> Post(DiscountDto discountDto)
        {

            var dto = await discountService.InsertAsync(discountDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(DiscountDto discountDto)
        {

            var dto = await discountService.ActiveAsync(discountDto);
            return Ok(dto);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(DiscountDto discount)
        {

            var dto = await discountService.DeleteAsync(discount);
            return Ok(dto);
        }
    }
}
