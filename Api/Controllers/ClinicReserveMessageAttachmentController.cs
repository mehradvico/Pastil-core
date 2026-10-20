using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicReserveMessageAttachmentSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveMessageAttachmentSrv.Iface;
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
    public class ClinicReserveMessageAttachmentController : ControllerBase
    {
        private readonly IClinicReserveMessageAttachmentService _companionReserveMessageAttachmentService;

        public ClinicReserveMessageAttachmentController(IClinicReserveMessageAttachmentService companionReserveMessageAttachmentService)
        {
            _companionReserveMessageAttachmentService = companionReserveMessageAttachmentService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicReserveMessageAttachmentVDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _companionReserveMessageAttachmentService.FindAsyncVDto(id);
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ClinicReserveMessageAttachmentSearchDto), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] ClinicReserveMessageAttachmentInputDto dto)
        {
            var result = _companionReserveMessageAttachmentService.Search(dto);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClinicReserveMessageAttachmentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] ClinicReserveMessageAttachmentDto dto)
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
