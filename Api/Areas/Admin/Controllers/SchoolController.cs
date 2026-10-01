using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolSrv.Dto;
using Application.Services.SchoolSrvs.SchoolSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مدرسه ها
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class SchoolController : ControllerBase
    {
        private readonly ISchoolService _schoolService;
        private readonly Application.Common.Interface.ICurrentUserHelper _currentUser;
        public SchoolController(ISchoolService schoolService, Application.Common.Interface.ICurrentUserHelper currentUser)
        {
            this._schoolService = schoolService;
            this._currentUser = currentUser;
        }

        [HttpGet()]
        [ProducesResponseType(typeof(SchoolSearchDto), 200)]
        /// <param name="deleted">true = فقط مدرسه‌های حذف‌شده (حذف نرم)؛ پیش‌فرض: حذف‌شده‌ها نمایش داده نمی‌شوند</param>
        public IActionResult Get([FromQuery] SchoolInputDto dto, [FromQuery] bool? deleted = null)
        {
            var search = _schoolService.Search(dto, onlyDeleted: deleted == true);
            return Ok(search);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<SchoolVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            // ادمین مدرسه‌ی حذف‌شده را هم می‌بیند (Deleted/DeleteDate در پاسخ)
            var school = await _schoolService.FindAsyncVDto(id, includeDeleted: true);
            return Ok(school);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<SchoolDto>), 200)]
        public async Task<IActionResult> Post(SchoolDto dto)
        {
            var result = await _schoolService.InsertAsyncDto(dto);
            return Ok(result);
        }

        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(SchoolDto dto)
        {
            // UpdateDto عمومی کل ردیف را بازنویسی می‌کند و Deleted را false می‌کرد؛ مدرسه‌ی حذف‌شده را اول با Restore برگردانید
            var existing = await _schoolService.FindAsyncVDto(dto.Id);
            if (!existing.IsSuccess)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            var school = _schoolService.UpdateDto(dto);
            return Ok(school);
        }

        /// <summary>
        /// حذف نرم مدرسه توسط ادمین (دوره‌ها، رزروها و گزارش مالی می‌مانند؛ با ثبت‌نام باز پرداخت‌شده حذف نمی‌شود)
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _schoolService.SoftDeleteAsync(id, null, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }

        /// <summary>
        /// بازگردانی مدرسه‌ی حذف‌شده (غیرفعال و منتشرنشده برمی‌گردد)
        /// </summary>
        [HttpPut("Restore")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Restore(long id)
        {
            var result = await _schoolService.RestoreAsync(id);
            return Ok(result);
        }
    }
}
