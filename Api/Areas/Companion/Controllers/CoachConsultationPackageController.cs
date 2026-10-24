using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationVenueSrv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// پکیج‌های «مشاوره آنلاین» (چت / تماس) مربی با شناسه‌ی خودِ مرکز؛ همان سامانه‌ی مشاوره‌ی Companion با لنگر خودکار (جزئیات: backend/Docs/COACH_FA.md)
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachConsultationPackageController : ControllerBase
    {
        private readonly IVenueConsultationService _service;
        private readonly ICurrentUserHelper _currentUser;

        public CoachConsultationPackageController(IVenueConsultationService service, ICurrentUserHelper currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        /// <summary>پکیج‌های مشاوره‌ی این مرکز (فعال و غیرفعال)</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationPackageItemDto>>), 200)]
        public async Task<IActionResult> Get([FromQuery] long coachId) => Ok(await _service.GetListAsync(VenueKind.Coach, coachId, UserId));

        /// <summary>ساخت پکیج جدید (نام، کانال، مدت ثابت، قیمت، ...)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationPackageItemDto>), 200)]
        public async Task<IActionResult> Post([FromQuery] long coachId, [FromBody] ConsultationPackageItemDto dto) => Ok(await _service.CreateAsync(VenueKind.Coach, coachId, UserId, dto));

        /// <summary>ویرایش پکیج (dto.id الزامی؛ فعال/غیرفعال هم همین‌جا)</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationPackageItemDto>), 200)]
        public async Task<IActionResult> Put([FromQuery] long coachId, [FromBody] ConsultationPackageItemDto dto) => Ok(await _service.UpdateAsync(VenueKind.Coach, coachId, UserId, dto));

        /// <summary>حذف نرم پکیج</summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete([FromQuery] long coachId, [FromQuery] long id) => Ok(await _service.DeleteAsync(VenueKind.Coach, coachId, UserId, id));
    }
}
