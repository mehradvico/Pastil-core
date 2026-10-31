using Application.Common.Dto.Result;
using Application.Common.Helpers;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionCommentSrv.Dto;
using Application.Services.CompanionSrvs.CompanionCommentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت نظرات نمایندگان
    /// </summary>
    ///
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanionCommentController : ControllerBase
    {
        private ICompanionCommentService _CompanionCommentService;
        private ICurrentUserHelper _currentUser;
        /// <summary>
        /// مدیریت نظرات نمایندگان
        /// </summary>
        ///
        public CompanionCommentController(ICompanionCommentService CompanionCommentService, ICurrentUserHelper currentUser)
        {
            this._CompanionCommentService = CompanionCommentService;
            this._currentUser = currentUser;
        }
        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        /// <param name="id">شناسه دسته بندی</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<CompanionCommentDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var role = await _CompanionCommentService.FindAsyncDto(id);
            if (role.IsSuccess && role.Data?.CompanionId != _currentUser.CurrentUser.CompanionId)
                return Ok(new Application.Common.Dto.Result.BaseResultDto<CompanionCommentDto>(false, Resource.Notification.AccessDenied, default!));
            return Ok(role);
        }
        /// <summary>
        ///  جستجو
        /// </summary>
        /// <returns></returns> 

        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<CompanionCommentDto>), 200)]
        public IActionResult Get([FromQuery] CompanionCommentInputDto dto)
        {
            if (!_currentUser.CurrentUser.CompanionId.HasValue)
                return NotFound(new Application.Common.Dto.Result.BaseResultDto(false, Resource.Notification.NothingFound));
            dto.CompanionId = _currentUser.CurrentUser.CompanionId;
            var searchDto = _CompanionCommentService.Search(dto);
            return Ok(searchDto);
        }

        /// <summary>
        /// ویرایش نظرات
        /// </summary>

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CompanionCommentDto CompanionCommentDto)
        {
            if (!_currentUser.CurrentUser.CompanionId.HasValue)
                return Forbid();
            var existing = await _CompanionCommentService.FindAsyncDto(CompanionCommentDto.Id);
            if (!existing.IsSuccess || existing.Data == null || existing.Data.CompanionId != _currentUser.CurrentUser.CompanionId.Value)
                return Ok(new Application.Common.Dto.Result.BaseResultDto(false, Resource.Notification.AccessDenied));
            // نماینده فقط پاسخ می‌دهد؛ تأیید/رد نظرِ مربوط به خودش با ادمین است (وگرنه نظرهای منفی را مخفی می‌کرد)
            CompanionCommentDto.StatusId = existing.Data.StatusId;
            CompanionCommentDto.CompanionId = _currentUser.CurrentUser.CompanionId.Value;
            var dto = _CompanionCommentService.UpdateDto(CompanionCommentDto);
            return Ok(dto);
        }
    }

}

