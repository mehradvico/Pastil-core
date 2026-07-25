using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionTypeSrv.Dto;
using Application.Services.CompanionSrvs.CompanionTypeSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت انواع نمایندگان
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionTypeController : ControllerBase
    {
        private readonly ICompanionTypeService CompanionTypeService;
        private readonly ICurrentUserHelper _current;
        /// <summary>
        /// مدیریت انواع نمایندگان
        /// </summary>

        public CompanionTypeController(ICompanionTypeService CompanionTypeService, ICurrentUserHelper _current)
        {
            this.CompanionTypeService = CompanionTypeService;
            this._current = _current;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionTypeDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var CompanionType = await CompanionTypeService.FindAsyncDto(id);
            return Ok(CompanionType);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionTypeSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionTypeInputDto dto)
        {
            dto.CompanionId = _current.CurrentUser.CompanionId;
            var search = CompanionTypeService.Search(dto);
            return Ok(search);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionTypeDto>), 200)]
        public async Task<IActionResult> Post(CompanionTypeDto CompanionTypeDto)
        {
            if (!_current.CurrentUser.CompanionId.HasValue)
                return Forbid();
            CompanionTypeDto.CompanionId = _current.CurrentUser.CompanionId.Value;
            var result = await CompanionTypeService.InsertAsyncDto(CompanionTypeDto);
            return Ok(result);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<CompanionTypeDto>), 200)]
        public IActionResult Put(CompanionTypeDto CompanionTypeDto)
        {
            if (!_current.CurrentUser.CompanionId.HasValue)
                return Forbid();
            CompanionTypeDto.CompanionId = _current.CurrentUser.CompanionId.Value;
            var result = CompanionTypeService.UpdateDto(CompanionTypeDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<CompanionTypeDto>), 200)]
        public IActionResult Delete(long id)
        {
            var result = CompanionTypeService.DeleteDto(id);
            return Ok(result);
        }
    }
}
