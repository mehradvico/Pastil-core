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
    /// ویدیوهای دوره‌ی مدرسه‌ی خودِ نماینده
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolCourseVideoController : ControllerBase
    {
        private readonly ISchoolCourseService _schoolCourseService;
        private readonly ICurrentUserHelper _currentUser;
        private readonly ISchoolOwnershipService _ownership;
        public SchoolCourseVideoController(ISchoolCourseService schoolCourseService, ICurrentUserHelper currentUser, ISchoolOwnershipService ownership)
        {
            this._ownership = ownership;
            this._schoolCourseService = schoolCourseService;
            this._currentUser = currentUser;
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCourseVideoDto>), 200)]
        public async Task<IActionResult> Post(SchoolCourseVideoDto dto)
        {
            var ownedSchoolId = dto.SchoolCourseId > 0 ? (dto.Id > 0 ? await _ownership.SchoolOfVideoAsync(dto.Id, _currentUser.CurrentUser.UserId) : await _ownership.SchoolOfCourseAsync(dto.SchoolCourseId, _currentUser.CurrentUser.UserId)) : null;
            if (!ownedSchoolId.HasValue)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            var result = await _schoolCourseService.UpsertVideoAsync(dto, ownedSchoolId);
            return Ok(result);
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<SchoolCourseVideoDto>), 200)]
        public async Task<IActionResult> Put(SchoolCourseVideoDto dto)
        {
            var ownedSchoolId = dto.SchoolCourseId > 0 ? (dto.Id > 0 ? await _ownership.SchoolOfVideoAsync(dto.Id, _currentUser.CurrentUser.UserId) : await _ownership.SchoolOfCourseAsync(dto.SchoolCourseId, _currentUser.CurrentUser.UserId)) : null;
            if (!ownedSchoolId.HasValue)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            var result = await _schoolCourseService.UpsertVideoAsync(dto, ownedSchoolId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var ownedSchoolId = await _ownership.SchoolOfVideoAsync(id, _currentUser.CurrentUser.UserId);
            if (!ownedSchoolId.HasValue)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            var result = await _schoolCourseService.DeleteVideoAsync(id, ownedSchoolId);
            return Ok(result);
        }
    }
}
