using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderSrv.Dto;
using Application.Services.ProviderSrvs.ProviderSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// ارائه‌دهندگان
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderController : ControllerBase
    {
        private readonly IProviderService _service;
        public ProviderController(IProviderService service)
        {
            _service = service;
        }

        /// <summary>
        /// اطلاعات آیتم
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<ProviderVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            return Ok(await _service.FindAsyncVDto(id));
        }

        /// <summary>
        /// جستجو
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ProviderSearchDto), 200)]
        public IActionResult Get([FromQuery] ProviderInputDto dto)
        {
            return Ok(_service.Search(dto));
        }

        /// <summary>
        /// آیتم جدید
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ProviderDto>), 200)]
        public async Task<IActionResult> Post(ProviderDto dto)
        {
            return Ok(await _service.InsertAsyncDto(dto));
        }

        /// <summary>
        /// ویرایش آیتم
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(ProviderDto dto)
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
