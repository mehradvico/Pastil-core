using Application.Common.Dto.Result;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationVenueSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    /// <summary>
    /// پکیج‌های قابل خرید «مشاوره آنلاین» (چت / تماس) یک کلینیک با شناسه‌ی مرکز؛ خرید با همان EndUser/ConsultationPurchase (packageId)
    /// </summary>
    [Route("api/Clinic")]
    [ApiController]
    public class ClinicConsultationPackagesController : ControllerBase
    {
        private readonly IVenueConsultationService _service;

        public ClinicConsultationPackagesController(IVenueConsultationService service) => _service = service;

        [HttpGet("{id:long}/ConsultationPackages")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationPackagePublicVDto>>), 200)]
        public async Task<IActionResult> Get(long id) => Ok(await _service.GetPublicAsync(VenueKind.Clinic, id));
    }
}
