using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderUserSrv;
using Application.Services.ProviderSrvs.ProviderUserSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// نقش‌های آماده‌ی کارمندان ارائه‌دهنده، زیر هر نمایندگی (Store/Clinic/Groomer/Coach/Pansion/School).
    /// هر ارائه‌دهنده فقط نقش‌های نمایندگی‌هایی را می‌بیند و می‌تواند به کارمندش بدهد که برایش فعال است.
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderUserRoleTemplateController : ControllerBase
    {
        private readonly IProviderUserRoleTemplateService _service;

        public ProviderUserRoleTemplateController(IProviderUserRoleTemplateService service)
        {
            _service = service;
        }

        /// <summary>
        /// فهرست نقش‌های آماده (scope خالی = همه‌ی نمایندگی‌ها)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ProviderUserRoleVDto>>), 200)]
        public async Task<IActionResult> Get([FromQuery] string scope) => Ok(await _service.ListAsync(scope));

        /// <summary>
        /// کاتالوگ کامل دسترسی‌ها (نمایندگی ← بخش ← مشاهده/مدیریت) برای فرم نقش
        /// </summary>
        [HttpGet("Catalog")]
        [ProducesResponseType(typeof(BaseResultDto<List<ProviderPermissionScopeVDto>>), 200)]
        public IActionResult Catalog() => Ok(new BaseResultDto<List<ProviderPermissionScopeVDto>>(true, _service.Catalog()));

        /// <summary>
        /// نقش آماده‌ی جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ProviderUserRoleVDto>), 200)]
        public async Task<IActionResult> Post(ProviderUserRoleTemplateInputDto dto)
        {
            if (dto != null)
                dto.Id = 0;
            return Ok(await _service.SaveAsync(dto));
        }

        /// <summary>
        /// ویرایش نام/توضیح/ترتیب و جایگزینی کامل دسترسی‌ها (نمایندگی عوض نمی‌شود)
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<ProviderUserRoleVDto>), 200)]
        public async Task<IActionResult> Put(ProviderUserRoleTemplateInputDto dto)
        {
            if (dto == null || dto.Id <= 0)
                return Ok(new BaseResultDto<ProviderUserRoleVDto>(false, Resource.Notification.ProviderUserRoleNotFound, null));
            return Ok(await _service.SaveAsync(dto));
        }

        /// <summary>
        /// حذف نقش آماده (اگر به کارمندی داده شده باشد رد می‌شود)
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id) => Ok(await _service.DeleteAsync(id));
    }
}
