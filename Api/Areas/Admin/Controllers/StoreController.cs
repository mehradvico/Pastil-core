using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Application.Services.StoreSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت فروشنده ها
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreController : ControllerBase
    {
        private IStoreService StoreService;
        /// <summary>
        /// مدیریت فروشنده ها
        /// </summary>
        public StoreController(IStoreService StoreService)
        {
            this.StoreService = StoreService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<StoreDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var Store = await StoreService.FindAsyncVDto(id);
            return Ok(Store);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [ProducesResponseType(typeof(StoreSearchDto), 200)]
        public IActionResult Get([FromQuery] StoreInputDto dto)
        {
            var Store = StoreService.Search(dto);
            return Ok(Store);
        }




        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// 

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<StoreDto>), 200)]
        public async Task<IActionResult> Post(StoreDto StoreDto)
        {
            var item = await StoreService.InsertAsyncDto(StoreDto);
            return Ok(item);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<StoreDto>), 200)]
        public IActionResult Put(StoreDto StoreDto)
        {
            var item = StoreService.UpdateDto(StoreDto);
            return Ok(item);
        }
        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<StoreDto>), 200)]
        public IActionResult Delete(long id)
        {
            var item = StoreService.DeleteDto(id);
            return Ok(item);
        }
    }
}
