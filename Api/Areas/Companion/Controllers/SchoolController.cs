using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.SchoolSrvs.SchoolSrv.Dto;
using Application.Services.SchoolSrvs.SchoolSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مدیریت مدرسه‌ی خودِ نماینده (مربی)
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolController : ControllerBase
    {
        private readonly ISchoolService _schoolService;
        private readonly ICurrentUserHelper _currentUser;
        public SchoolController(ISchoolService schoolService, ICurrentUserHelper currentUser)
        {
            this._schoolService = schoolService;
            this._currentUser = currentUser;
        }

        [HttpGet()]
        [ProducesResponseType(typeof(SchoolSearchDto), 200)]
        public IActionResult Get([FromQuery] SchoolInputDto dto)
        {
            dto.CompanionId = _currentUser.CurrentUser.CompanionId;
            var search = _schoolService.Search(dto);
            return Ok(search);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var school = await _schoolService.FindAsyncVDto(id);
            if (!school.IsSuccess ||
                !_currentUser.CurrentUser.CompanionId.HasValue ||
                school.Data?.CompanionId != _currentUser.CurrentUser.CompanionId.Value)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(school);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<SchoolDto>), 200)]
        public async Task<IActionResult> Post(SchoolDto dto)
        {
            if (!_currentUser.CurrentUser.CompanionId.HasValue)
                return Forbid();
            dto.CompanionId = _currentUser.CurrentUser.CompanionId.Value;
            var result = await _schoolService.InsertAsyncDto(dto);
            return Ok(result);
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(SchoolDto dto)
        {
            if (!_currentUser.CurrentUser.CompanionId.HasValue)
                return Forbid();

            var existing = await _schoolService.FindAsyncVDto(dto.Id);
            if (!existing.IsSuccess || existing.Data?.CompanionId != _currentUser.CurrentUser.CompanionId.Value)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));

            dto.CompanionId = _currentUser.CurrentUser.CompanionId.Value;
            var result = _schoolService.UpdateDto(dto);
            return Ok(result);
        }
    }
}
