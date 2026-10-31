using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// فعال سازی پانسیون
    /// </summary>
    ///
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    [Api.Filters.RequireCompanionScope]
    public class PansionActiveController : ControllerBase
    {
        private readonly IPansionService _PansionService;
        private readonly ICurrentUserHelper _currentUserHelper;
        public PansionActiveController(IPansionService PansionService, ICurrentUserHelper currentUserHelper)
        {
            this._PansionService = PansionService;
            this._currentUserHelper = currentUserHelper;
        }

        /// <summary>
        ///  فعال سازی آیتم
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(PansionActiveDto dto)
        {
            // CompanionId خالی (مثلاً درخواست نمایندگیِ تأییدنشده) یعنی سرویس چک مالکیت را رد می‌کند؛ پس اینجا حتماً لازم است
            if (!_currentUserHelper.CurrentUser.CompanionId.HasValue)
                return Forbid();
            var Pansion = _PansionService.UpdatePansionActiveDto(dto, _currentUserHelper.CurrentUser.CompanionId.Value);
            return Ok(Pansion);
        }
    }
}
