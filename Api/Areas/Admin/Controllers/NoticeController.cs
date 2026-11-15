using Api.Authorization;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CommonSrv.PushSubscriptionSrv.Dto;
using Application.Services.Setting.NoticeSrv.Dto;
using Application.Services.Setting.NoticeSrv.Iface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Areas.Admin.Controllers
{
    /// <summary>
    /// اعلان ها
    /// </summary>
    ///
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    [Authorize(Policy = PolicyNames.AdminOnly)]
    public class NoticeController : ControllerBase
    {
        private readonly INoticeService _noticeService;
        private readonly IPanelNoticePushService _panelPush;
        private readonly ICurrentUserHelper _currentUser;

        public NoticeController(INoticeService noticeService, IPanelNoticePushService panelPush, ICurrentUserHelper currentUser)
        {
            _noticeService = noticeService;
            _panelPush = panelPush;
            _currentUser = currentUser;
        }

        ///<summary>
        ///جستجو 
        /// </summary>
        ///<returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(NoticeSearchDto), 200)]
        public IActionResult Get([FromQuery] NoticeInputDto dto)
        {
            return Ok(_noticeService.Search(dto));
        }
        
        ///<summary>
        ///نوع آیتم
        /// </summary>
        ///<returns></returns>
        [HttpGet("types")]
        [ProducesResponseType(typeof(List<NoticeTypeVDto>), 200)]
        public async Task<IActionResult> GetTypes(bool activeOnly = true)
        {
            return Ok(await _noticeService.GetTypesAsync(activeOnly));
        }

        ///<summary>
        ///تعداد آیتم های خوانده نشده
        /// </summary>
        ///<returns></returns>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            return Ok(await _noticeService.GetUnreadCountAsync());
        }

        ///<summary>
        ///اطلاعات آیتم
        /// </summary>
        ///<returns></returns>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(BaseResultDto<NoticeDto>), 200)]
        public async Task<IActionResult> Get(long id)
        {
            return Ok(await _noticeService.FindAsyncDto(id));
        }

        ///<summary>
        ///خواندن آیتم
        /// </summary>
        ///<returns></returns>
        [HttpPost("{id:long}/read")]
        [ProducesResponseType(typeof(BaseResultDto<NoticeDto>), 200)]
        public async Task<IActionResult> Read(long id)
        {
            return Ok(await _noticeService.ReadAsync(id));
        }

        ///<summary>
        ///خواندن همه آیتم ها
        /// </summary>
        ///<returns></returns>
        [HttpPost("read/bulk")]
        [ProducesResponseType(typeof(BaseResultDto<NoticeBulkReadVDto>), 200)]
        public async Task<IActionResult> ReadBulk([FromBody] NoticeBulkReadDto dto)
        {
            return Ok(await _noticeService.ReadBulkAsync(dto));
        }

        ///<summary>
        ///فعال‌سازی پوش سیستمی اعلان‌ها روی همین دستگاه/مرورگرِ ادمین
        /// </summary>
        [HttpPost("push/subscribe")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PushSubscribe([FromBody] PushSubscribeDto dto)
        {
            return Ok(await _panelPush.SubscribeAsync(_currentUser.CurrentUser.UserId, dto));
        }

        ///<summary>
        ///غیرفعال‌سازی پوش سیستمی اعلان‌ها روی همین دستگاه/مرورگر
        /// </summary>
        [HttpPost("push/unsubscribe")]
        [ProducesResponseType(typeof(BaseResultDto), 200)]
        public async Task<IActionResult> PushUnsubscribe([FromBody] PanelPushUnsubscribeDto dto)
        {
            return Ok(await _panelPush.UnsubscribeAsync(_currentUser.CurrentUser.UserId, dto?.Endpoint));
        }
    }
}
