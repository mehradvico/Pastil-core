using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionUserSrv.Dto;
using Application.Services.CompanionSrvs.CompanionUserSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// کاربران نماینده
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionUserAvailableController : ControllerBase
    {
        private readonly ICompanionUserService _companionUserService;

        public CompanionUserAvailableController(ICompanionUserService companionUserService)
        {
            _companionUserService = companionUserService;
        }

        /// <summary>
        /// دریافت کاربران قابل انتخاب برای یک خدمت نمایندگی
        /// </summary>
        /// <param name="companionAssistanceId">شناسه خدمت نمایندگی</param>
        [HttpGet("{companionAssistanceId}")]
        [ProducesResponseType(typeof(BaseResultDto<List<CompanionUserDto>>), 200)]
        public async Task<IActionResult> Get(long companionAssistanceId)
        {
            var result = await _companionUserService.GetAvailableCompanionUsersAsync(companionAssistanceId);
            return Ok(result);
        }
    }
}
