using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolCourseLiveSrv.Dto;
using Application.Services.SchoolSrvs.SchoolCourseLiveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت «پاستیل لایو» یک جلسه‌ی دوره‌ی زنده‌ی مدرسه، توسط مربی
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolCourseLiveController : ControllerBase
    {
        private readonly ISchoolCourseLiveService _liveService;
        private readonly ICurrentUserHelper _currentUser;

        public SchoolCourseLiveController(ISchoolCourseLiveService liveService, ICurrentUserHelper currentUser)
        {
            _liveService = liveService;
            _currentUser = currentUser;
        }

        /// <summary>وضعیت فعلی لایوِ یک جلسه (اگر وجود نداشته باشد، ساخته می‌شود - هنوز شروع نشده)</summary>
        [HttpGet("session/{schoolCourseSessionId}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCourseLiveSessionVDto>), 200)]
        public async Task<IActionResult> GetForSession(long schoolCourseSessionId)
        {
            var companionId = _currentUser.CurrentUser.CompanionId;
            if (!companionId.HasValue)
                return Ok(new BaseResultDto<SchoolCourseLiveSessionVDto>(false, Resource.Notification.AccessDenied, null));

            var result = await _liveService.GetOrCreateForCompanionAsync(schoolCourseSessionId, companionId.Value);
            return Ok(result);
        }

        [HttpPost("start")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolLiveTokenVDto>), 200)]
        public async Task<IActionResult> Start(SchoolLiveStartDto dto)
        {
            var companionId = _currentUser.CurrentUser.CompanionId;
            if (!companionId.HasValue)
                return Ok(new BaseResultDto<SchoolLiveTokenVDto>(false, Resource.Notification.AccessDenied, null));

            var result = await _liveService.StartAsync(dto, companionId.Value);
            return Ok(result);
        }

        [HttpPost("end/{liveSessionId}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> End(long liveSessionId)
        {
            var companionId = _currentUser.CurrentUser.CompanionId;
            if (!companionId.HasValue)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));

            var result = await _liveService.EndAsync(liveSessionId, companionId.Value);
            return Ok(result);
        }

        /// <summary>مشخصات پت‌های ثبت‌نام‌شده در دوره + این‌که الان در لایو حاضرند یا غایب</summary>
        [HttpGet("{liveSessionId}/participants")]
        [ProducesResponseType(typeof(BaseResultDto<List<SchoolLiveParticipantVDto>>), 200)]
        public async Task<IActionResult> GetParticipants(long liveSessionId)
        {
            var companionId = _currentUser.CurrentUser.CompanionId;
            if (!companionId.HasValue)
                return Ok(new BaseResultDto<List<SchoolLiveParticipantVDto>>(false, Resource.Notification.AccessDenied, null));

            var result = await _liveService.GetParticipantsAsync(liveSessionId, companionId.Value);
            return Ok(result);
        }

        [HttpGet("{liveSessionId}/comments")]
        [ProducesResponseType(typeof(BaseResultDto<List<SchoolLiveCommentDto>>), 200)]
        public async Task<IActionResult> GetComments(long liveSessionId)
        {
            var result = await _liveService.GetCommentsAsync(liveSessionId, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }
    }
}
