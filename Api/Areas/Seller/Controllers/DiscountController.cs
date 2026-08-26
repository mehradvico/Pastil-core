using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Services.ProductSrvs.DiscountSrv.Dto;
using Application.Services.ProductSrvs.DiscountSrv.IFace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Seller.Controllers
{
    /// <summary>
    /// مدیریت برندها
    /// </summary>
    ///
    [Area("Seller")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class DiscountController : ControllerBase
    {
        private IDiscountService discountService;
        private readonly ICurrentUserHelper _currentUser;

        /// <summary>
        /// مدیریت برند ها
        /// </summary>
        ///
        public DiscountController(IDiscountService discountService, ICurrentUserHelper currentUser)
        {
            this.discountService = discountService;
            this._currentUser = currentUser;

        }

        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(DiscountSearchDto), 200)]
        public IActionResult Get([FromQuery] DiscountInputDto dto)
        {
            dto.StoreId = _currentUser.CurrentUser.StoreId;
            var searchDto = discountService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<DiscountDto>), 200)]
        public async Task<IActionResult> Post(DiscountDto discountDto)
        {
            discountDto.StoreId = _currentUser.CurrentUser.StoreId;

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
            discountDto.StoreId = _currentUser.CurrentUser.StoreId;
            var dto = await discountService.ActiveAsync(discountDto);
            return Ok(dto);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(DiscountDto discountDto)
        {
            discountDto.StoreId = _currentUser.CurrentUser.StoreId;

            var dto = await discountService.DeleteAsync(discountDto);
            return Ok(dto);
        }
    }
}
