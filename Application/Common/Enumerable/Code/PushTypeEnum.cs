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
        PushProductOrderNotReceivedStore = 90,
        /// <summary>به کاربر: نماینده یک پکیج رزروش را تأیید کرد.</summary>
        PushCompanionReservePackageApproved = 91,
        /// <summary>به کاربر: نماینده/ادمین یک پکیج رزروش را لغو کرد و سهم پرداختی به کیف پول برگشت.</summary>
        PushCompanionReservePackageCancelled = 92,
        /// <summary>به کاربر: سبد خرید فروشگاه رها شده (روزی ساعت ۱۸، حداکثر ۲ بار برای هر سبد).</summary>
        PushAbandonedCart = 93,
        /// <summary>به فروشنده: سفارش با ارسال میاره پرداخت شد؛ باید تا مهلت آماده‌سازی را تأیید کند.</summary>
        PushShipmentAwaitingSeller = 94,
        /// <summary>به فروشنده: ۱۵ دقیقه به پایان مهلت تأیید مانده.</summary>
        PushShipmentSellerReminder = 95,
        /// <summary>به مشتری: فروشنده در مهلت تأیید نکرد؛ پشتیبانی لغو و استرداد را پیگیری می‌کند.</summary>
        PushShipmentSellerExpiredUser = 96,
        /// <summary>به فروشنده: پیک میاره به‌خاطر تأخیر لغو شد؛ یک‌بار دیگر می‌تواند تأیید کند.</summary>
        PushShipmentCourierCanceled = 97,
        /// <summary>به فروشنده: کالا تحویل مشتری نشد و پیک آن را برمی‌گرداند.</summary>
        PushShipmentReturning = 98,
        /// <summary>به مشتری: تحویل از بازه‌ی انتخابی‌اش عقب افتاده.</summary>
        PushShipmentDelayedUser = 99,
        /// <summary>به مشتری: سفارش میاره تحویل پیک شد؛ کد تحویل.</summary>
        PushShipmentShippedUser = 100,
        /// <summary>به ادمین: تحویل یک سفارش از بازه‌اش عقب افتاده.</summary>
        PushShipmentLateAdmin = 101,
        /// <summary>به ادمین: مشتری بعد از تحویل میاره «تحویل نگرفتم» زد (فرصت گزارش به میاره ۳ ساعت).</summary>
        PushShipmentNotReceivedAdmin = 102,
        /// <summary>به فروشنده: نزدیک پایان مهلت «آماده تحویل به پیک».</summary>
        PushShipmentReadyReminder = 103,
        /// <summary>به مشتری: راس پایان بازه‌ی تحویل «آیا سفارش را تحویل گرفتید؟» با دکمه‌های بله/خیر روی خود پوش.</summary>
        PushOrderAskReceived = 104,
        /// <summary>به فروشنده: مشتری تحویل گرفتم زد.</summary>
        PushOrderReceivedStore = 105,
        /// <summary>به ادمین: مشتری تحویل گرفتم زد.</summary>
        PushOrderReceivedAdmin = 106,
        /// <summary>به فروشگاه: ادمین کل سفارش را لغو کرد.</summary>
        PushOrderCancelledByAdminStore = 107,
        /// <summary>به ادمین: فروشگاه کل سفارش را لغو کرد.</summary>
        PushOrderCancelledByStoreAdmin = 108,
        /// <summary>به فروشگاه: ادمین کالایی را از سفارش کم/حذف کرد.</summary>
        PushOrderChangedByAdminStore = 109,
        /// <summary>به ادمین: فروشگاه کالایی را از سفارش کم/حذف کرد.</summary>
        PushOrderChangedByStoreAdmin = 110,
        /// <summary>به مسافر: سفر پت‌رسان فعال است و پت سوار شده؛ اعلان ماندگار با زمان تقریبی رسیدن (با tag ثابت جایگزین می‌شود).</summary>
        PushTripOngoing = 111,
        /// <summary>به مسافر: پایان/لغو سفر؛ اعلان ماندگار قبلی را جایگزین و بعد از چند ثانیه بسته می‌شود.</summary>
        PushTripOngoingEnd = 112,
        /// <summary>به رانندگان: چند نوبت جدید سرویس هفتگی پت‌رسان ساخته شد (یک پوش تجمیعی، نه یک پوش برای هر نوبت). Token1 = تعداد.</summary>
        PushTripServiceOccurrencesAvailable = 113,
        /// <summary>به مرکز: رزروی پرداخت شد و منتظر تأیید شماست.</summary>
        PushPansionReserveApprovalRequired = 114,
        /// <summary>به کاربر: مرکز رزرو را تأیید کرد.</summary>
        PushPansionReserveApproved = 115,
        /// <summary>به کاربر: مرکز رزرو را رد کرد و مبلغ به کیف پول برگشت. Token2 = دلیل.</summary>
        PushPansionReserveRejected = 116,
        /// <summary>به کاربر: مرکز در مهلت پاسخ نداد؛ رزرو لغو و مبلغ برگشت.</summary>
        PushPansionReserveExpired = 117,
        /// <summary>به کاربر: موجودی کیف پول برای کسر نوبت‌های سرویس هفتگی پت‌رسان کافی نیست. Token1 = مبلغ کسری (تومان).</summary>
        PushTripServiceWalletTopUp = 118,
        /// <summary>به کاربر: زمان رزرو (کلینیک/مربی/آرایشگاه) توسط مرکز یا ادمین تغییر کرد.</summary>
        PushCompanionReserveRescheduledUser = 119,
        /// <summary>به راننده‌ی پذیرنده‌ی سفر پت‌رسانِ متصل: زمان حرکت عوض شد.</summary>
        PushCompanionReserveRescheduledDriver = 120
    }
}
