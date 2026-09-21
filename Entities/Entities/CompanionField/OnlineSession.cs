using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    // جلسه‌ی آنلاین بدون رزرو: نماینده/اپراتور از پنل کاری خودش برای یک کاربر جلسه (چت/تماس/ویدیو کال) باز می‌کند
    // و به کاربر پوش می‌رود. برخلاف CompanionReserve، مبلغ/نوبت/پرداخت ندارد.
    public class OnlineSession : Id_Field
    {
        public long InitiatorUserId { get; set; }
        public long TargetUserId { get; set; }
        // مقدار OnlineSessionChannelEnum (چت، تماس درون‌برنامه، تماس تصویری، تماس تلفنی)
        public int ChannelId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? EndDate { get; set; }
        // جلسه‌ی مدت‌دار مشاوره (خرید پکیج): پایان پنجره‌ی زمانی و خرید مربوط؛ نال = جلسه‌ی آزاد بدون محدودیت زمانی.
        // ConsultationPurchaseId عمداً FK ندارد (حلقه‌ی وابستگی با ConsultationPurchase.OnlineSessionId).
        public DateTime? ExpireDate { get; set; }
        public long? ConsultationPurchaseId { get; set; }

        public User InitiatorUser { get; set; }
        public User TargetUser { get; set; }
        public ICollection<OnlineSessionMessage> Messages { get; set; }
    }
}
