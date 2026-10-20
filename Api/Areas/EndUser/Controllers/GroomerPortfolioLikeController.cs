using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.GroomerSrvs.GroomerPortfolioSrv.Dto;
using Application.Services.GroomerSrvs.GroomerPortfolioSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// لایک نمونه‌کار آرایشگاه
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerPortfolioLikeController : ControllerBase
    {
        private readonly IGroomerPortfolioService _service;
        private readonly ICurrentUserHelper _currentUser;

        public GroomerPortfolioLikeController(IGroomerPortfolioService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        /// <summary>لایک (تکراری بودن مشکلی ندارد؛ همیشه وضعیت نهایی و تعداد را برمی‌گرداند)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<GroomerPortfolioLikeResultDto>), 200)]
        public async Task<IActionResult> Post(long postId) => Ok(await _service.LikeAsync(postId, _currentUser.CurrentUser.UserId));

        /// <summary>برداشتن لایک</summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto<GroomerPortfolioLikeResultDto>), 200)]
        public async Task<IActionResult> Delete(long postId) => Ok(await _service.UnlikeAsync(postId, _currentUser.CurrentUser.UserId));
    }
}
