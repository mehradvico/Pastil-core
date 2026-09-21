using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// تعریف پکیج‌های «مشاوره آنلاین» (کانال × مدت) توسط مالک کلینیک — جدا از پکیج‌های عادی خدمات
    /// </summary>
    [Area("Companion")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ConsultationPackageController : ControllerBase
    {
        private readonly IConsultationPackageService _service;
        private readonly ICurrentUserHelper _currentUserHelper;

        public ConsultationPackageController(IConsultationPackageService service, ICurrentUserHelper currentUserHelper)
        {
            _service = service;
            _currentUserHelper = currentUserHelper;
        }

        /// <summary>ماتریس کامل ۸ خانه‌ای پکیج‌های کلینیک من</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationPackageItemDto>>), 200)]
        public async Task<IActionResult> Get()
        {
            var companionId = _currentUserHelper.CurrentUser.CompanionId;
            if (!companionId.HasValue || companionId.Value <= 0)
                return Forbid();

            return Ok(await _service.GetMatrixAsync(companionId.Value));
        }

        /// <summary>ذخیره‌ی ماتریس پکیج‌ها (قیمت + فعال/غیرفعال هر خانه)</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationPackageItemDto>>), 200)]
        public async Task<IActionResult> Put([FromBody] ConsultationPackageSaveDto dto)
        {
            var companionId = _currentUserHelper.CurrentUser.CompanionId;
            if (!companionId.HasValue || companionId.Value <= 0)
                return Forbid();

            return Ok(await _service.SaveMatrixAsync(companionId.Value, dto));
        }
    }
}
