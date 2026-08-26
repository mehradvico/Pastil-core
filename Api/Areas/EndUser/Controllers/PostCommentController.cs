using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Content.PostCommentSrv.Dto;
using Application.Services.Content.PostCommentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مرتبط با پست ها
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PostCommentController : ControllerBase
    {
        private IPostCommentService postCommentService;
        private readonly ICurrentUserHelper _currentUser;
        /// <summary>
        /// مرتبط با پست ها
        /// </summary>
        public PostCommentController(IPostCommentService postCommentService, ICurrentUserHelper currentUser)
        {
            this.postCommentService = postCommentService;
            this._currentUser = currentUser;
        }
        /// <summary>
        /// جستجو
        /// </summary>

        /// 
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PostCommentSearchDto), 200)]
        public IActionResult Get([FromQuery] PostCommentInputDto dto)
        {
            dto.Available = true;
            dto.AllStatus = false;
            var post = postCommentService.Search(dto);
            return Ok(post);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PostCommentDto>), 200)]
        public async Task<IActionResult> Post(PostCommentDto postComment)
        {
            postComment.UserId = _currentUser.CurrentUser.UserId;
            var dto = await postCommentService.InsertAsyncDto(postComment);
            return Ok(dto);
        }

    }
}
