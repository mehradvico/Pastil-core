using Application.Common.Dto.Result;
using Application.Services.ConsultationSrvs.ConsultationAdminSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationAdminSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// مشاوره‌ی آنلاین کلینیک/مربی مستقل (kind = clinic | coach)
    /// </summary>
    /// <remarks>
    /// بسته‌ها مستقیماً روی ClinicId / CoachId کلید می‌خورند. درصد سهم سایت هر بسته با PUT Admin/FinanceConsultationPackage تنظیم می‌شود؛
    /// خریدهای هر مرکز با GET Admin/ConsultationPurchase?ClinicId= یا ?CoachId=. دسترسی با RolePermission ناحیه‌ی Admin.
    /// </remarks>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsultationVenueController : ControllerBase
    {
        private readonly IConsultationAdminService _service;

        public ConsultationVenueController(IConsultationAdminService service)
        {
            _service = service;
        }

        /// <summary>فهرست کلینیک‌ها یا مربی‌ها با خلاصه‌ی بسته‌های مشاوره</summary>
        [HttpGet("{kind}")]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationVenueAdminVDto>>), 200)]
        public async Task<IActionResult> Get(string kind, [FromQuery] string q)
            => Ok(await _service.GetVenuesAsync(kind, q));

        /// <summary>همه‌ی پکیج‌های مشاوره‌ی یک کلینیک/مربی با درصد سهم سایت</summary>
        [HttpGet("{kind}/{venueId}/Packages")]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationVenuePackageAdminVDto>>), 200)]
        public async Task<IActionResult> GetPackages(string kind, long venueId)
            => Ok(await _service.GetVenuePackagesAsync(kind, venueId));

        /// <summary>حذف پکیج یک کلینیک/مربی (حذف نرم؛ خریدهای قبلی دست‌نخورده می‌مانند)</summary>
        [HttpDelete("{kind}/{venueId}/Packages")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> DeletePackage(string kind, long venueId, [FromQuery] long id)
            => Ok(await _service.DeleteVenuePackageAsync(kind, venueId, id));

        /// <summary>تأیید یا لغو تأیید پکیج یک کلینیک/مربی؛ فقط پکیج تأییدشده به کاربر نمایش داده و فروخته می‌شود</summary>
        [HttpPut("{kind}/{venueId}/Packages/Approve")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> ApprovePackage(string kind, long venueId, [FromQuery] long id, [FromQuery] bool approved = true)
            => Ok(await _service.ApproveVenuePackageAsync(kind, venueId, id, approved));
    }
}
