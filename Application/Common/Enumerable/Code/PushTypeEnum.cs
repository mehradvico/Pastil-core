using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Enumerable.Code
{
    public enum PushTypeEnum
    {
        PushSignUpUser = 1,
        PushSignUpAdmin = 3,
        PushRegisterOrderUser = 4,
        PushProccessOrderUser = 5,
        PushSentOrderUser = 6,
        PushRegisterOrderStore = 7,
        PushRegisterOrderAdmin = 8,
        PushSentOrderAdmin = 9,
        PushRegisterReserveUser = 10,
        PushCompleteReserveUser = 11,
        PushCancelReserveUser = 12,
        PushRegisterReserveCompanion = 13,
        PushCancelReserveCompanion = 14,
        PushRegisterReserveAdmin = 15,
        PushCompleteReserveAdmin = 16,
        PushCancelReserveAdmin = 17,
        PushRegisterPansionUser = 18,
        PushCompletePansionUser = 19,
        PushRegisterPansionCompanion = 20,
        PushRegisterPansionAdmin = 21,
        PushCompletePansionAdmin = 22,
        PushMemoryReminder = 23,
        PushReminderOneWeekBefore = 24,
        PushReminderOneDayBefore = 25,
        PushReminderOneDayAfter = 26,
        PushPastilMatchRequestReceived = 27,
        PushPastilMatchRequestAccepted = 28,
        PushPastilMatchRequestRejected = 29,
        PushPastilMatchNewMessage = 30,
        PushPastilMatchProfileLiked = 31,
        PushPastilMatchClosed = 32,
        PushPastilMatchVerificationApproved = 33,
        PushPastilMatchVerificationRejected = 34,
        PushPastilMatchMessageReaction = 35,
        PushPastilMatchRequestCancelled = 36,
        PushCompanionRequestApproved = 37,
        PushCompanionRequestRejected = 38,
        PushDriverRequestApproved = 39,
        PushDriverRequestRejected = 40,
        PushStoreRequestApproved = 41,
        PushStoreRequestRejected = 42,
        PushPansionRequestApproved = 43,
        PushPansionRequestRejected = 44,
        PushCompanionReserveAssigned = 45,
        PushCompanionReserveReviewReminder = 46,
        PushPansionReserveReviewReminder = 47,
        PushProductStockAvailable = 48,
        PushTripArrivedOrigin = 49,
        PushTripPetPickedUp = 50,
        PushTripArrivedDestination = 51,
        PushTripCompleted = 52,
        PushTripCanceled = 53,
        PushTripRequestAvailable = 54,
        PushTripDriverCanceled = 55,
        PushPetBirthday = 56,
        PushPetBirthdayUpcoming = 57,
        PushTripUserCanceled = 58,
        PushOnlineReserveConfirmedUser = 59,
        PushOnlineReserveReminderBeforeCompanion = 60,
        PushOnlineReserveReminderAtTimeCompanion = 61,
        PushInAppCallStarted = 63,
        PushSchoolClassStarting = 64,
        PushCompanionReserveNewMessage = 65,
        PushOnlineSessionChatInvite = 66,
        PushOnlineSessionNewMessage = 67,
        PushOnlineSessionCallStarted = 68,
        PushOnlineSessionVideoCallStarted = 69,
        PushOnlineSessionPhoneCallStarted = 70,
        PushConsultationPurchasedAgent = 71,
        PushConsultationPurchasedUser = 72,
        PushConsultationEndingSoon = 73,
        PushConsultationExpiredRefund = 74,
        /// <summary>یادآور با چرخه‌ی روزانه/هفتگی: دقیقاً روز نوبت (بدون یادآوری چند روز قبل/بعد مثل چرخه‌ی ماهانه).</summary>
        PushReminderToday = 75,
        /// <summary>سفر فوری (Broadcast) بعد از N دقیقه هنوز راننده‌ای قبول نکرده - مرحله‌ی ۱: به کاربر اطلاع می‌دهیم که ادمین در جریان است.</summary>
        PushTripNoDriverAdminNotified = 76,
        /// <summary>سفر فوری (Broadcast) بعد از مرحله‌ی ۱ هم همچنان راننده‌ای پیدا نشده - مرحله‌ی ۲: به کاربر پیشنهاد می‌دهیم رزرو/سرویس دیگری را امتحان کند.</summary>
        PushTripNoDriverTryAnotherOption = 77,
        PushConsultationAssigned = 78,
        PushConsultationTakenByColleague = 79,
        PushConsultationUnclaimed = 80,
        /// <summary>به راننده: ۳۰ دقیقه به شروع یک سفر رزروشده/سرویس هفتگی‌ای که قبلاً قبول کرده مانده.</summary>
        PushTripDriverUpcomingReminder = 81,
        /// <summary>به کاربر: ۵ دقیقه به شروع یک جلسه‌ی زنده‌ی مدرسه (پاستیل لایو) مانده.</summary>
        PushSchoolClassReminder5Min = 82,
        /// <summary>به نماینده‌ها: کاربری یک مشاوره‌ی ساعت‌دار (قابل رزرو) را برای ساعت مشخصی رزرو کرد.</summary>
        PushConsultationBookedAgent = 83,
        /// <summary>به کاربر: رزرو مشاوره‌ی ساعت‌دار ثبت شد.</summary>
        PushConsultationBookedUser = 84,
        /// <summary>به کاربر و نماینده: ۳۰ دقیقه به ساعت رزرو مشاوره مانده.</summary>
        PushConsultationBookingReminder = 85,
        /// <summary>به کاربر: بدهی پرداخت‌نشده‌ی خدمت به کلینیک دارد و کیف پولش برای کسر خودکار کافی نیست (حداکثر ۳ بار در روز).</summary>
        PushCompanionDebtReminder = 86,
        /// <summary>به کاربر: بدهی خدمت به کلینیک به‌صورت خودکار از کیف پولش کسر شد.</summary>
        PushCompanionDebtCollected = 87,
        /// <summary>به کاربر: ۲ روز مانده به تأیید خودکار تحویل سفارش فروشگاهی (۵ روز بعد از ارسال و بدون پاسخ).</summary>
        PushProductOrderAutoDeliveryWarning = 88,
        /// <summary>به کاربر: سفارش فروشگاهی چون ۷ روز بدون پاسخ ماند، خودکار «تحویل داده شد» ثبت شد.</summary>
        PushProductOrderAutoDelivered = 89,
        /// <summary>به کاربران فروشگاه: مشتری «تحویل نگرفتم» را ثبت کرد (یک‌بار برای هر سفارش).</summary>
        PushProductOrderNotReceivedStore = 90
    }
}
