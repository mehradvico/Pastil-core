using Application.Services.CompanionSrvs.AssistanceGroupSrv.Dto;
using Application.Services.CompanionSrvs.AssistanceGroupSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// گروه‌های خدمات
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AssistanceGroupController : ControllerBase
    {
        private readonly IAssistanceGroupService assistanceGroupService;

        public AssistanceGroupController(
            IAssistanceGroupService assistanceGroupService)
        {
            this.assistanceGroupService = assistanceGroupService;
        }

        /// <summary>
        /// جستجوی گروه‌های فعال خدمات
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(AssistanceGroupSearchDto), 200)]
        public IActionResult Get([FromQuery] AssistanceGroupInputDto dto)
        {
            dto.Available = true;
            var result = assistanceGroupService.Search(dto);
            return Ok(result);
        }
    }
}
