using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.CoachSrvs.CoachSrv.Dto;
using Application.Services.CoachSrvs.CoachSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تنظیمات مدیریتی مربی: پیشنهادی، تأیید پروانه، درصد کمیسیون و نمایش در سایت
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachSettingController : ControllerBase
    {
        private readonly ICoachCoreService _service;
        public CoachSettingController(ICoachCoreService service) => _service = service;

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CoachAdminSettingsDto dto) => Ok(await _service.UpdateAdminSettingsAsync(dto));

        /// <summary>نمایش در سایت (فقط مربی تأییدشده و فعال)</summary>
        [HttpPatch("{id:long}/visibility")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Visibility(long id, SiteVisibilityDto dto) =>
            Ok(await _service.UpdateSiteVisibilityAsync(id, dto.ShowToSite));
    }
}
