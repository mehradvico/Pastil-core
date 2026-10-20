using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.GroomerSrvs.GroomerSrv.Dto;
using Application.Services.GroomerSrvs.GroomerSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تنظیمات مدیریتی آرایشگاه: پیشنهادی، تأیید پروانه، درصد کمیسیون و نمایش در سایت
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerSettingController : ControllerBase
    {
        private readonly IGroomerCoreService _service;
        public GroomerSettingController(IGroomerCoreService service) => _service = service;

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(GroomerAdminSettingsDto dto) => Ok(await _service.UpdateAdminSettingsAsync(dto));

        /// <summary>نمایش در سایت (فقط آرایشگاه تأییدشده و فعال)</summary>
        [HttpPatch("{id:long}/visibility")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Visibility(long id, SiteVisibilityDto dto) =>
            Ok(await _service.UpdateSiteVisibilityAsync(id, dto.ShowToSite));
    }
}
