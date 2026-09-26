using Application.Common.Dto.Result;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// پکیج‌های قابل خرید «مشاوره آنلاین» یک کلینیک (فقط فعال و با قیمت مثبت)
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsultationPackageController : ControllerBase
    {
        private readonly IConsultationPackageService _service;

        public ConsultationPackageController(IConsultationPackageService service)
        {
            _service = service;
        }

        // فهرست بسته‌ها اطلاعات عمومی کلینیک است (نام، روش، مدت، قیمت)؛ مهمان هم می‌تواند ببیند. خرید همچنان نیازمند ورود است.
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationPackagePublicVDto>>), 200)]
        public async Task<IActionResult> Get([FromQuery] long companionId)
            => Ok(await _service.GetPublicAsync(companionId));
    }
}
