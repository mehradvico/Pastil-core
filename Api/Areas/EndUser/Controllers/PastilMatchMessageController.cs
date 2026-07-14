using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت پیام‌های پاستیل مچ
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchMessageController : ControllerBase
    {
        private readonly IPastilMatchMessageService _pastilMatchMessageService;

        public PastilMatchMessageController(IPastilMatchMessageService pastilMatchMessageService)
        {
            _pastilMatchMessageService = pastilMatchMessageService;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PastilMatchMessageVDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _pastilMatchMessageService.FindAsyncVDto(id);
            return Ok(result);
        }

        /// <summary>
        /// جستجوی پیام‌های Match
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PastilMatchMessageSearchDto), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] PastilMatchMessageInputDto dto)
        {
            var result = _pastilMatchMessageService.Search(dto);
            return Ok(result);
        }

        /// <summary>
        /// ارسال پیام جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PastilMatchMessageDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] PastilMatchMessageDto dto)
        {
            var result = await _pastilMatchMessageService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        /// حذف پیام
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public IActionResult Delete(long id)
        {
            var result = _pastilMatchMessageService.DeleteDto(id);
            return Ok(result);
        }
    }
}