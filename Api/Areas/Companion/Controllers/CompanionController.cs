using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionSrv.Dto;
using Application.Services.CompanionSrvs.CompanionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Agent.Controllers
{
    /// <summary>
    /// مدیریت نمایندگان 
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionController : ControllerBase
    {
        private readonly ICompanionService _companionService;
        private readonly ICurrentUserHelper _currentUserService;
        public CompanionController(ICompanionService companionService, ICurrentUserHelper currentUserHelper)
        {
            this._companionService = companionService;
            this._currentUserService = currentUserHelper;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه نمایندگی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            if (!_currentUserService.CurrentUser.CompanionId.HasValue)
                return Forbid();
            var companion = await _companionService.FindAsyncVDto(_currentUserService.CurrentUser.CompanionId.Value);
            return Ok(companion);
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CompanionDto dto)
        {
            if (!_currentUserService.CurrentUser.CompanionId.HasValue)
                return Forbid();
            dto.Active = false;
            dto.Approved = false;
            dto.OwnerId = _currentUserService.CurrentUser.UserId;
            dto.Id = _currentUserService.CurrentUser.CompanionId.Value;
            var companion = await _companionService.ResubmitAsyncDto(dto, _currentUserService.CurrentUser.UserId);
            return Ok(companion);
        }
    }
}
