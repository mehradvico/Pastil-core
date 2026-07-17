using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionZoneSrv.Dto;
using Application.Services.CompanionSrvs.CompanionZoneSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت محل فعالیت همکاران
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionZoneController : ControllerBase
    {
        private readonly ICompanionZoneService CompanionZoneService;
        private readonly ICurrentUserHelper _currentUser;
        public CompanionZoneController(ICompanionZoneService CompanionZoneService, ICurrentUserHelper currentUser)
        {
            this.CompanionZoneService = CompanionZoneService;
            this._currentUser = currentUser;
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
            dto.CompanionId = _currentUser.CurrentUser.CompanionId;
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
            if (!_currentUser.CurrentUser.CompanionId.HasValue)
                return Forbid();
            CompanionZoneDto.CompanionId = _currentUser.CurrentUser.CompanionId.Value;
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
        public IActionResult Put(CompanionZoneDto CompanionZoneDto)
        {
            if (!_currentUser.CurrentUser.CompanionId.HasValue)
                return Forbid();
            CompanionZoneDto.CompanionId = _currentUser.CurrentUser.CompanionId.Value;
            var result = CompanionZoneService.UpdateDto(CompanionZoneDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<CompanionZoneDto>), 200)]
        public IActionResult Delete(long id)
        {
            var result = CompanionZoneService.DeleteDto(id);
            return Ok(result);
        }
    }
}
