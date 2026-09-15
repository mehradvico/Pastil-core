using Application.Common.Dto.Input;

namespace Application.Services.CommonSrv.PushInboxSrv.Dto
{
    public class NotificationInboxInputDto : BaseInputDto
    {
        /// <summary>فقط اعلان‌های خوانده‌نشده.</summary>
        public bool UnreadOnly { get; set; }
    }
}
