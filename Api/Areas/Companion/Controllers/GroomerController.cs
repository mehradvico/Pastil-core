using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.GroomerSrvs.GroomerSrv.Dto;
using Application.Services.GroomerSrvs.GroomerSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// آرایشگاه‌های من (مالک/ارائه‌دهنده). همه‌ی عملیات فقط روی آرایشگاه‌های خود کاربر جاری است.
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class GroomerController : ControllerBase
    {
        private readonly IGroomerCoreService _service;
        private readonly ICurrentUserHelper _currentUser;

        public GroomerController(IGroomerCoreService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        /// <summary>لیست آرایشگاه‌های من</summary>
        [HttpGet]
        [ProducesResponseType(typeof(GroomerSearchDto), 200)]
        public IActionResult Get([FromQuery] GroomerInputDto dto)
        {
            dto.OwnerId = UserId;
            dto.ProviderId = null;
            return Ok(_service.Search(dto));
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<GroomerVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _service.FindAsyncVDto(id);
            if (!result.IsSuccess || result.Data.OwnerId != UserId)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(result);
        }

        /// <summary>ثبت آرایشگاه جدید زیر ارائه‌دهنده‌ی خودم (منتظر تأیید ادمین می‌ماند)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<GroomerVDto>), 200)]
        public async Task<IActionResult> Post(GroomerDto dto)
        {
            dto.ProviderId = null;
            return Ok(await _service.CreateAsync(dto, byAdmin: false, UserId));
        }

        /// <summary>ویرایش (دوباره منتظر تأیید ادمین می‌شود)</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(GroomerDto dto) => Ok(await _service.UpdateAsync(dto, UserId));

        [HttpPut("Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Active(GroomerActiveDto dto) => Ok(await _service.UpdateActiveAsync(dto, UserId));

        [HttpPut("Pets")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Pets(GroomerPetsRequestDto dto) => Ok(await _service.SetPetsAsync(dto.GroomerId, dto.PetIds, UserId));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.SoftDeleteAsync(id, UserId, UserId));
    }

    public class GroomerPetsRequestDto
    {
        public long GroomerId { get; set; }
        public List<long> PetIds { get; set; } = new List<long>();
    }
}
