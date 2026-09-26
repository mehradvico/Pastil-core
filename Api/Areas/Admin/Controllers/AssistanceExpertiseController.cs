using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.AssistanceExpertiseSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceExpertiseSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تخصص‌های مرتبط با هر خدمت (برای پیشنهاد همکار مناسب هنگام تخصیص رزرو)
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class AssistanceExpertiseController : ControllerBase
    {
        private readonly IAssistanceExpertiseService _service;

        public AssistanceExpertiseController(IAssistanceExpertiseService service)
        {
            _service = service;
        }

        /// <summary>
        /// تخصص‌های مرتبط با یک خدمت
        /// </summary>
        [HttpGet("{assistanceId}")]
        [ProducesResponseType(typeof(BaseResultDto<AssistanceExpertiseVDto>), 200)]
        public async Task<IActionResult> Get(long assistanceId) =>
            Ok(await _service.GetAsync(assistanceId));

        /// <summary>
        /// جایگزینی کامل تخصص‌های مرتبط با یک خدمت
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<AssistanceExpertiseVDto>), 200)]
        public async Task<IActionResult> Put(AssistanceExpertiseDto dto) =>
            Ok(await _service.SaveAsync(dto));
    }
}
