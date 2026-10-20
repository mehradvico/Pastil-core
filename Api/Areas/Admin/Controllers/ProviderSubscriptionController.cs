using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ProviderSrvs.ProviderSubscriptionSrv.Dto;
using Application.Services.ProviderSrvs.ProviderSubscriptionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// اشتراک ارائه‌دهندگان: اختصاص دستی پلن توسط ادمین + تاریخچه
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderSubscriptionController : ControllerBase
    {
        private readonly IProviderSubscriptionService _service;
        private readonly ICurrentUserHelper _currentUser;
        public ProviderSubscriptionController(IProviderSubscriptionService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>
        /// تاریخچه‌ی اشتراک‌ها (فیلتر: ارائه‌دهنده، پلن، فقط جاری)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ProviderSubscriptionSearchDto), 200)]
        public IActionResult Get([FromQuery] ProviderSubscriptionInputDto dto)
        {
            return Ok(_service.Search(dto));
        }

        /// <summary>
        /// اشتراک جاری یک ارائه‌دهنده (Data = null یعنی بدون اشتراک)
        /// </summary>
        [HttpGet("current/{providerId}")]
        [ProducesResponseType(typeof(BaseResultDto<ProviderSubscriptionVDto>), 200)]
        public async Task<IActionResult> GetCurrent(long providerId)
        {
            return Ok(await _service.GetCurrentAsync(providerId));
        }

        /// <summary>
        /// اختصاص پلن به ارائه‌دهنده
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ProviderSubscriptionVDto>), 200)]
        public async Task<IActionResult> Post(ProviderSubscriptionDto dto)
        {
            return Ok(await _service.AssignAsync(dto, _currentUser.CurrentUser?.UserId));
        }

        /// <summary>
        /// پایان‌دادن فوری به اشتراک
        /// </summary>
        [HttpPut("cancel/{id}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Cancel(long id)
        {
            return Ok(await _service.CancelAsync(id));
        }

        /// <summary>
        /// حذف رکورد اشتباه از تاریخچه
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            return Ok(await _service.DeleteAsync(id));
        }
    }
}
