using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CoachSrvs.CoachSrv.Dto;
using Application.Services.CoachSrvs.CoachSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مربی‌ها (Coach)
    /// </summary>
    [Area("Admin")]
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

        /// <summary>جستجو</summary>
        /// <param name="deleted">true = فقط مربی‌های حذف‌شده (حذف نرم)</param>
        [HttpGet]
        [ProducesResponseType(typeof(CoachSearchDto), 200)]
        public IActionResult Get([FromQuery] CoachInputDto dto, [FromQuery] bool? deleted = null) =>
            Ok(_service.Search(dto, onlyDeleted: deleted == true));

        /// <summary>اطلاعات کامل یک مربی (حذف‌شده هم برای ادمین دیده می‌شود)</summary>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<CoachVDto>), 200)]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindAsyncVDto(id, includeDeleted: true));

        /// <summary>ساخت مربی (مستقیم تأییدشده و فعال)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CoachVDto>), 200)]
        public async Task<IActionResult> Post(CoachDto dto) =>
            Ok(await _service.CreateAsync(dto, byAdmin: true, _currentUser.CurrentUser.UserId));

        /// <summary>ویرایش اطلاعات مربی (تأیید، کمیسیون و ... تغییر نمی‌کند)</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(CoachDto dto) => Ok(await _service.UpdateAsync(dto, ownerUserId: null));

        /// <summary>حذف نرم (رزروها و گزارش مالی می‌مانند)</summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) =>
            Ok(await _service.SoftDeleteAsync(id, ownerUserId: null, _currentUser.CurrentUser.UserId));

        /// <summary>بازگردانی مربی حذف‌شده (غیرفعال و منتشرنشده برمی‌گردد)</summary>
        [HttpPut("Restore")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Restore(long id) => Ok(await _service.RestoreAsync(id));

        /// <summary>گونه‌های پذیرش‌شده (جایگزین کل لیست)</summary>
        [HttpPut("Pets")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Pets(CoachPetsDto dto) =>
            Ok(await _service.SetPetsAsync(dto.CoachId, dto.PetIds, ownerUserId: null));
    }

    public class CoachPetsDto
    {
        public long CoachId { get; set; }
        public List<long> PetIds { get; set; } = new List<long>();
    }
}
