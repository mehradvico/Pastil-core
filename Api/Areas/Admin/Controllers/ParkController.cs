using Application.Common.Dto.Result;
using Application.Services.LocationFields.ParkSrv.Dto;
using Application.Services.LocationFields.ParkSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت پارک ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ParkController : ControllerBase
    {
        private IParkService ParkService;
        /// <summary>
        /// مدیریت پارک ها
        /// </summary>
        ///
        public ParkController(IParkService ParkService)
        {
            this.ParkService = ParkService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ParkDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await ParkService.FindAsyncDto(id);
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<ParkVDto>), 200)]
        public IActionResult Get([FromQuery] ParkInputDto dto)
        {
            var searchDto = ParkService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BaseResultDto<ParkDto>), 200)]
        public async Task<IActionResult> Post(ParkDto ParkDto)
        {

            var dto = await ParkService.InsertAsyncDto(ParkDto);
            return Ok(dto);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(ParkDto ParkDto)
        {
            var dto = ParkService.UpdateDto(ParkDto);
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
            var dto = ParkService.DeleteDto(id);
            return Ok(dto);
        }
    }
}
