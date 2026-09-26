using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionReserveCommentSrv.Dto;
using Application.Services.CompanionSrvs.CompanionReserveCommentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت کامنت های رزرو نمایندگان
    /// </summary>
    /// 
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionReserveCommentController : ControllerBase
    {
        private readonly ICompanionReserveCommentService CompanionReserveCommentService;
        private readonly ICurrentUserHelper _currentUser;
        /// <summary>
        /// مدیریت کامنت های رزرو نمایندگان
        /// </summary>

        public CompanionReserveCommentController(ICompanionReserveCommentService CompanionReserveCommentService, ICurrentUserHelper currentUser)
        {
            this._currentUser = currentUser;
            this.CompanionReserveCommentService = CompanionReserveCommentService;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// 
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionReserveCommentVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var CompanionReserveComment = await CompanionReserveCommentService.FindAsyncVDto(id);
            // فقط نظر رزروهای همین نماینده؛ نظر ردشده هم پنهان است (بدون افشای وجود رکورد دیگران)
            var companionId = _currentUser.CurrentUser.CompanionId;
            if (!CompanionReserveComment.IsSuccess || !companionId.HasValue ||
                CompanionReserveComment.Data?.CompanionReserve?.CompanionAssistance?.CompanionId != companionId.Value)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(CompanionReserveComment);
        }
        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(CompanionReserveCommentSearchDto), 200)]
        public IActionResult Get([FromQuery] CompanionReserveCommentInputDto dto)
        {
            // فقط نظرهای رزروهای همین نماینده (قبلاً هر نماینده‌ای می‌توانست هر رزروی را بخواند)
            var companionId = _currentUser.CurrentUser.CompanionId;
            if (!companionId.HasValue)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            dto.CompanionId = companionId.Value;
            dto.HideRejected = true;
            var CompanionReserveComment = CompanionReserveCommentService.Search(dto);
            return Ok(CompanionReserveComment);
        }
    }
}
