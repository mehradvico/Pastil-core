using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderRoleSrv.Dto;
using Application.Services.ProviderSrvs.ProviderRoleSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// نقش های نمایندگان
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderRoleController : ControllerBase
    {
        private readonly IProviderRoleService _service;
        public ProviderRoleController(IProviderRoleService service)
        {
            _service = service;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ProviderRoleDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            return Ok(await _service.FindAsyncDto(id));
        }

        /// <summary>
        /// جستجو
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ProviderRoleSearchDto), 200)]
        public IActionResult Get([FromQuery] ProviderRoleInputDto dto)
        {
            return Ok(_service.Search(dto));
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ProviderRoleDto>), 200)]
        public async Task<IActionResult> Post(ProviderRoleDto dto)
        {
            return Ok(await _service.InsertAsyncDto(dto));
        }

        /// <summary>
        /// ویرایش آیتم (فقط نام و فعال بودن)
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(ProviderRoleDto dto)
        {
            return Ok(_service.UpdateDto(dto));
        }

        /// <summary>
        /// حذف آیتم (اگر به ارائه‌دهنده‌ای داده شده باشد رد می‌شود)
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            return Ok(_service.DeleteDto(id));
        }
    }
}
