using Application.Common.Dto.Result;
using Application.Services.Content.DetailSrv.Dto;
using Application.Services.Content.DetailSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مشخصات
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class DetailController : ControllerBase
    {
        private IDetailService DetailService;
        public DetailController(IDetailService DetailService)
        {
            this.DetailService = DetailService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<DetailDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await DetailService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<DetailDto>), 200)]
        public IActionResult Get([FromQuery] DetailInputDto dto)
        {
            var searchDto = DetailService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<DetailDto>), 200)]
        public async Task<IActionResult> Post(DetailDto DetailDto)
        {

            var dto = await DetailService.InsertAsyncDto(DetailDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(DetailDto DetailDto)
        {
            var dto = DetailService.UpdateDto(DetailDto);
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
            var dto = DetailService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
