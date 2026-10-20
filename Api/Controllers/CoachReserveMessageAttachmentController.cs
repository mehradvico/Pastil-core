using Application.Common.Dto.Result;
using Application.Services.CoachSrvs.CoachReserveMessageAttachmentSrv.Dto;
using Application.Services.CoachSrvs.CoachReserveMessageAttachmentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت فایل‌های پیام چت خدمات آنلاین
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachReserveMessageAttachmentController : ControllerBase
    {
        private readonly ICoachReserveMessageAttachmentService _companionReserveMessageAttachmentService;

        public CoachReserveMessageAttachmentController(ICoachReserveMessageAttachmentService companionReserveMessageAttachmentService)
        {
            _companionReserveMessageAttachmentService = companionReserveMessageAttachmentService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CoachReserveMessageAttachmentVDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _companionReserveMessageAttachmentService.FindAsyncVDto(id);
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(CoachReserveMessageAttachmentSearchDto), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] CoachReserveMessageAttachmentInputDto dto)
        {
            var result = _companionReserveMessageAttachmentService.Search(dto);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CoachReserveMessageAttachmentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] CoachReserveMessageAttachmentDto dto)
        {
            var result = await _companionReserveMessageAttachmentService.InsertAsyncDto(dto);
            return Ok(result);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public IActionResult Delete(long id)
        {
            var result = _companionReserveMessageAttachmentService.DeleteDto(id);
            return Ok(result);
        }
    }
}
