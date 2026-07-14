using Application.Common.Dto.Result;
using Application.Services.PastilMatchSrvs.PastilMatchMessageAttachmentSrv.Dto;
using Application.Services.PastilMatchSrvs.PastilMatchMessageAttachmentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت فایل‌های پیام پاستیل مچ
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PastilMatchMessageAttachmentController : ControllerBase
    {
        private readonly IPastilMatchMessageAttachmentService _pastilMatchMessageAttachmentService;

        public PastilMatchMessageAttachmentController(IPastilMatchMessageAttachmentService pastilMatchMessageAttachmentService)
        {
            _pastilMatchMessageAttachmentService = pastilMatchMessageAttachmentService;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PastilMatchMessageAttachmentVDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _pastilMatchMessageAttachmentService.FindAsyncVDto(id);
            return Ok(result);
        }

        /// <summary>
        /// جستجوی فایل‌های پیام
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PastilMatchMessageAttachmentSearchDto), StatusCodes.Status200OK)]
        public IActionResult Get([FromQuery] PastilMatchMessageAttachmentInputDto dto)
        {
            var result = _pastilMatchMessageAttachmentService.Search(dto);
            return Ok(result);
        }

        /// <summary>
        /// اضافه کردن فایل به پیام
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PastilMatchMessageAttachmentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] PastilMatchMessageAttachmentDto dto)
        {
            var result = await _pastilMatchMessageAttachmentService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        /// حذف فایل پیام
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), StatusCodes.Status200OK)]
        public IActionResult Delete(long id)
        {
            var result = _pastilMatchMessageAttachmentService.DeleteDto(id);
            return Ok(result);
        }
    }
}