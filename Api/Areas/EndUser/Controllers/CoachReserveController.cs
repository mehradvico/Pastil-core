using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CoachSrvs.CoachReserveSrv.Dto;
using Application.Services.CoachSrvs.CoachReserveSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.EndUser.Controllers
{
    /// <summary>
    /// رزرو نوبت مربی (کاربر)
    /// </summary>
    [Area("EndUser")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CoachReserveController : ControllerBase
    {
        private readonly ICoachReserveService _service;
        private readonly ICoachReserveApprovalService _approval;
        private readonly ICurrentUserHelper _currentUser;

        public CoachReserveController(ICoachReserveService service, ICoachReserveApprovalService approval, ICurrentUserHelper currentUser)
        {
            _service = service;
            _approval = approval;
            _currentUser = currentUser;
        }

        private long UserId => _currentUser.CurrentUser.UserId;

        /// <summary>رزروهای من</summary>
        [HttpGet]
        [ProducesResponseType(typeof(CoachReservePage), 200)]
        public IActionResult Get([FromQuery] CoachReserveSearchInputDto dto)
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
        [ProducesResponseType(typeof(BaseResultDto<CoachReserveVDto>), 200)]
        public async Task<IActionResult> Get(long id) => Ok(await _service.FindAsync(id, UserId, null));

        /// <summary>قیمت بسته (پیش‌پرداخت و باقی‌مانده‌ی حضوری) قبل از رزرو</summary>
        /// <param name="mode">نحوه‌ی ارائه: ۳۷ آنلاین، ۳۸ حضوری در مربی، ۳۹ در محل مشتری؛ قیمت هر بسته به آن بستگی دارد.</param>
        /// <param name="packageIds">یک یا چند بسته (رزرو چندخدمتی؛ حداکثر ۵، هر خدمت یک‌بار). packageId قدیمی هم پذیرفته می‌شود.</param>
        [HttpGet("Quote")]
        [ProducesResponseType(typeof(BaseResultDto<CoachReserveQuoteDto>), 200)]
        public async Task<IActionResult> Quote([FromQuery] List<long> packageIds, int mode, long packageId = 0)
        {
            var ids = (packageIds ?? new List<long>()).ToList();
            if (packageId > 0 && !ids.Contains(packageId))
                ids.Insert(0, packageId);
            return Ok(await _service.QuoteAsync(ids, mode));
        }

        /// <summary>زمان‌های آزاد یک روز برای بسته‌ی انتخابی</summary>
        [HttpGet("Availability")]
        [ProducesResponseType(typeof(BaseResultDto<CoachAvailabilityDto>), 200)]
        public async Task<IActionResult> Availability(long coachId, DateTime date, long packageId, long? assigneeUserId = null, [FromQuery] List<long> extraPackageIds = null) =>
            Ok(await _service.GetAvailabilityAsync(coachId, date, packageId, assigneeUserId, extraPackageIds));

        /// <summary>ثبت رزرو (پرداخت‌نشده)؛ بعد از آن کد تخفیف/کیف پول و پرداخت</summary>
        [HttpPost]
        [ProducesResponseType(typeof(BaseResultDto<CoachReserveVDto>), 200)]
        public async Task<IActionResult> Post(CoachReserveInputDto dto) => Ok(await _service.CreateAsync(dto, UserId));

        /// <summary>لغو رزرو: تا ۱۲ ساعت قبل از نوبت (یا تا قبل از تأیید مربی) با برگشت کل مبلغ به کیف پول</summary>
        [HttpPut("Cancel")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> Cancel(long id) => Ok(await _approval.CancelByBookerAsync(id, UserId));
    }
}
