using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Content.DiscussionAnswerSrv.Dto;
using Application.Services.Content.DiscussionAnswerSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// فعال سازی پاسخ تالار گفت و گو
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class DiscussionAnswerActiveController : ControllerBase
    {
        private IDiscussionAnswerService _discussionAnswerService;
        public DiscussionAnswerActiveController(IDiscussionAnswerService discussionAnswerService)
        {
            _discussionAnswerService = discussionAnswerService;
        }
        /// <summary>
        /// فعالسازی آیتم
        /// </summary>  
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put([FromBody] DiscussionAnswerActiveDto dto)
        {
            var result = _discussionAnswerService.DiscussionAnswerActivation(dto);
            return Ok(result);
        }
    }
}
