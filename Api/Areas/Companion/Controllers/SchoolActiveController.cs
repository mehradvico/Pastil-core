using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolSrv.Dto;
using Application.Services.SchoolSrvs.SchoolSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// فعال/غیرفعال کردن مدرسه‌ی خودِ نماینده
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolActiveController : ControllerBase
    {
        private readonly ISchoolService _schoolService;
        private readonly ICurrentUserHelper _currentUser;
        public SchoolActiveController(ISchoolService schoolService, ICurrentUserHelper currentUser)
        {
            this._schoolService = schoolService;
            this._currentUser = currentUser;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(SchoolActiveDto dto)
        {
            var result = _schoolService.UpdateSchoolActiveDto(dto, _currentUser.CurrentUser.CompanionId);
            return Ok(result);
        }
    }
}
