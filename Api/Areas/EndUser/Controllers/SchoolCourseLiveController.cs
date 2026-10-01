using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolCourseLiveSrv.Dto;
using Application.Services.SchoolSrvs.SchoolCourseLiveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// ورود کاربر به «پاستیل لایو» یک جلسه‌ی دوره‌ی زنده‌ی مدرسه (فقط تماشا)
    /// </summary>
    [Area("EndUser")]
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

        /// <summary>از schoolCourseSessionId (چیزی که پوش «شروع شد» می‌دهد) به liveSessionId می‌رسد.</summary>
        [HttpGet("session/{schoolCourseSessionId}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCourseLiveSessionVDto>), 200)]
        public async Task<IActionResult> GetForSession(long schoolCourseSessionId)
        {
            var result = await _liveService.GetForEnrolledUserAsync(schoolCourseSessionId, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }

        /// <summary>توکن ورود LiveKit (فقط تماشا) - اگر مربی هنوز شروع نکرده، StatusId=۱ برمی‌گردد؛ صفحه باید منتظر بماند.</summary>
        [HttpGet("{liveSessionId}/token")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolLiveTokenVDto>), 200)]
        public async Task<IActionResult> GetToken(long liveSessionId)
        {
            var result = await _liveService.GetViewerTokenAsync(liveSessionId, _currentUser.CurrentUser.UserId);
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
