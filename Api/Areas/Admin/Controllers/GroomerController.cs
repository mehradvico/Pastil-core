using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.GroomerSrvs.GroomerSrv.Dto;
using Application.Services.GroomerSrvs.GroomerSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// آرایشگاه‌ها (Groomer)
    /// </summary>
    [Area("Admin")]
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

        /// <summary>جستجو</summary>
        /// <param name="deleted">true = فقط آرایشگاه‌های حذف‌شده (حذف نرم)</param>
        [HttpGet]
        [ProducesResponseType(typeof(GroomerSearchDto), 200)]
        public IActionResult Get([FromQuery] GroomerInputDto dto, [FromQuery] bool? deleted = null) =>
            Ok(_service.Search(dto, onlyDeleted: deleted == true));

        /// <summary>اطلاعات کامل یک آرایشگاه (حذف‌شده هم برای ادمین دیده می‌شود)</summary>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<GroomerVDto>), 200)]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindAsyncVDto(id, includeDeleted: true));

        /// <summary>ساخت آرایشگاه (مستقیم تأییدشده و فعال)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<GroomerVDto>), 200)]
        public async Task<IActionResult> Post(GroomerDto dto) =>
            Ok(await _service.CreateAsync(dto, byAdmin: true, _currentUser.CurrentUser.UserId));

        /// <summary>ویرایش اطلاعات آرایشگاه (تأیید، کمیسیون و ... تغییر نمی‌کند)</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(GroomerDto dto) => Ok(await _service.UpdateAsync(dto, ownerUserId: null));

        /// <summary>حذف نرم (رزروها و گزارش مالی می‌مانند)</summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) =>
            Ok(await _service.SoftDeleteAsync(id, ownerUserId: null, _currentUser.CurrentUser.UserId));

        /// <summary>بازگردانی آرایشگاه حذف‌شده (غیرفعال و منتشرنشده برمی‌گردد)</summary>
        [HttpPut("Restore")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Restore(long id) => Ok(await _service.RestoreAsync(id));

        /// <summary>گونه‌های پذیرش‌شده (جایگزین کل لیست)</summary>
        [HttpPut("Pets")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Pets(GroomerPetsDto dto) =>
            Ok(await _service.SetPetsAsync(dto.GroomerId, dto.PetIds, ownerUserId: null));
    }

    public class GroomerPetsDto
    {
        public long GroomerId { get; set; }
        public List<long> PetIds { get; set; } = new List<long>();
    }
}
