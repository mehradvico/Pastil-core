using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationVenueSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// پکیج‌های «مشاوره آنلاین» (چت / تماس) کلینیک با شناسه‌ی خودِ مرکز؛ همان سامانه‌ی مشاوره‌ی Companion با لنگر خودکار (جزئیات: backend/Docs/COACH_FA.md)
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicConsultationPackageController : ControllerBase
    {
        private readonly IVenueConsultationService _service;
        private readonly ICurrentUserHelper _currentUser;

        public ClinicConsultationPackageController(IVenueConsultationService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        /// <summary>پکیج‌های مشاوره‌ی این مرکز (فعال و غیرفعال)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationPackageItemDto>>), 200)]
        public async Task<IActionResult> Get([FromQuery] long clinicId) => Ok(await _service.GetListAsync(VenueKind.Clinic, clinicId, UserId));

        /// <summary>ساخت پکیج جدید (نام، کانال، مدت ثابت، قیمت، ...)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationPackageItemDto>), 200)]
        public async Task<IActionResult> Post([FromQuery] long clinicId, [FromBody] ConsultationPackageItemDto dto) => Ok(await _service.CreateAsync(VenueKind.Clinic, clinicId, UserId, dto));

        /// <summary>ویرایش پکیج (dto.id الزامی؛ فعال/غیرفعال هم همین‌جا)</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationPackageItemDto>), 200)]
        public async Task<IActionResult> Put([FromQuery] long clinicId, [FromBody] ConsultationPackageItemDto dto) => Ok(await _service.UpdateAsync(VenueKind.Clinic, clinicId, UserId, dto));

        /// <summary>حذف نرم پکیج</summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete([FromQuery] long clinicId, [FromQuery] long id) => Ok(await _service.DeleteAsync(VenueKind.Clinic, clinicId, UserId, id));
    }
}
