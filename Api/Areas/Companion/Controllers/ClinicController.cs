using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ClinicSrvs.ClinicSrv.Dto;
using Application.Services.ClinicSrvs.ClinicSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// کلینیک‌های من (مالک/ارائه‌دهنده). همه‌ی عملیات فقط روی کلینیک‌های خود کاربر جاری است.
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicController : ControllerBase
    {
        private readonly IClinicCoreService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ClinicController(IClinicCoreService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        /// <summary>لیست کلینیک‌های من</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ClinicSearchDto), 200)]
        public IActionResult Get([FromQuery] ClinicInputDto dto)
        {
            dto.OwnerId = UserId;
            dto.ProviderId = null;
            return Ok(_service.Search(dto));
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _service.FindAsyncVDto(id);
            if (!result.IsSuccess || result.Data.OwnerId != UserId)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            return Ok(result);
        }

        /// <summary>ثبت کلینیک جدید زیر ارائه‌دهنده‌ی خودم (منتظر تأیید ادمین می‌ماند)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClinicVDto>), 200)]
        public async Task<IActionResult> Post(ClinicDto dto)
        {
            dto.ProviderId = null;
            return Ok(await _service.CreateAsync(dto, byAdmin: false, UserId));
        }

        /// <summary>ویرایش (دوباره منتظر تأیید ادمین می‌شود)</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(ClinicDto dto) => Ok(await _service.UpdateAsync(dto, UserId));

        [HttpPut("Active")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Active(ClinicActiveDto dto) => Ok(await _service.UpdateActiveAsync(dto, UserId));

        [HttpPut("Pets")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Pets(ClinicPetsRequestDto dto) => Ok(await _service.SetPetsAsync(dto.ClinicId, dto.PetIds, UserId));

        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.SoftDeleteAsync(id, UserId, UserId));
    }

    public class ClinicPetsRequestDto
    {
        public long ClinicId { get; set; }
        public List<long> PetIds { get; set; } = new List<long>();
    }
}
