using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.GroomerSrvs.GroomerPortfolioSrv.Dto;
using Application.Services.GroomerSrvs.GroomerPortfolioSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// نمونه‌کار آرایشگاه‌ها (فید عمومی اینستاگرامی). برای کاربر واردشده LikedByMe هم پر می‌شود.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class GroomerPortfolioController : ControllerBase
    {
        private readonly IGroomerPortfolioService _service;
        private readonly ICurrentUserHelper _currentUser;

        public GroomerPortfolioController(IGroomerPortfolioService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long? UserId => _currentUser.CurrentUser?.UserId;

        /// <summary>فید نمونه‌کارها؛ با groomerId فقط نمونه‌کارهای همان آرایشگاه</summary>
        [HttpGet]
        [ProducesResponseType(typeof(GroomerPortfolioPage<GroomerPortfolioPostVDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] GroomerPortfolioPostSearchInputDto dto)
        {
            dto.Approved = null;
            return Ok(await _service.SearchPublicAsync(dto, UserId));
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<GroomerPortfolioPostVDto>), 200)]
        public async Task<IActionResult> Get(long id) => Ok(await _service.GetPublicAsync(id, UserId));

        /// <summary>کامنت‌های یک نمونه‌کار؛ ParentId خالی = نظرهای اصلی، پر = پاسخ‌های آن نظر</summary>
        [HttpGet("Comments")]
        [ProducesResponseType(typeof(GroomerPortfolioPage<GroomerPortfolioCommentVDto>), 200)]
        public async Task<IActionResult> Comments([FromQuery] GroomerPortfolioCommentSearchInputDto dto) =>
            Ok(await _service.SearchCommentsAsync(dto, UserId));
    }
}
