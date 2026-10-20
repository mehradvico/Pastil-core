using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CoachSrvs.CoachSrv.Dto;
using Application.Services.CoachSrvs.CoachSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// مربی‌های من (مالک/ارائه‌دهنده). همه‌ی عملیات فقط روی مربی‌های خود کاربر جاری است.
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachController : ControllerBase
    {
        private readonly ICoachCoreService _service;
        private readonly ICurrentUserHelper _currentUser;

        public CoachController(ICoachCoreService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        /// <summary>لیست مربی‌های من</summary>
        [HttpGet]
        [ProducesResponseType(typeof(CoachSearchDto), 200)]
        public IActionResult Get([FromQuery] CoachInputDto dto)
        {
            dto.OwnerId = UserId;
            dto.ProviderId = null;
            return Ok(_service.Search(dto));
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<CoachVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _service.FindAsyncVDto(id);
            if (!result.IsSuccess || result.Data.OwnerId != UserId)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(result);
        }

        /// <summary>ثبت مربی جدید زیر ارائه‌دهنده‌ی خودم (منتظر تأیید ادمین می‌ماند)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CoachVDto>), 200)]
        public async Task<IActionResult> Post(CoachDto dto)
        {
            dto.ProviderId = null;
            return Ok(await _service.CreateAsync(dto, byAdmin: false, UserId));
        }

        /// <summary>ویرایش (دوباره منتظر تأیید ادمین می‌شود)</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CoachDto dto) => Ok(await _service.UpdateAsync(dto, UserId));

        [HttpPut("Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Active(CoachActiveDto dto) => Ok(await _service.UpdateActiveAsync(dto, UserId));

        [HttpPut("Pets")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Pets(CoachPetsRequestDto dto) => Ok(await _service.SetPetsAsync(dto.CoachId, dto.PetIds, UserId));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.SoftDeleteAsync(id, UserId, UserId));
    }

    public class CoachPetsRequestDto
    {
        public long CoachId { get; set; }
        public List<long> PetIds { get; set; } = new List<long>();
    }
}
