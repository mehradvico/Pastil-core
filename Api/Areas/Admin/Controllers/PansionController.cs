using Application.Common.Dto.Result;
using Application.Services.PansionSrvs.PansionSrv.Dto;
using Application.Services.PansionSrvs.PansionSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// پانسیون ها
    /// </summary>
    /// 
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class PansionController : ControllerBase
    {
        private readonly IPansionService _PansionService;
        private readonly Application.Common.Interface.ICurrentUserHelper _currentUser;
        public PansionController(IPansionService PansionService, Application.Common.Interface.ICurrentUserHelper currentUser)
        {
            this._PansionService = PansionService;
            this._currentUser = currentUser;
        }

        /// <summary>
        /// جستجو
        /// </summary>
        /// <returns></returns> 
        [HttpGet()]
        [ProducesResponseType(typeof(PansionSearchDto), 200)]
        /// <param name="deleted">true = فقط پانسیون‌های حذف‌شده (حذف نرم)؛ پیش‌فرض: حذف‌شده‌ها نمایش داده نمی‌شوند</param>
        public IActionResult Get([FromQuery] PansionInputDto dto, [FromQuery] bool? deleted = null)
        {
            var search = _PansionService.Search(dto, onlyDeleted: deleted == true);
            return Ok(search);
        }


        /// <summary>
        ///  اطلاعات آیتم 
        /// </summary>
        /// <param name="id">شناسه پانسیون</param>
        /// <returns>
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<PansionDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            // ادمین پانسیون حذف‌شده را هم می‌بیند (Deleted/DeleteDate در پاسخ)
            var Pansion = await _PansionService.FindAsyncVDto(id, includeDeleted: true);
            return Ok(Pansion);
        }


        /// <summary>
        /// آیتم جدید
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<PansionDto>), 200)]
        public async Task<IActionResult> Post(PansionDto dto)
        {
            var result = await _PansionService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        ///  ویرایش آیتم 
        /// </summary>
        /// <returns>
        /// </returns>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Put(PansionDto dto)
        {
            // UpdateDto عمومی کل ردیف را بازنویسی می‌کند و Deleted را false می‌کرد؛ پانسیون حذف‌شده را اول با Restore برگردانید
            var existing = await _PansionService.FindAsyncVDto(dto.Id);
            if (!existing.IsSuccess)
                return NotFound(new BaseResultDto(false, Resource.Notification.NothingFound));
            var Pansion = _PansionService.UpdateDto(dto);
            return Ok(Pansion);
        }

        /// <summary>
        /// حذف نرم پانسیون توسط ادمین (رزروها و گزارش مالی می‌مانند؛ با رزرو باز پرداخت‌شده حذف نمی‌شود)
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _PansionService.SoftDeleteAsync(id, null, _currentUser.CurrentUser.UserId);
            return Ok(result);
        }

        /// <summary>
        /// بازگردانی پانسیون حذف‌شده (غیرفعال و منتشرنشده برمی‌گردد)
        /// </summary>
        [HttpPut("Restore")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Restore(long id)
        {
            var result = await _PansionService.RestoreAsync(id);
            return Ok(result);
        }
    }
}
