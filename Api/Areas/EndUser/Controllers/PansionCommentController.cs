using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Dto;
using Application.Services.PansionSrvs.PansionCommentSrv.Dto;
using Application.Services.PansionSrvs.PansionCommentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت نظرات پانسیون ها
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionCommentController : ControllerBase
    {
        private IPansionCommentService PansionCommentService;
        private readonly CurrentUserDto _currentUser;
        /// <summary>
        /// مدیریت نظرات پانسیون ها
        /// </summary>
        public PansionCommentController(IPansionCommentService PansionCommentService, ICurrentUserHelper currentUserHelper)
        {
            this.PansionCommentService = PansionCommentService;
            _currentUser = currentUserHelper.CurrentUser;

        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// 
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PansionCommentSearchDto), 200)]
        public IActionResult Get([FromQuery] PansionCommentInputDto dto, [FromQuery] bool mine = false)
        {
            // فهرست عمومی = نظرهای تأییدشده‌ی همه؛ فقط با mine=true نظرهای خودِ کاربر (لایک‌های خودش هم همراه می‌آید).
            dto.Available = true;
            dto.AllStatus = false;
            dto.CompanionId = null;
            dto.UserId = null;
            if (mine && _currentUser != null)
            {
                dto.UserId = _currentUser.UserId;
            }
            var post = PansionCommentService.Search(dto);
            return Ok(post);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PansionCommentDto>), 200)]
        public async Task<IActionResult> Post(PansionCommentDto PansionComment)
        {
            PansionComment.UserId = _currentUser.UserId;
            var dto = await PansionCommentService.InsertAsyncDto(PansionComment);
            return Ok(dto);
        }

    }
}
