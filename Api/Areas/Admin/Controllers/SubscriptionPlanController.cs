using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.SubscriptionPlanSrv.Dto;
using Application.Services.ProviderSrvs.SubscriptionPlanSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پلن‌های اشتراک
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly ISubscriptionPlanService _service;
        public SubscriptionPlanController(ISubscriptionPlanService service)
        {
            _service = service;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<SubscriptionPlanDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            return Ok(await _service.FindAsyncDto(id));
        }

        /// <summary>
        /// جستجو
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(SubscriptionPlanSearchDto), 200)]
        public IActionResult Get([FromQuery] SubscriptionPlanInputDto dto)
        {
            return Ok(_service.Search(dto));
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<SubscriptionPlanDto>), 200)]
        public async Task<IActionResult> Post(SubscriptionPlanDto dto)
        {
            return Ok(await _service.InsertAsyncDto(dto));
        }

        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(SubscriptionPlanDto dto)
        {
            return Ok(_service.UpdateDto(dto));
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            return Ok(_service.DeleteDto(id));
        }
    }
}
