using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderRoleAssignmentSrv.Dto;
using Application.Services.ProviderSrvs.ProviderRoleAssignmentSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// تخصص‌های ارائه‌دهنده
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderRoleAssignmentController : ControllerBase
    {
        private readonly IProviderRoleAssignmentService _service;
        public ProviderRoleAssignmentController(IProviderRoleAssignmentService service)
        {
            _service = service;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ProviderRoleAssignmentDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            return Ok(await _service.FindAsyncDto(id));
        }

        /// <summary>
        /// جستجو
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ProviderRoleAssignmentSearchDto), 200)]
        public IActionResult Get([FromQuery] ProviderRoleAssignmentInputDto dto)
        {
            return Ok(_service.Search(dto));
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ProviderRoleAssignmentDto>), 200)]
        public async Task<IActionResult> Post(ProviderRoleAssignmentDto dto)
        {
            return Ok(await _service.InsertAsyncDto(dto));
        }

        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(ProviderRoleAssignmentDto dto)
        {
            return Ok(_service.UpdateDto(dto));
        }

        /// <summary>
        /// حذف آیتم
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            return Ok(_service.DeleteDto(id));
        }
    }
}
