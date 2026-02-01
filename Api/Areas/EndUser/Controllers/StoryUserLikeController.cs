using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Content.StoryUserLikeSrv.Dto;
using Application.Services.Content.StoryUserLikeSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مدیریت لایک استوری
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class StoryUserLikeController : ControllerBase
    {
        private readonly IStoryUserLikeService _storyUserLikeService;
        private readonly ICurrentUserHelper _currentUserHelper;

        public StoryUserLikeController(IStoryUserLikeService storyUserLikeService, ICurrentUserHelper currentUserHelper)
        {
            _storyUserLikeService = storyUserLikeService;
            _currentUserHelper = currentUserHelper;
        }

        /// <summary>
        /// جسنجو
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(StoryUserLikeSearchDto), 200)]
        public IActionResult Get([FromQuery] StoryUserLikeInputDto dto)
        {
            dto.Available = true;
            dto.UserId = _currentUserHelper.CurrentUser.UserId;

            var result = _storyUserLikeService.SearchDto(dto);
            return Ok(result);
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<StoryUserLikeDto>), 200)]
        public async Task<IActionResult> Post(StoryUserLikeDto dto)
        {
            dto.UserId = _currentUserHelper.CurrentUser.UserId;
            await _storyUserLikeService.ToggleLikeAsync(dto.StoryItemId, dto.UserId);
            return Ok(new BaseResultDto(true, Resource.Notification.Success));
        }
    }
}
