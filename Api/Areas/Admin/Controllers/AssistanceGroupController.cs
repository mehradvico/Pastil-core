using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.AssistanceGroupSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceGroupSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// گروه‌های خدمات
    /// </summary>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class AssistanceGroupController : ControllerBase
    {
        private readonly IAssistanceGroupService assistanceGroupService;

        public AssistanceGroupController(
            IAssistanceGroupService assistanceGroupService)
        {
            this.assistanceGroupService = assistanceGroupService;
        }

        /// <summary>
        /// اطلاعات گروه خدمات
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BaseResultDto<AssistanceGroupVDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            var result = await assistanceGroupService.FindAsyncVDto(id);
            return Ok(result);
        }

        /// <summary>
        /// جستجوی گروه‌های خدمات
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(AssistanceGroupSearchDto), 200)]
        public IActionResult Get([FromQuery] AssistanceGroupInputDto dto)
        {
            var result = assistanceGroupService.Search(dto);
            return Ok(result);
        }

        /// <summary>
        /// ثبت گروه خدمات
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<AssistanceGroupDto>), 200)]
        public async Task<IActionResult> Post(AssistanceGroupDto dto)
        {
            var result = await assistanceGroupService.InsertAsyncDto(dto);
            return Ok(result);
        }

        /// <summary>
        /// ویرایش گروه خدمات
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Put(AssistanceGroupDto dto)
        {
            var result = assistanceGroupService.UpdateDto(dto);
            return Ok(result);
        }

        /// <summary>
        /// حذف گروه خدمات
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public IActionResult Delete(long id)
        {
            var result = assistanceGroupService.DeleteDto(id);
            return Ok(result);
        }
    }
}
