using Application.Common.Dto.Result;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.CommonSrv.PushInboxSrv.Dto
{
    public class NotificationInboxSearchDto : BaseSearchDto<PushNotification, NotificationInboxVDto>
    {
        public NotificationInboxSearchDto(NotificationInboxInputDto dto, IQueryable<PushNotification> list, IMapper mapper)
            : base(dto, list, mapper)
        {
            UnreadOnly = dto.UnreadOnly;
        }

        public bool UnreadOnly { get; set; }

        /// <summary>تعداد کل خوانده‌نشده‌ها (مستقل از صفحه‌بندی) تا اپ بِج را بدون درخواست دوم بسازد.</summary>
        public int UnreadCount { get; set; }
    }
}
