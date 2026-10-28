using Application.Services.CompanionSrvs.ExpertiseSrv.Dto;
using Application.Services.CompanionSrvs.ExpertiseSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// تخصص‌ها و عنوان‌های شغلی فعال
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ExpertiseController : ControllerBase
    {
        private readonly IExpertiseService expertiseService;

        public ExpertiseController(IExpertiseService expertiseService)
        {
            this.expertiseService = expertiseService;
        }

        /// <summary>
        /// جستجوی تخصص‌های قابل انتخاب
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ExpertiseSearchDto), 200)]
        public IActionResult Get([FromQuery] ExpertiseInputDto dto)
        {
            dto.Available = true;
            return Ok(expertiseService.Search(dto));
        }

        /// <summary>
        /// گروه‌های تخصص (نقش‌های ارائه‌دهنده: کلینیک، مربی، آرایشگاه، پانسیون، مدرسه) برای فیلتر لیست تخصص‌ها در پنل/اپ نماینده
        /// </summary>
        [HttpGet("groups")]
        [ProducesResponseType(typeof(System.Collections.Generic.List<Application.Services.CompanionSrvs.ExpertiseSrv.Dto.ExpertiseGroupDto>), 200)]
        public async System.Threading.Tasks.Task<IActionResult> Groups() => Ok(await expertiseService.GroupsAsync());
    }
}
