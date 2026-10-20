using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ClinicSrvs.ClinicSrv.Dto;
using Application.Services.ClinicSrvs.ClinicSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// کلینیک‌ها (Clinic)
    /// </summary>
    [Area("Admin")]
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

        /// <summary>جستجو</summary>
        /// <param name="deleted">true = فقط کلینیک‌های حذف‌شده (حذف نرم)</param>
        [HttpGet]
        [ProducesResponseType(typeof(ClinicSearchDto), 200)]
        public IActionResult Get([FromQuery] ClinicInputDto dto, [FromQuery] bool? deleted = null) =>
            Ok(_service.Search(dto, onlyDeleted: deleted == true));

        /// <summary>اطلاعات کامل یک کلینیک (حذف‌شده هم برای ادمین دیده می‌شود)</summary>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicVDto>), 200)]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindAsyncVDto(id, includeDeleted: true));

        /// <summary>ساخت کلینیک (مستقیم تأییدشده و فعال)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClinicVDto>), 200)]
        public async Task<IActionResult> Post(ClinicDto dto) =>
            Ok(await _service.CreateAsync(dto, byAdmin: true, _currentUser.CurrentUser.UserId));

        /// <summary>ویرایش اطلاعات کلینیک (تأیید، کمیسیون و ... تغییر نمی‌کند)</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(ClinicDto dto) => Ok(await _service.UpdateAsync(dto, ownerUserId: null));

        /// <summary>حذف نرم (رزروها و گزارش مالی می‌مانند)</summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) =>
            Ok(await _service.SoftDeleteAsync(id, ownerUserId: null, _currentUser.CurrentUser.UserId));

        /// <summary>بازگردانی کلینیک حذف‌شده (غیرفعال و منتشرنشده برمی‌گردد)</summary>
        [HttpPut("Restore")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Restore(long id) => Ok(await _service.RestoreAsync(id));

        /// <summary>گونه‌های پذیرش‌شده (جایگزین کل لیست)</summary>
        [HttpPut("Pets")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Pets(ClinicPetsDto dto) =>
            Ok(await _service.SetPetsAsync(dto.ClinicId, dto.PetIds, ownerUserId: null));
    }

    public class ClinicPetsDto
    {
        public long ClinicId { get; set; }
        public List<long> PetIds { get; set; } = new List<long>();
    }
}
