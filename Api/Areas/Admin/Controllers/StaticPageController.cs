using Application.Common.Dto.Result;
using Application.Services.Content.StaticPageSrv.Dto;
using Application.Services.Content.StaticPageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// صفحات ثابت
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class StaticPageController : ControllerBase
    {
        private IStaticPageService StaticPageService;
        /// <summary>
        /// صفحات ثابت 
        /// </summary>
        ///
        public StaticPageController(IStaticPageService StaticPageService)
        {
            this.StaticPageService = StaticPageService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<StaticPageDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var item = await StaticPageService.FindAsyncDto(id);
            return Ok(item);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<StaticPageDto>), 200)]
        public IActionResult Get([FromQuery] StaticPageInputDto dto)
        {
            var searchDto = StaticPageService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BaseResultDto<StaticPageDto>), 200)]
        public async Task<IActionResult> Post(StaticPageDto dto)
        {

            var model = await StaticPageService.InsertAsyncDto(dto);
            return Ok(model);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(StaticPageDto StaticPageDto)
        {
            var dto = StaticPageService.UpdateDto(StaticPageDto);
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
            var dto = StaticPageService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
