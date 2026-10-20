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
        PushCompanionReserveRescheduledDriver = 120,
        /// <summary>به فروشگاه: سفارش اکسپرس ثبت شد؛ همین الان «درخواست پیک» بزنید. Token1 = شماره سفارش، Token2 = مهلت (ساعت).</summary>
        PushShipmentExpressAwaitingSeller = 121,
        /// <summary>به مشتری: سفارش اکسپرس ثبت شد؛ ظرف ۲ تا ۳ ساعت کاری. Token1 = شماره سفارش، Token2 = متن زمان.</summary>
        PushOrderExpressUser = 122,
        /// <summary>به ادمین‌ها: فروشنده ۳۰ دقیقه بعد از پرداخت هنوز سفارش را آماده/تأیید نکرده (مهلت ۶۰ دقیقه). Token1 = شماره سفارش، Token2 = نام فروشگاه، Token3 = مهلت (ساعت).</summary>
        PushShipmentSellerNotReadyAdmin = 123,
        /// <summary>به راننده: یک ساعت به نوبت سرویس هفتگی پت‌رسان مانده؛ آماده باشید. Token1 = ساعت (HH:mm)، Token2 = شناسه‌ی سفر.</summary>
        PushTripServiceReminderDriver = 124,
        /// <summary>به کاربر: یک ساعت به نوبت سرویس هفتگی پت‌رسان مانده؛ آماده باشید. Token1 = ساعت (HH:mm)، Token2 = شناسه‌ی سفر.</summary>
        PushTripServiceReminderUser = 125,
        /// <summary>به مالک آرایشگاه: رزرو جدید پرداخت شد و منتظر تأیید شماست. Token1 = نام آرایشگاه، Token2 = نام پت، Token3 = شناسه‌ی رزرو.</summary>
        PushGroomerReserveApprovalRequired = 126,
        /// <summary>به کاربر: آرایشگاه رزرو را تأیید کرد. Token1 = نام آرایشگاه، Token2 = نام پت، Token3 = شناسه‌ی رزرو.</summary>
        PushGroomerReserveApproved = 127,
        /// <summary>به کاربر: آرایشگاه رزرو را رد کرد و مبلغ به کیف پول برگشت. Token1 = نام آرایشگاه، Token2 = دلیل.</summary>
        PushGroomerReserveRejected = 128,
        /// <summary>به کاربر: آرایشگاه در مهلت پاسخ نداد؛ رزرو لغو و مبلغ برگشت.</summary>
        PushGroomerReserveExpired = 129,
        /// <summary>به مالک آرایشگاه: کاربر رزرو را لغو کرد. Token1 = نام پت، Token2 = زمان نوبت.</summary>
        PushGroomerReserveCancelledByBooker = 130,
        /// <summary>به مالک: آرایشگاه تأیید شد. Token1 = نام آرایشگاه.</summary>
        PushGroomerApproved = 131,
        /// <summary>به مالک: آرایشگاه رد شد. Token1 = نام آرایشگاه، Token2 = دلیل.</summary>
        PushGroomerRejected = 132,
        /// <summary>به کاربر: زمان رزرو عوض شد. Token1 = نام آرایشگاه، Token2 = تاریخ، Token3 = ساعت.</summary>
        PushGroomerReserveRescheduled = 133,
        /// <summary>به مالک: زمان رزرو توسط ادمین عوض شد. Token1 = نام پت، Token2 = تاریخ، Token3 = ساعت.</summary>
        PushGroomerReserveRescheduledForGroomer = 134,
        /// <summary>به کاربرِ دعوت‌شده: دعوت به عضویت در پرسنل آرایشگاه. Token1 = نام آرایشگاه.</summary>
        PushGroomerStaffInvite = 135,
        /// <summary>به کاربر: رزرو توسط ادمین لغو شد. Token1 = نام آرایشگاه، Token2 = دلیل.</summary>
        PushGroomerReserveCancelledByAdmin = 136,
        /// <summary>به مالک کلینیک: رزرو جدید پرداخت شد و منتظر تأیید شماست. Token1 = نام کلینیک، Token2 = نام پت، Token3 = شناسه‌ی رزرو.</summary>
        PushClinicReserveApprovalRequired = 137,
        /// <summary>به کاربر: کلینیک رزرو را تأیید کرد. Token1 = نام کلینیک، Token2 = نام پت، Token3 = شناسه‌ی رزرو.</summary>
        PushClinicReserveApproved = 138,
        /// <summary>به کاربر: کلینیک رزرو را رد کرد و مبلغ به کیف پول برگشت. Token1 = نام کلینیک، Token2 = دلیل.</summary>
        PushClinicReserveRejected = 139,
        /// <summary>به کاربر: کلینیک در مهلت پاسخ نداد؛ رزرو لغو و مبلغ برگشت.</summary>
        PushClinicReserveExpired = 140,
        /// <summary>به مالک کلینیک: کاربر رزرو را لغو کرد. Token1 = نام پت، Token2 = زمان نوبت.</summary>
        PushClinicReserveCancelledByBooker = 141,
        /// <summary>به مالک: کلینیک تأیید شد. Token1 = نام کلینیک.</summary>
        PushClinicApproved = 142,
        /// <summary>به مالک: کلینیک رد شد. Token1 = نام کلینیک، Token2 = دلیل.</summary>
        PushClinicRejected = 143,
        /// <summary>به کاربر: زمان رزرو عوض شد. Token1 = نام کلینیک، Token2 = تاریخ، Token3 = ساعت.</summary>
        PushClinicReserveRescheduled = 144,
        /// <summary>به مالک: زمان رزرو توسط ادمین عوض شد. Token1 = نام پت، Token2 = تاریخ، Token3 = ساعت.</summary>
        PushClinicReserveRescheduledForClinic = 145,
        /// <summary>به کاربرِ دعوت‌شده: دعوت به عضویت در پرسنل کلینیک. Token1 = نام کلینیک.</summary>
        PushClinicStaffInvite = 146,
        /// <summary>به کاربر: رزرو توسط ادمین لغو شد. Token1 = نام کلینیک، Token2 = دلیل.</summary>
        PushClinicReserveCancelledByAdmin = 147,
        /// <summary>به کاربر: کلینیک یک بسته‌ی رزروش را تأیید کرد. Token1 = نام بسته، Token3 = شناسه‌ی رزرو.</summary>
        PushClinicReserveItemApproved = 148,
        /// <summary>به کاربر: کلینیک/ادمین یک بسته‌ی رزرو را لغو کرد و سهم پرداختی به کیف پول برگشت. Token1 = نام بسته، Token4 = مبلغ برگشتی.</summary>
        PushClinicReserveItemCancelled = 149,
        /// <summary>پیام تازه در گفتگوی رزرو کلینیک. Token1 = نام فرستنده، Token2 = پیش‌نمایش، Token3 = شناسه‌ی رزرو کلینیک.</summary>
        PushClinicReserveNewMessage = 150,
        /// <summary>به کاربر: کلینیک تماس درون‌برنامه‌ای رزرو را شروع کرد. Token1 = نام کلینیک، Token2 = شناسه‌ی رزرو کلینیک.</summary>
        PushClinicInAppCallStarted = 151,
        /// <summary>به کلینیک/پزشک: ۱۰ دقیقه به رزرو آنلاین مانده. Token1 = نام مشتری، Token2 = بسته، Token3 = نحوه‌ی ارائه، Token4 = شناسه‌ی رزرو کلینیک.</summary>
        PushClinicOnlineReserveReminderBefore = 152,
        /// <summary>به کلینیک/پزشک: زمان رزرو آنلاین رسید. Token1 = نام مشتری، Token2 = بسته، Token3 = نحوه‌ی ارائه، Token4 = شناسه‌ی رزرو کلینیک.</summary>
        PushClinicOnlineReserveReminderAtTime = 153,
        /// <summary>به مالک مربی: رزرو جدید پرداخت شد و منتظر تأیید شماست. Token1 = نام مربی، Token2 = نام پت، Token3 = شناسه‌ی رزرو.</summary>
        PushCoachReserveApprovalRequired = 154,
        /// <summary>به کاربر: مربی رزرو را تأیید کرد. Token1 = نام مربی، Token2 = نام پت، Token3 = شناسه‌ی رزرو.</summary>
        PushCoachReserveApproved = 155,
        /// <summary>به کاربر: مربی رزرو را رد کرد و مبلغ به کیف پول برگشت. Token1 = نام مربی، Token2 = دلیل.</summary>
        PushCoachReserveRejected = 156,
        /// <summary>به کاربر: مربی در مهلت پاسخ نداد؛ رزرو لغو و مبلغ برگشت.</summary>
        PushCoachReserveExpired = 157,
        /// <summary>به مالک مربی: کاربر رزرو را لغو کرد. Token1 = نام پت، Token2 = زمان نوبت.</summary>
        PushCoachReserveCancelledByBooker = 158,
        /// <summary>به مالک: مربی تأیید شد. Token1 = نام مربی.</summary>
        PushCoachApproved = 159,
        /// <summary>به مالک: مربی رد شد. Token1 = نام مربی، Token2 = دلیل.</summary>
        PushCoachRejected = 160,
        /// <summary>به کاربر: زمان رزرو عوض شد. Token1 = نام مربی، Token2 = تاریخ، Token3 = ساعت.</summary>
        PushCoachReserveRescheduled = 161,
        /// <summary>به مالک: زمان رزرو توسط ادمین عوض شد. Token1 = نام پت، Token2 = تاریخ، Token3 = ساعت.</summary>
        PushCoachReserveRescheduledForCoach = 162,
        /// <summary>به کاربرِ دعوت‌شده: دعوت به عضویت در پرسنل مربی. Token1 = نام مربی.</summary>
        PushCoachStaffInvite = 163,
        /// <summary>به کاربر: رزرو توسط ادمین لغو شد. Token1 = نام مربی، Token2 = دلیل.</summary>
        PushCoachReserveCancelledByAdmin = 164,
        /// <summary>پیام تازه در گفتگوی رزرو مربی. Token1 = نام فرستنده، Token2 = پیش‌نمایش، Token3 = شناسه‌ی رزرو مربی.</summary>
        PushCoachReserveNewMessage = 165,
        /// <summary>به کاربر: مربی تماس درون‌برنامه‌ای رزرو را شروع کرد. Token1 = نام مربی، Token2 = شناسه‌ی رزرو مربی.</summary>
        PushCoachInAppCallStarted = 166,
        /// <summary>به مربی/پزشک: ۱۰ دقیقه به رزرو آنلاین مانده. Token1 = نام مشتری، Token2 = بسته، Token3 = نحوه‌ی ارائه، Token4 = شناسه‌ی رزرو مربی.</summary>
        PushCoachOnlineReserveReminderBefore = 167,
        /// <summary>به مربی/پزشک: زمان رزرو آنلاین رسید. Token1 = نام مشتری، Token2 = بسته، Token3 = نحوه‌ی ارائه، Token4 = شناسه‌ی رزرو مربی.</summary>
        PushCoachOnlineReserveReminderAtTime = 168,
    }
}
