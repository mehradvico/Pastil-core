using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderVenueSrv.Dto;
using Application.Services.ProviderSrvs.ProviderVenueSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// نمایندگی‌های یک ارائه‌دهنده (فروشگاه‌ها و نماینده‌ها) و اتصال آن‌ها
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderVenueController : ControllerBase
    {
        private readonly IProviderVenueService _service;
        public ProviderVenueController(IProviderVenueService service)
        {
            _service = service;
        }

        /// <summary>
        /// فروشگاه‌ها و نماینده‌های یک ارائه‌دهنده
        /// </summary>
        [HttpGet("{providerId}")]
        [ProducesResponseType(typeof(BaseResultDto<ProviderVenuesVDto>), 200)]
        public async Task<IActionResult> Get(long providerId)
        {
            return Ok(await _service.GetVenuesAsync(providerId));
        }

        /// <summary>
        /// اتصال یک نماینده (کلینیک/آرایشگاه/پانسیون‌دار...) به ارائه‌دهنده
        /// </summary>
        [HttpPut("companion")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> AssignCompanion(ProviderVenueAssignDto dto)
        {
            return Ok(await _service.AssignCompanionAsync(dto));
        }

        /// <summary>
        /// اتصال یک فروشگاه به ارائه‌دهنده
        /// </summary>
        [HttpPut("store")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> AssignStore(ProviderVenueAssignDto dto)
        {
            return Ok(await _service.AssignStoreAsync(dto));
        }

        /// <summary>
        /// اتصال یک آرایشگاه (Groomer) به ارائه‌دهنده
        /// </summary>
        [HttpPut("groomer")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> AssignGroomer(ProviderVenueAssignDto dto)
        {
            return Ok(await _service.AssignGroomerAsync(dto));
        }

        /// <summary>
        /// اتصال مدرسه به ارائه‌دهنده (نیازمند نقش تأییدشده‌ی School)
        /// </summary>
        [HttpPut("school")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> AssignSchool(ProviderVenueAssignDto dto)
        {
            return Ok(await _service.AssignSchoolAsync(dto));
        }

        /// <summary>
        /// اتصال یک کلینیک (Clinic) به ارائه‌دهنده
        /// </summary>
        [HttpPut("clinic")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> AssignClinic(ProviderVenueAssignDto dto)
        {
            return Ok(await _service.AssignClinicAsync(dto));
        }
    }
}
