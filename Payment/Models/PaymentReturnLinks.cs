using Application.Services.Order.PaymentSrv.Dto;

namespace Payment.Models
{
    // دکمه‌های صفحه‌ی نتیجه‌ی پرداخت: کاربر مستقیم به همان رزرو/سفارش برمی‌گردد (موفق: جزئیات؛ ناموفق/لغو: جایی که بتواند دوباره پرداخت کند)
    public sealed record PaymentReturnLinks(string PrimaryUrl, string PrimaryLabel, string SecondaryUrl, string SecondaryLabel)
    {
        public static PaymentReturnLinks For(PaymentDto payment, bool isSuccess, string baseUrl)
        {
            var site = (baseUrl ?? string.Empty).TrimEnd('/');
            string Url(string path) => site + path;
            var id = System.Uri.EscapeDataString(payment?.CallBackId ?? string.Empty);
            var hasId = !string.IsNullOrWhiteSpace(payment?.CallBackId);

            switch (payment?.CallBackTypeLabel)
            {
                case "ProductOrder":
                    return isSuccess && hasId
                        ? new(Url($"/reserve/order-{id}"), "مشاهده سفارش", Url("/"), "بازگشت به اپ پاستیل")
                        : new(Url("/basket"), "بازگشت به سبد خرید", Url("/"), "بازگشت به اپ پاستیل");
                case "CompanionReserve":
                    return new(hasId ? Url($"/reserve/{id}") : Url("/reserve"), isSuccess ? "مشاهده رزرو" : "بازگشت به رزرو و پرداخت دوباره", Url("/reserve"), "همه‌ی رزروها");
                case "CompanionReserveBatch":
                    return new(Url("/reserve"), isSuccess ? "مشاهده رزروها" : "بازگشت به رزروها و پرداخت دوباره", Url("/"), "بازگشت به اپ پاستیل");
                case "PansionReserve":
                    return new(hasId ? Url($"/pansion/reserve-{id}") : Url("/reserve?section=pansion"), isSuccess ? "مشاهده رزرو" : "بازگشت به رزرو و پرداخت دوباره", Url("/reserve?section=pansion"), "همه‌ی رزروها");
                case "GroomerReserve":
                    return new(hasId ? Url($"/groomer/reserve-{id}") : Url("/reserve?section=groomer"), isSuccess ? "مشاهده رزرو" : "بازگشت به رزرو و پرداخت دوباره", Url("/reserve?section=groomer"), "همه‌ی رزروها");
                case "ClinicReserve":
                    return new(hasId ? Url($"/clinic/reserve-{id}") : Url("/reserve?section=clinic"), isSuccess ? "مشاهده رزرو" : "بازگشت به رزرو و پرداخت دوباره", Url("/reserve?section=clinic"), "همه‌ی رزروها");
                case "CoachReserve":
                    return new(hasId ? Url($"/coach/reserve-{id}") : Url("/reserve?section=coach"), isSuccess ? "مشاهده رزرو" : "بازگشت به رزرو و پرداخت دوباره", Url("/reserve?section=coach"), "همه‌ی رزروها");
                case "SchoolReserve":
                    return new(hasId ? Url($"/school/reserve-{id}") : Url("/reserve?section=training"), isSuccess ? "مشاهده ثبت‌نام" : "بازگشت به ثبت‌نام و پرداخت دوباره", Url("/reserve?section=training"), "همه‌ی رزروها");
                case "ConsultationPurchase":
                    return new(isSuccess && hasId ? Url($"/consultations/{id}") : Url("/consultations"), isSuccess ? "مشاهده مشاوره" : "بازگشت به مشاوره‌ها", Url("/"), "بازگشت به اپ پاستیل");
                case "Trip":
                    return new(hasId ? Url($"/trip/{id}") : Url("/transportation"), isSuccess ? "مشاهده سفر" : "بازگشت به سفر و پرداخت دوباره", Url("/"), "بازگشت به اپ پاستیل");
                case "Wallet":
                    return new(Url("/wallet"), "بازگشت به کیف پول", Url("/"), "بازگشت به اپ پاستیل");
                case "PastilAI":
                    return new(Url("/ai"), "بازگشت به پاستیل AI", Url("/"), "بازگشت به اپ پاستیل");
                default:
                    return new(Url("/"), "بازگشت به اپ پاستیل", Url("/reserve"), "مشاهده رزروها و سفارش‌ها");
            }
        }
    }
}
