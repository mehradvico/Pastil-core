using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceStoreSrv.Dto
{
    public class FinanceProductOrderVDto
    {
        public string ProductOrderId { get; set; }
        public string OrderCode { get; set; }
        public string BuyerFullName { get; set; }
        public double PaymentPrice { get; set; }
        /// <summary>مبلغ کالای سفارش پس از تخفیف محصول و پیش از کد تخفیف.</summary>
        public double GoodsPrice { get; set; }
        /// <summary>ارسال پیش‌پرداخت؛ تماماً متعلق به پاستیل. قیمت استعلام پس‌کرایه نیست.</summary>
        public double DeliveryPrice { get; set; }
        public double CommissionBase { get; set; }
        /// <summary>کمیسیون ناخالص کالا، استخراج‌شده از سهم ثبت‌شده (مبنای کالا منهای سهم فروشگاه).</summary>
        public double CommissionAmount { get; set; }
        /// <summary>کمیسیون پس از تخفیف محصول با هزینه پاستیل؛ SiteShare منهای DeliveryPrice.</summary>
        public double NetCommissionAmount { get; set; }
        /// <summary>اختلاف با قاعده فعلی یا سفارش چندفروشگاهی؛ قبل از تسویه بررسی شود.</summary>
        public bool AccountingReviewRequired { get; set; }
        public decimal CommissionPercent { get; set; }
        public double StoreShare { get; set; }
        public double SiteShare { get; set; }
        /// <summary>تخفیف محصول با هزینه پاستیل؛ از کمیسیون کالا کم می‌شود، نه از سهم فروشگاه.</summary>
        public double PastilFundedAmount { get; set; }
        public string StatusLabel { get; set; }
        public bool Permitted { get; set; }
        // تحویل‌گیری توسط خود مشتری (برای بررسی قبل از تسویه): null = هنوز پاسخ نداده، true = تحویل گرفت، false = تحویل نگرفت
        public bool? UserReceived { get; set; }
        public System.DateTime? UserReceivedDate { get; set; }
        // true = تأیید خودکار بعد از ۷ روز بدون پاسخ
        public bool UserReceivedAuto { get; set; }
        public string UserReceiveNote { get; set; }
    }
}
