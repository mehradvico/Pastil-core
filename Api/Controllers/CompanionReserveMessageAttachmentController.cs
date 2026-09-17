using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionReserveMessageAttachmentSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveMessageAttachmentSrv.Iface;
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
    public class CompanionReserveMessageAttachmentController : ControllerBase
    {
        private readonly ICompanionReserveMessageAttachmentService _companionReserveMessageAttachmentService;

        public CompanionReserveMessageAttachmentController(ICompanionReserveMessageAttachmentService companionReserveMessageAttachmentService)
        {
            _companionReserveMessageAttachmentService = companionReserveMessageAttachmentService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveMessageAttachmentVDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _companionReserveMessageAttachmentService.FindAsyncVDto(id);
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(CompanionReserveMessageAttachmentSearchDto), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] CompanionReserveMessageAttachmentInputDto dto)
        {
            var result = _companionReserveMessageAttachmentService.Search(dto);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveMessageAttachmentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] CompanionReserveMessageAttachmentDto dto)
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
