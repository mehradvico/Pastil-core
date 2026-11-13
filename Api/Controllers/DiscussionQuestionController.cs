using Application.Services.Content.DiscussionQuestionSrv.Dto;
using Application.Services.Content.DiscussionQuestionSrv.Iface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مدیریت پرسش های گفت و گو
    /// </summary>
    ///
    [Route("api/[controller]")]
    [ApiController]
    public class DiscussionQuestionController : ControllerBase
    {
        private IDiscussionQuestionService _DiscussionQuestionService;
        /// <summary>
        /// مدیریت پرسش گفت و گو
        /// </summary>
        ///
        public DiscussionQuestionController(IDiscussionQuestionService DiscussionQuestionService)
        {
            _DiscussionQuestionService = DiscussionQuestionService;
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(DiscussionQuestionInputDto), 200)]
        public IActionResult Get([FromQuery] DiscussionQuestionInputDto dto)
        {
            dto.Available = true;
            dto.HasPendingAnswers = null;
            dto.Pending = null;
            var searchDto = _DiscussionQuestionService.Search(dto);
            return Ok(searchDto);
        }
    }
}
