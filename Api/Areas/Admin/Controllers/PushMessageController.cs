using Application.Common.Dto.Result;
using Application.Services.CommonSrv.PushBroadcastSrv.Dto;
using Application.Services.CommonSrv.PushBroadcastSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت پیام های پوش
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PushMessageController : ControllerBase
    {
        private readonly IPushMessageService PushMessageService;
        /// <summary>
        /// مدیریت پیام های پوش
        /// </summary>

        public PushMessageController(IPushMessageService PushMessageService)
        {
            this.PushMessageService = PushMessageService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PushMessageDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var PushMessage = await PushMessageService.FindAsyncVDto(id);
            return Ok(PushMessage);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(PushMessageSearchDto), 200)]
        public IActionResult Get([FromQuery] PushMessageInputDto dto)
        {
            var PushMessage = PushMessageService.Search(dto);
            return Ok(PushMessage);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PushMessageDto>), 200)]
        public async Task<IActionResult> Post(PushMessageDto PushMessageDto)
        {
            var result = await PushMessageService.InsertAsyncDto(PushMessageDto);
            return Ok(result);
        }
        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<PushMessageDto>), 200)]
        public IActionResult Put(PushMessageDto PushMessageDto)
        {
            var result = PushMessageService.UpdateDto(PushMessageDto);
            return Ok(result);
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<PushMessageDto>), 200)]
        public IActionResult Delete(long id)
        {
            var result = PushMessageService.DeleteDto(id);
            return Ok(result);
        }
    }
}
