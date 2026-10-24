using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolCourseSrv.Iface;
using Application.Services.Filing.FileSrv.Dto;
using Application.Services.Filing.FileSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Common.Controllers
{
    /// <summary>
    /// فایل
    /// </summary>
    ///
    [Area("Common")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ISchoolCourseService _schoolCourseService;
        private readonly ICurrentUserHelper _currentUser;
        /// <summary>
        /// فایل
        /// </summary>
        ///        
        public FileController(IFileService fileService, ISchoolCourseService schoolCourseService, ICurrentUserHelper currentUser)
        {
            _fileService = fileService;
            _schoolCourseService = schoolCourseService;
            _currentUser = currentUser;
        }
        /// <summary>
        ///  دریافت 
        /// </summary>

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<FileVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            // فایل ویدیوی دوره‌ی مدرسه: فقط خریدار پرداخت‌کرده، نماینده‌ی صاحب مدرسه یا ادمین
            var user = _currentUser.CurrentUser;
            if (!await _schoolCourseService.CanReadLessonFileAsync(id, user.UserId, user.CompanionId, user.RoleEnum == RoleEnum.Admin.ToString()))
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            var item = await _fileService.FindVDtoAsync(id);
            return Ok(item);
        }
    }
}
