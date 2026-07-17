using Api.Authorization;
using Application.Common.Dto.Result;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public class NoticeController : ControllerBase
    {
        private readonly INoticeService _noticeService;

        public NoticeController(INoticeService noticeService)
        {
            _noticeService = noticeService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(NoticeSearchDto), 200)]
        public IActionResult Get([FromQuery] NoticeInputDto dto)
        {
            return Ok(_noticeService.Search(dto));
        }

        [HttpGet("types")]
        [ProducesResponseType(typeof(List<NoticeTypeVDto>), 200)]
        public async Task<IActionResult> GetTypes(bool activeOnly = true)
        {
            return Ok(await _noticeService.GetTypesAsync(activeOnly));
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            return Ok(await _noticeService.GetUnreadCountAsync());
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<NoticeDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            return Ok(await _noticeService.FindAsyncDto(id));
        }

        [HttpPost("{id:long}/read")]
        [ProducesResponseType(typeof(BaseResultDto<NoticeDto>), 200)]
        public async Task<IActionResult> Read(long id)
        {
            return Ok(await _noticeService.ReadAsync(id));
        }

        [HttpPost("read/bulk")]
        [ProducesResponseType(typeof(BaseResultDto<NoticeBulkReadVDto>), 200)]
        public async Task<IActionResult> ReadBulk([FromBody] NoticeBulkReadDto dto)
        {
            return Ok(await _noticeService.ReadBulkAsync(dto));
        }
    }
}
