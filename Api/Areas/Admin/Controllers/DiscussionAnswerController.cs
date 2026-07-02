using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Content.DiscussionAnswerSrv.Dto;
using Application.Services.Content.DiscussionAnswerSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت پاسخ های تالار گفت و گو
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class DiscussionAnswerController : ControllerBase
    {
        private IDiscussionAnswerService _discussionAnswerService;
        /// <summary>
        /// مدیریت پاسخ گفت و گو
        /// </summary>
        ///
        public DiscussionAnswerController(IDiscussionAnswerService discussionAnswerService)
        {
            _discussionAnswerService = discussionAnswerService;
        }
        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<DiscussionAnswerVDto>),200)]
        public async Task<IActionResult> Get(long id)
        {
            var dto = await _discussionAnswerService.FindAsyncVDto(id);
            return Ok(dto);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(DiscussionAnswerInputDto), 200)]
        public IActionResult Get([FromQuery] DiscussionAnswerInputDto dto)
        {
            var searchDto = _discussionAnswerService.Search(dto);
            return Ok(searchDto);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<DiscussionAnswerDto>), 200)]
        public async Task<IActionResult> Post(DiscussionAnswerDto discussionAnswerDto)
        {
            var dto = await _discussionAnswerService.InsertAsyncDto(discussionAnswerDto);
            return Ok(dto);
        }
    }
}
