using Application.Common.Dto.Result;
using Application.Services.ConsultationSrvs.ConsultationAdminSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationAdminSrv.Iface;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// بسته‌های مشاوره آنلاین
    /// </summary>
    /// <remarks>مشاهده و ویرایش بسته‌های هر کلینیک؛ دسترسی با RolePermission ناحیه‌ی Admin</remarks>
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

        /// <summary>فهرست کلینیک‌ها با تعداد بسته‌ی فعال و بازه‌ی قیمت</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationClinicAdminVDto>>), 200)]
        public async Task<IActionResult> Get([FromQuery] string q)
            => Ok(await _service.GetClinicsAsync(q));

        /// <summary>ماتریس بسته‌های یک کلینیک</summary>
        [HttpGet("{companionId}")]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationPackageAdminVDto>>), 200)]
        public async Task<IActionResult> Get(long companionId)
            => Ok(await _service.GetClinicPackagesAsync(companionId));

        /// <summary>ویرایش بسته‌های یک کلینیک</summary>
        [HttpPut("{companionId}")]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationPackageAdminVDto>>), 200)]
        public async Task<IActionResult> Put(long companionId, [FromBody] ConsultationPackageSaveDto dto)
            => Ok(await _service.SaveClinicPackagesAsync(companionId, dto));
    }
}
