using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.GroomerSrvs.GroomerPortfolioSrv.Dto;
using Application.Services.GroomerSrvs.GroomerPortfolioSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// نمونه‌کارهای آرایشگاه‌های من (پست‌های اینستاگرامی با لایک و کامنت کاربران)
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerPortfolioController : ControllerBase
    {
        private readonly IGroomerPortfolioService _service;
        private readonly ICurrentUserHelper _currentUser;

        public GroomerPortfolioController(IGroomerPortfolioService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        /// <summary>پست‌های آرایشگاه‌های من (شامل پنهان‌شده‌ها با دلیل)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(GroomerPortfolioPage<GroomerPortfolioPostVDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] GroomerPortfolioPostSearchInputDto dto) =>
            Ok(await _service.SearchManageAsync(dto, UserId));

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<GroomerPortfolioPostVDto>), 200)]
        public async Task<IActionResult> Post(GroomerPortfolioPostInputDto dto) => Ok(await _service.CreateAsync(dto, UserId));

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<GroomerPortfolioPostVDto>), 200)]
        public async Task<IActionResult> Put(GroomerPortfolioPostInputDto dto) => Ok(await _service.UpdateAsync(dto, UserId));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.DeleteAsync(id, UserId));

        /// <summary>حذف کامنت زیر نمونه‌کار خودم</summary>
        [HttpDelete("Comment")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> DeleteComment(long id) => Ok(await _service.DeleteCommentAsync(id, UserId, isAdmin: false));
    }
}
