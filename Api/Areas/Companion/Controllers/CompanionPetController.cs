using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionPetSrv.Dto;
using Application.Services.CompanionSrvs.CompanionPetSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت پت های نمایندگان
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionPetController : ControllerBase
    {
        private readonly ICompanionPetService CompanionPetService;
        private readonly ICurrentUserHelper _current;
        /// <summary>
        /// مدیریت پت های نمایندگان
        /// </summary>

        public CompanionPetController(ICompanionPetService CompanionPetService, ICurrentUserHelper _current)
        {
            this.CompanionPetService = CompanionPetService;
            this._current = _current;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionPetDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var CompanionPet = await CompanionPetService.FindAsyncDto(id);
            return Ok(CompanionPet);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionPetSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionPetInputDto dto)
        {
            dto.CompanionId = _current.CurrentUser.CompanionId;
            var search = CompanionPetService.Search(dto);
            return Ok(search);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionPetDto>), 200)]
        public async Task<IActionResult> Post(CompanionPetDto CompanionPetDto)
        {
            if (!_current.CurrentUser.CompanionId.HasValue)
                return Forbid();
            CompanionPetDto.CompanionId = _current.CurrentUser.CompanionId.Value;
            var result = await CompanionPetService.InsertAsyncDto(CompanionPetDto);
            return Ok(result);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<CompanionPetDto>), 200)]
        public async Task<IActionResult> Put(CompanionPetDto CompanionPetDto)
        {
            if (!_current.CurrentUser.CompanionId.HasValue)
                return Forbid();
            var existing = await CompanionPetService.FindAsyncDto(CompanionPetDto.Id);
            if (!existing.IsSuccess || existing.Data?.CompanionId != _current.CurrentUser.CompanionId.Value)
                return Ok(new BaseResultDto<CompanionPetDto>(false, Resource.Notification.AccessDenied, default));
            CompanionPetDto.CompanionId = _current.CurrentUser.CompanionId.Value;
            var result = CompanionPetService.UpdateDto(CompanionPetDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        ///
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<CompanionPetDto>), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var existing = await CompanionPetService.FindAsyncDto(id);
            if (!existing.IsSuccess || existing.Data?.CompanionId != _current.CurrentUser.CompanionId)
                return Ok(new BaseResultDto<CompanionPetDto>(false, Resource.Notification.AccessDenied, default));
            var result = CompanionPetService.DeleteDto(id);
            return Ok(result);
        }
    }
}
