using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ClinicSrvs.ClinicReserveSrv.Dto;
using Application.Services.ClinicSrvs.ClinicReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// رزرو نوبت کلینیک (کاربر)
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class ClinicReserveController : ControllerBase
    {
        private readonly IClinicReserveService _service;
        private readonly IClinicReserveApprovalService _approval;
        private readonly ICurrentUserHelper _currentUser;

        public ClinicReserveController(IClinicReserveService service, IClinicReserveApprovalService approval, ICurrentUserHelper currentUser)
        {
            _service = service;
            _approval = approval;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        /// <summary>رزروهای من</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ClinicReservePage), 200)]
        public IActionResult Get([FromQuery] ClinicReserveSearchInputDto dto)
        {
            dto.BookerId = UserId;
            dto.OwnerUserId = null;
            dto.ProviderId = null;
            var page = _service.Search(dto);
            foreach (var item in page.List)
            {
                item.ProviderShare = 0;
                item.SiteShare = 0;
                item.CommissionPercent = 0;
                item.Permitted = false;
            }
            return Ok(page);
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicReserveVDto>), 200)]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindAsync(id, UserId, null));

        /// <summary>قیمت بسته (پیش‌پرداخت و باقی‌مانده‌ی حضوری) قبل از رزرو</summary>
        /// <param name="mode">نحوه‌ی ارائه: ۳۷ آنلاین، ۳۸ حضوری در کلینیک، ۳۹ در محل مشتری؛ قیمت هر بسته به آن بستگی دارد.</param>
        /// <param name="packageIds">یک یا چند بسته (رزرو چندخدمتی؛ حداکثر ۵، هر خدمت یک‌بار). packageId قدیمی هم پذیرفته می‌شود.</param>
        [HttpGet("Quote")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicReserveQuoteDto>), 200)]
        public async Task<IActionResult> Quote([FromQuery] List<long> packageIds, int mode, long packageId = 0)
        {
            var ids = (packageIds ?? new List<long>()).ToList();
            if (packageId > 0 && !ids.Contains(packageId))
                ids.Insert(0, packageId);
            return Ok(await _service.QuoteAsync(ids, mode));
        }

        /// <summary>زمان‌های آزاد یک روز برای بسته‌ی انتخابی</summary>
        [HttpGet("Availability")]
        [ProducesResponseType(typeof(BaseResultDto<ClinicAvailabilityDto>), 200)]
        public async Task<IActionResult> Availability(long clinicId, DateTime date, long packageId, long? assigneeUserId = null, [FromQuery] List<long> extraPackageIds = null) =>
            Ok(await _service.GetAvailabilityAsync(clinicId, date, packageId, assigneeUserId, extraPackageIds));

        /// <summary>ثبت رزرو (پرداخت‌نشده)؛ بعد از آن کد تخفیف/کیف پول و پرداخت</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<ClinicReserveVDto>), 200)]
        public async Task<IActionResult> Post(ClinicReserveInputDto dto) => Ok(await _service.CreateAsync(dto, UserId));

        /// <summary>لغو رزرو: تا ۱۲ ساعت قبل از نوبت (یا تا قبل از تأیید کلینیک) با برگشت کل مبلغ به کیف پول</summary>
        [HttpPut("Cancel")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Cancel(long id) => Ok(await _approval.CancelByBookerAsync(id, UserId));
    }
}
