using Application.Common.Dto.Input;
using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicSrv.Dto;
using Application.Services.ClinicSrvs.ClinicSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تنظیمات مدیریتی کلینیک: پیشنهادی، تأیید پروانه، درصد کمیسیون و نمایش در سایت
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicSettingController : ControllerBase
    {
        private readonly IClinicCoreService _service;
        public ClinicSettingController(IClinicCoreService service) => _service = service;

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(ClinicAdminSettingsDto dto) => Ok(await _service.UpdateAdminSettingsAsync(dto));

        /// <summary>نمایش در سایت (فقط کلینیک تأییدشده و فعال)</summary>
        [HttpPatch("{id:long}/visibility")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Visibility(long id, SiteVisibilityDto dto) =>
            Ok(await _service.UpdateSiteVisibilityAsync(id, dto.ShowToSite));
    }
}
