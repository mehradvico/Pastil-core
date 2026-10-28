using Application.Services.SchoolSrvs.SchoolOwnershipSrv;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Dto;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// جلسات زنده‌ی دوره‌ی مدرسه‌ی خودِ نماینده
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolCourseSessionController : ControllerBase
    {
        private readonly ISchoolCourseService _schoolCourseService;
        private readonly ICurrentUserHelper _currentUser;
        private readonly ISchoolOwnershipService _ownership;
        public SchoolCourseSessionController(ISchoolCourseService schoolCourseService, ICurrentUserHelper currentUser, ISchoolOwnershipService ownership)
        {
            this._ownership = ownership;
            this._schoolCourseService = schoolCourseService;
            this._currentUser = currentUser;
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCourseSessionDto>), 200)]
        public async Task<IActionResult> Post(SchoolCourseSessionDto dto)
        {
            var ownedSchoolId = dto.SchoolCourseId > 0 ? (dto.Id > 0 ? await _ownership.SchoolOfSessionAsync(dto.Id, _currentUser.CurrentUser.UserId) : await _ownership.SchoolOfCourseAsync(dto.SchoolCourseId, _currentUser.CurrentUser.UserId)) : null;
            if (!ownedSchoolId.HasValue)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            var result = await _schoolCourseService.UpsertSessionAsync(dto, ownedSchoolId);
            return Ok(result);
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCourseSessionDto>), 200)]
        public async Task<IActionResult> Put(SchoolCourseSessionDto dto)
        {
            var ownedSchoolId = dto.SchoolCourseId > 0 ? (dto.Id > 0 ? await _ownership.SchoolOfSessionAsync(dto.Id, _currentUser.CurrentUser.UserId) : await _ownership.SchoolOfCourseAsync(dto.SchoolCourseId, _currentUser.CurrentUser.UserId)) : null;
            if (!ownedSchoolId.HasValue)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            var result = await _schoolCourseService.UpsertSessionAsync(dto, ownedSchoolId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var ownedSchoolId = await _ownership.SchoolOfSessionAsync(id, _currentUser.CurrentUser.UserId);
            if (!ownedSchoolId.HasValue)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            var result = await _schoolCourseService.DeleteSessionAsync(id, ownedSchoolId);
            return Ok(result);
        }

        /// <summary>
        /// تعیین یک‌جای روزهای دوره: «این دوره n جلسه است» (SessionCount) و روزها/ساعت‌ها انتخاب می‌شوند.
        /// جلسه‌های آینده با این لیست جایگزین می‌شوند؛ جلسه‌های گذشته/دارای حضور و غیاب می‌مانند و جمع باید برابر SessionCount باشد.
        /// </summary>
        [HttpPut("Schedule")]
        [ProducesResponseType(typeof(BaseResultDto<List<SchoolCourseSessionDto>>), 200)]
        public async Task<IActionResult> Schedule(SchoolCourseScheduleDto dto)
        {
            var ownedSchoolId = dto?.SchoolCourseId > 0 ? await _ownership.SchoolOfCourseAsync(dto.SchoolCourseId, _currentUser.CurrentUser.UserId) : null;
            if (!ownedSchoolId.HasValue)
                return Ok(new BaseResultDto(false, Resource.Notification.AccessDenied));
            return Ok(await _schoolCourseService.SetScheduleAsync(dto, ownedSchoolId));
        }
    }
}
