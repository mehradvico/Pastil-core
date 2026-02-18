using Application.Common.Dto.Result;
using Application.Services.ProductSrvs.StoreSrv.Dto;
using Application.Services.StoreSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت فروشنده ها
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
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
        [HttpGet]
        [ProducesResponseType(typeof(StoreSearchDto), 200)]
        public IActionResult Get([FromQuery] StoreInputDto dto)
        {
            dto.Available = true;
            var Store = StoreService.Search(dto);
            return Ok(Store);
        }
    }
}
