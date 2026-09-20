using Application.Common.Dto.Result;
using Application.Services.Content.PostCommentSrv.Dto;
using Application.Services.Content.PostCommentSrv.Iface;
using Application.Common.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// مرتبط با پست ها
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class PostCommentController : ControllerBase
    {
        private IPostCommentService postCommentService;
        /// <summary>
        /// مرتبط با پست ها
        /// </summary>
        private readonly ICurrentUserHelper _currentUser;
        public PostCommentController(IPostCommentService postCommentService, ICurrentUserHelper currentUser)
        {
            this.postCommentService = postCommentService;
            _currentUser = currentUser;
        }
        /// <summary>
        /// جستجو
        /// </summary>

        /// 
        [HttpGet]
        [ProducesResponseType(typeof(PostCommentSearchDto), 200)]
        public IActionResult Get([FromQuery] PostCommentInputDto dto)
        {
            dto.Available = true;
            var post = postCommentService.Search(dto);
            return Ok(post);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        // قبلاً ناشناس بود و UserId را از بدنه می‌پذیرفت: هر کسی با اسم هر کاربر دیگری نظر می‌گذاشت و spam بی‌محدودیت بود.
        // (کلاینت‌ها از EndUser/PostComment استفاده می‌کنند؛ این مسیر فقط با لاگین و با UserId از توکن.)
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(BaseResultDto<PostCommentDto>), 200)]
        public async Task<IActionResult> Post(PostCommentDto postComment)
        {
            postComment.UserId = _currentUser.CurrentUser.UserId;
            var dto = await postCommentService.InsertAsyncDto(postComment);
            return Ok(dto);
        }

    }
}
