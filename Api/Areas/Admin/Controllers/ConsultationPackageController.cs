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
    /// بسته‌های مشاوره آنلاین
    /// </summary>
    /// <remarks>فقط خواندن؛ دسترسی با RolePermission ناحیه‌ی Admin</remarks>
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsultationPackageController : ControllerBase
    {
        private readonly IConsultationAdminService _service;

        public ConsultationPackageController(IConsultationAdminService service)
        {
            _service = service;
        }

        /// <summary>کلینیک‌های دارای بسته‌ی فعال + بازه‌ی قیمت + تعداد خرید</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationClinicAdminVDto>>), 200)]
        public async Task<IActionResult> Get()
            => Ok(await _service.GetClinicsAsync());

        /// <summary>ماتریس بسته‌های یک کلینیک</summary>
        [HttpGet("{companionId}")]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationPackageAdminVDto>>), 200)]
        public async Task<IActionResult> Get(long companionId)
            => Ok(await _service.GetClinicPackagesAsync(companionId));
    }
}
