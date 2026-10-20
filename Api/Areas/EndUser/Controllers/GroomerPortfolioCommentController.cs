using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.GroomerSrvs.GroomerPortfolioSrv.Dto;
using Application.Services.GroomerSrvs.GroomerPortfolioSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// کامنت روی نمونه‌کار آرایشگاه
    /// </summary>
    [Area("EndUser")]
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

        /// <summary>ثبت کامنت (یا پاسخ با ParentId)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<GroomerPortfolioCommentVDto>), 200)]
        public async Task<IActionResult> Post(GroomerPortfolioCommentInputDto dto) =>
            Ok(await _service.AddCommentAsync(dto, _currentUser.CurrentUser.UserId));

        /// <summary>حذف کامنت خودم</summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) =>
            Ok(await _service.DeleteCommentAsync(id, _currentUser.CurrentUser.UserId, isAdmin: false));
    }
}
