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
    /// <remarks>مشاهده و مدیریت پکیج‌های نام‌دار هر کلینیک؛ دسترسی با RolePermission ناحیه‌ی Admin</remarks>
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

        /// <summary>همه‌ی پکیج‌های نام‌دار یک کلینیک</summary>
        [HttpGet("{companionId}")]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationPackageAdminVDto>>), 200)]
        public async Task<IActionResult> Get(long companionId)
            => Ok(await _service.GetClinicPackagesAsync(companionId));

        /// <summary>ساخت پکیج برای یک کلینیک</summary>
        [HttpPost("{companionId}")]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationPackageAdminVDto>), 200)]
        public async Task<IActionResult> Post(long companionId, [FromBody] ConsultationPackageItemDto dto)
            => Ok(await _service.CreateClinicPackageAsync(companionId, dto));

        /// <summary>ویرایش پکیج یک کلینیک (dto.id الزامی)</summary>
        [HttpPut("{companionId}")]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationPackageAdminVDto>), 200)]
        public async Task<IActionResult> Put(long companionId, [FromBody] ConsultationPackageItemDto dto)
            => Ok(await _service.UpdateClinicPackageAsync(companionId, dto));

        /// <summary>حذف پکیج یک کلینیک</summary>
        [HttpDelete("{companionId}")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long companionId, [FromQuery] long id)
            => Ok(await _service.DeleteClinicPackageAsync(companionId, id));
    }
}
