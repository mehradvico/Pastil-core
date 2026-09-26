using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Dto;
using Application.Services.ConsultationSrvs.ConsultationPackageSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Areas.Companion.Controllers
{
    /// <summary>
    /// پکیج‌های نام‌دار «مشاوره آنلاین» (چت / تماس درون‌برنامه / تماس تصویری / تماس تلفنی) توسط مالک کلینیک — جدا از پکیج‌های عادی خدمات
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

        private long? CompanionId
        {
            get
            {
                var id = _currentUserHelper.CurrentUser.CompanionId;
                return id.HasValue && id.Value > 0 ? id : null;
            }
        }

        /// <summary>همه‌ی پکیج‌های کلینیک من (فعال و غیرفعال) به ترتیب کانال، ترتیب نمایش و مدت</summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResultDto<List<ConsultationPackageItemDto>>), 200)]
        public async Task<IActionResult> Get()
        {
            if (CompanionId is not { } companionId)
                return Forbid();

            return Ok(await _service.GetListAsync(companionId));
        }

        /// <summary>ساخت یک پکیج جدید (نام، کانال، مدت از فهرست ثابت، قیمت، توضیح و تصویر اختیاری)</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationPackageItemDto>), 200)]
        public async Task<IActionResult> Post([FromBody] ConsultationPackageItemDto dto)
        {
            if (CompanionId is not { } companionId)
                return Forbid();

            return Ok(await _service.CreateAsync(companionId, dto));
        }

        /// <summary>ویرایش یک پکیج (dto.id الزامی است؛ فعال/غیرفعال کردن هم با همین)</summary>
        [HttpPut]
        [ProducesResponseType(typeof(BaseResultDto<ConsultationPackageItemDto>), 200)]
        public async Task<IActionResult> Put([FromBody] ConsultationPackageItemDto dto)
        {
            if (CompanionId is not { } companionId)
                return Forbid();

            return Ok(await _service.UpdateAsync(companionId, dto));
        }

        /// <summary>حذف پکیج؛ خریدهای قبلی نام و قیمت خودشان را نگه داشته‌اند</summary>
        [HttpDelete]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Delete(long id)
        {
            if (CompanionId is not { } companionId)
                return Forbid();

            return Ok(await _service.DeleteAsync(companionId, id));
        }
    }
}
