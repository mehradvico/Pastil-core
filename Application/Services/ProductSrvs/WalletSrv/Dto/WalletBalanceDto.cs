using System;

namespace Application.Services.ProductSrvs.WalletSrv.Dto
{
    // موجودی نمایشی کیف پول کاربر (تومان)
    public class WalletBalanceDto
    {
        /// <summary>نقد + اعتبار کلاب قابل خرج در همه‌جا.</summary>
        public double Total { get; set; }
        /// <summary>موجودی نقدی کیف پول.</summary>
        public double Cash { get; set; }
        /// <summary>اعتبار تشویقی فعال کلاب (Global)، مثل هدیه‌ی تکمیل پروفایل پت.</summary>
        public double Credit { get; set; }
        /// <summary>نزدیک‌ترین تاریخ انقضای اعتبار کلاب (null اگر اعتبار ندارد).</summary>
        public DateTimeOffset? CreditExpiresAt { get; set; }
    }
}
