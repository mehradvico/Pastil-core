using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    using Application.Common.Dto.Result;
    using Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Dto;
    using Application.Services.PastilMatchSrvs.PastilMatchMessageSrv.Iface;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    namespace Api.Areas.Admin.Controllers
    {
        /// <summary>
        /// پیام‌های پاستیل مچ
        /// </summary>
        [Area("Admin")]
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
            /// جستجو
            /// </summary>
            [HttpGet]
            [ProducesResponseType(typeof(PastilMatchMessageSearchDto), StatusCodes.Status200OK)]
            public IActionResult Get([FromQuery] PastilMatchMessageInputDto dto)
            {
                var result = _pastilMatchMessageService.Search(dto);
                return Ok(result);
            }

            /// <summary>
            /// حذف آیتم
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
}
