using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionZoneSrv.Dto;
using Application.Services.CompanionSrvs.CompanionZoneSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// محل فعالیت نمایندگان
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionZoneController : ControllerBase
    {
        private readonly ICompanionZoneService CompanionZoneService;
        public CompanionZoneController(ICompanionZoneService CompanionZoneService)
        {
            this.CompanionZoneService = CompanionZoneService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionZoneDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var CompanionZone = await CompanionZoneService.FindAsyncDto(id);
            return Ok(CompanionZone);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionZoneSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionZoneInputDto dto)
        {
            var search = CompanionZoneService.Search(dto);
            return Ok(search);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionZoneDto>), 200)]
        public async Task<IActionResult> Post(CompanionZoneDto CompanionZoneDto)
        {
            var result = await CompanionZoneService.InsertAsyncDto(CompanionZoneDto);
            return Ok(result);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<CompanionZoneDto>), 200)]
        public async Task<IActionResult> Put(CompanionZoneDto CompanionZoneDto)
        {
            var result = await CompanionZoneService.UpdateAsyncDto(CompanionZoneDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<CompanionZoneDto>), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await CompanionZoneService.DeleteAsync(id);
            return Ok(result);
        }
    }
}
