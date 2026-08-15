using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionUserSrv.Dto;
using Application.Services.CompanionSrvs.CompanionUserSrv.Iface;
using Application.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت خدمات نمایندگان
    /// </summary>
    /// 
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionUserController : ControllerBase
    {
        private readonly ICompanionUserService _CompanionUserService;
        private readonly CurrentUserDto _currentUser;

        public CompanionUserController(ICompanionUserService CompanionUserService, ICurrentUserHelper currentUserHelper)
        {
            this._CompanionUserService = CompanionUserService;
            _currentUser = currentUserHelper.CurrentUser;
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(CompanionUserSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionUserInputDto dto)
        {
            dto.UserId = _currentUser.UserId;
            dto.Active = null;
            dto.AllUserAccept = true;
            var search = _CompanionUserService.SearchDto(dto);
            return Ok(search);
        }
        /// <summary>
        /// تایید و عدم تایید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionUserDto>), 200)]
        public async Task<IActionResult> Post(CompanionUserDto companionUser)
        {
            if (!companionUser.UserAccept.HasValue)
                return Ok(new BaseResultDto(false, Resource.Notification.Unsuccess));

            var result = await _CompanionUserService.UserAccept(
                companionUser.Id,
                _currentUser.UserId,
                companionUser.UserAccept.Value);
            return Ok(result);
        }

        /// <summary>
        /// تایید یا رد درخواست عضویت در نمایندگی
        /// </summary>
        [HttpPatch("{id:long}/decision")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PatchDecision(long id, CompanionUserDecisionDto dto)
        {
            if (!dto.UserAccept.HasValue)
                return Ok(new BaseResultDto(false, Resource.Notification.Unsuccess));

            var result = await _CompanionUserService.UserAccept(
                id,
                _currentUser.UserId,
                dto.UserAccept.Value);
            return Ok(result);
        }
    }
}
