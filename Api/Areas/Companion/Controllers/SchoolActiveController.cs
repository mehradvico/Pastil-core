using Application.Services.SchoolSrvs.SchoolOwnershipSrv;
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
        private readonly ISchoolOwnershipService _ownership;
        public SchoolActiveController(ISchoolService schoolService, ICurrentUserHelper currentUser, ISchoolOwnershipService ownership)
        {
            this._ownership = ownership;
            this._schoolService = schoolService;
            this._currentUser = currentUser;
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(SchoolActiveDto dto)
        {
            var companionId = await _ownership.CompanionOfSchoolAsync(dto.Id, _currentUser.CurrentUser.UserId);
            if (!companionId.HasValue)
                return Forbid();
            var result = _schoolService.UpdateSchoolActiveDto(dto, companionId);
            return Ok(result);
        }
    }
}
