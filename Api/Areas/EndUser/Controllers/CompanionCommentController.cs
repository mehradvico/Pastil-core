using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionCommentSrv.Dto;
using Application.Services.CompanionSrvs.CompanionCommentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// مرتبط با نمایندگان
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionCommentController : ControllerBase
    {
        private ICompanionCommentService _CompanionCommentService;
        private ICurrentUserHelper _currentUserHelper;
        /// <summary>
        /// مرتبط با نمایندگان
        /// </summary>
        public CompanionCommentController(ICompanionCommentService CompanionCommentService, ICurrentUserHelper currentUserHelper)
        {
            this._CompanionCommentService = CompanionCommentService;
            this._currentUserHelper = currentUserHelper;
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// 
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CompanionCommentSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionCommentInputDto dto)
        {
            dto.Available = true;
            dto.AllStatus = false;
            var post = _CompanionCommentService.Search(dto);
            return Ok(post);
        }
        /// <summary>
        /// آیتم جدید
        /// </summary>  
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CompanionCommentDto>), 200)]
        public async Task<IActionResult> Post(CompanionCommentDto CompanionComment)
        {
            CompanionComment.UserId = _currentUserHelper.CurrentUser.UserId;
            var dto = await _CompanionCommentService.InsertAsyncDto(CompanionComment);
            return Ok(dto);
        }

    }
}
