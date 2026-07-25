using Application.Common.Dto.Result;
using Application.Services.Content.BannerSrv.Dto;
using Application.Services.Content.BannerSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    ///  بنرها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class BannerController : ControllerBase
    {
        private IBannerService bannerService;
        /// <summary>
        ///  بنرها
        /// </summary>
        ///
        public BannerController(IBannerService bannerService)
        {
            this.bannerService = bannerService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<BannerVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await bannerService.FindAsyncVDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<BannerDto>), 200)]
        public IActionResult Get([FromQuery] BannerInputDto dto)
        {
            var searchDto = bannerService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<BannerDto>), 200)]
        public async Task<IActionResult> Post(BannerDto dto)
        {

            var model = await bannerService.InsertAsyncDto(dto);
            return Ok(model);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(BannerDto bannerDto)
        {
            var dto = bannerService.UpdateDto(bannerDto);
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
            var dto = bannerService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
