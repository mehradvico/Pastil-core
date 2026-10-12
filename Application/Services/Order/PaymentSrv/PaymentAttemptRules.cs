using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Services.Order.PaymentSrv.Dto;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.Order.PaymentSrv
{
    // تلاش پرداختِ نیمه‌کاره: کاربر به درگاه رفت و بدون پرداخت برگشت، یا اصلاً به درگاه نرسید.
    // چنین پرداختی (IsSuccess == null) هیچ‌وقت callback نمی‌گیرد و اگر کنار گذاشته نشود، تلاش بعدی کاربر را برای همیشه قفل می‌کند.
    // کنار گذاشتن = IsSuccess=false با وضعیت SUPERSEDED؛ اگر کاربر بعداً همان درگاه قدیمی را پرداخت کند، CallbackPayment آن را
    // verify نمی‌کند (IsSuccess=false) و بانک مبلغ تأییدنشده را خودکار به حساب پرداخت‌کننده برمی‌گرداند - پس دوبار کسر نهایی نمی‌شود.
    // شرط IsSuccess == null در هر دو طرف (این‌جا و MerchantService.CallbackAsync) گذار را اتمی می‌کند: فقط یکی برنده می‌شود.
    // پرداختی که callback آن همین الان در حال verify با بانک است (VERIFYING) کنار گذاشته نمی‌شود: بانک ممکن است پول را همین لحظه
    // قطعی کند و کنار گذاشتنش یعنی پولِ کسرشده بدون اعمال روی رزرو/سفارش.
    public static class PaymentAttemptRules
    {
        public const string SupersededGatewayStatus = "SUPERSEDED";
        public const string VerifyingGatewayStatus = "VERIFYING";
        private const string SupersededDescription = "Superseded by a newer payment attempt";

        // عمر یک تلاش درگاه؛ بعد از آن CallbackPayment پرداخت بی‌نتیجه را EXPIRED می‌کند
        public static readonly TimeSpan PendingAttemptLifetime = TimeSpan.FromMinutes(30);

        public enum TargetState
        {
            // هیچ پرداخت موفق یا در حال تأییدی نمانده؛ کاربر می‌تواند دوباره پرداخت کند یا مبلغ را عوض کند
            Free,
            // callback یک تلاش قبلی همین الان در حال verify با بانک است (چند ثانیه)
            Verifying,
            // پرداخت موفق برای این هدف ثبت شده
            Paid
        }

        public static Task<int> SupersedePendingAsync(IDataBaseContext context, string callBackTypeLabel, string callBackId)
        {
            var verifyingSince = DateTime.Now - PendingAttemptLifetime;
            return context.Payments
                .Where(s => s.CallBackTypeLabel == callBackTypeLabel &&
                            s.CallBackId == callBackId &&
                            s.IsSuccess == null &&
                            s.AppliedDate == null &&
                            (s.GatewayStatus == null || s.GatewayStatus != VerifyingGatewayStatus || s.CreateDate < verifyingSince))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.IsSuccess, false)
                    .SetProperty(s => s.GatewayStatus, SupersededGatewayStatus)
                    .SetProperty(s => s.Description, SupersededDescription));
        }

        // همه‌ی پرداخت‌های نیمه‌کاره‌ی یک کاربر برای یک نوع هدف (مثلاً اشتراک‌های رهاشده‌ی PastilAI)
        public static Task<int> SupersedeUserPendingAsync(IDataBaseContext context, long userId, string callBackTypeLabel)
        {
            var verifyingSince = DateTime.Now - PendingAttemptLifetime;
            return context.Payments
                .Where(s => s.UserId == userId &&
                            s.CallBackTypeLabel == callBackTypeLabel &&
                            s.IsSuccess == null &&
                            s.AppliedDate == null &&
                            (s.GatewayStatus == null || s.GatewayStatus != VerifyingGatewayStatus || s.CreateDate < verifyingSince))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.IsSuccess, false)
                    .SetProperty(s => s.GatewayStatus, SupersededGatewayStatus)
                    .SetProperty(s => s.Description, SupersededDescription));
        }

        public static Task<bool> HasSuccessfulAsync(IDataBaseContext context, string callBackTypeLabel, string callBackId)
        {
            return context.Payments.AsNoTracking().AnyAsync(s =>
                s.CallBackTypeLabel == callBackTypeLabel &&
                s.CallBackId == callBackId &&
                s.IsSuccess == true);
        }

        // قبل از هر تغییر کاربر روی مبلغ (کیف پول/تخفیف) یا تلاش دوباره: تلاش‌های رهاشده کنار گذاشته می‌شوند و
        // فقط اگر پرداخت قبلی واقعاً موفق شده (Paid) یا همین الان در حال تأیید است (Verifying) کاربر متوقف می‌شود.
        public static async Task<TargetState> ReleaseForRetryAsync(IDataBaseContext context, string callBackTypeLabel, string callBackId)
        {
            if (string.IsNullOrWhiteSpace(callBackTypeLabel) || string.IsNullOrWhiteSpace(callBackId))
                return TargetState.Free;

            await SupersedePendingAsync(context, callBackTypeLabel, callBackId);
            var remaining = await context.Payments.AsNoTracking()
                .Where(s => s.CallBackTypeLabel == callBackTypeLabel &&
                            s.CallBackId == callBackId &&
                            (s.IsSuccess == true || s.IsSuccess == null && s.AppliedDate == null))
                .Select(s => s.IsSuccess)
                .ToListAsync();
            if (remaining.Any(s => s == true))
                return TargetState.Paid;
            return remaining.Count > 0 ? TargetState.Verifying : TargetState.Free;
        }

        // نتیجه‌ی «قبلاً پرداخت شده»: کد ۲۰ + هدف، تا فرانت به‌جای خطا جزئیات همان مورد را نشان دهد
        public static BaseResultDto AlreadyPaid(string message, string callBackTypeLabel, string callBackId)
        {
            return new BaseResultDto<PaymentAlreadyPaidDto>(
                false,
                message,
                new PaymentAlreadyPaidDto { CallBackTypeLabel = callBackTypeLabel, CallBackId = callBackId },
                (int)ResultCodeEnum.PaymentAlreadyPaid);
        }

        // ReleaseForRetryAsync + نتیجه‌ی آماده برای کاربر؛ null یعنی آزاد است و کار ادامه پیدا می‌کند
        public static async Task<BaseResultDto> GuardUserRetryAsync(
            IDataBaseContext context, string callBackTypeLabel, string callBackId, string alreadyPaidMessage)
        {
            var state = await ReleaseForRetryAsync(context, callBackTypeLabel, callBackId);
            return state switch
            {
                TargetState.Paid => AlreadyPaid(alreadyPaidMessage, callBackTypeLabel, callBackId),
                TargetState.Verifying => new BaseResultDto(false, Resource.Notification.PaymentPreviousAttemptVerifying),
                _ => null
            };
        }

        // برای تغییرات سمت نماینده/اپراتور: تلاشی که هنوز ممکن است کاربر روی درگاهش باشد (کمتر از عمر درگاه) دست نمی‌خورد
        // و تغییر مبلغ را قفل می‌کند؛ تلاش رهاشده‌ی قدیمی‌تر (که دیگر هرگز اعمال نمی‌شود) قفل نمی‌کند.
        public static Task<bool> HasLiveAttemptAsync(IDataBaseContext context, string callBackTypeLabel, string callBackId)
        {
            var liveSince = DateTime.Now - PendingAttemptLifetime;
            return context.Payments.AsNoTracking().AnyAsync(s =>
                s.CallBackTypeLabel == callBackTypeLabel &&
                s.CallBackId == callBackId &&
                (s.IsSuccess == true || s.IsSuccess == null && s.AppliedDate == null && s.CreateDate >= liveSince));
        }

        // callback قبل از تماس verify با بانک پرداخت را «در حال تأیید» علامت می‌زند تا یک تلاش تازه‌ی هم‌زمان آن را کنار نگذارد.
        // چند callback هم‌زمانِ همان پرداخت هر کدام موفق به علامت‌زدن می‌شوند (رفتار قبلی SaveCallbackResultAsync حفظ می‌شود).
        public static async Task<bool> TryMarkVerifyingAsync(IDataBaseContext context, long paymentId)
        {
            var affected = await context.Payments
                .Where(s => s.Id == paymentId && s.IsSuccess == null && s.AppliedDate == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.GatewayStatus, VerifyingGatewayStatus));
            return affected > 0;
        }

        // تماس verify بدون نتیجه قطع شد (exception): علامت برداشته می‌شود تا تلاش بعدی کاربر قفل نشود
        public static Task<int> ClearVerifyingAsync(IDataBaseContext context, long paymentId)
        {
            return context.Payments
                .Where(s => s.Id == paymentId && s.IsSuccess == null && s.GatewayStatus == VerifyingGatewayStatus)
                .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.GatewayStatus, (string)null));
        }
    }
}
