using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.GroomerSrvs.GroomerPortfolioSrv.Dto;
using Application.Services.GroomerSrvs.GroomerPortfolioSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// کامنت‌های نمونه‌کار آرایشگاه (مشاهده و حذف توسط ادمین)
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerPortfolioCommentController : ControllerBase
    {
        private readonly IGroomerPortfolioService _service;
        private readonly ICurrentUserHelper _currentUser;

        public GroomerPortfolioCommentController(IGroomerPortfolioService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        [HttpGet]
        [ProducesResponseType(typeof(GroomerPortfolioPage<GroomerPortfolioCommentVDto>), 200)]
        public async Task<IActionResult> Get([FromQuery] GroomerPortfolioCommentSearchInputDto dto) =>
            Ok(await _service.SearchCommentsAsync(dto, _currentUser.CurrentUser.UserId));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) =>
            Ok(await _service.DeleteCommentAsync(id, _currentUser.CurrentUser.UserId, isAdmin: true));
    }
}
