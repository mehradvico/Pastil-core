using Application.Services.Dto;
using Application.Services.Order.AddressSrv.Dto;
using Application.Services.Order.ProductOrderStoreSrv.Dto;
using Application.Services.Order.RebateSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.Order.ProductOrderSrv.Dto
{
    public class ProductOrderVDto
    {
        public string Id { get; set; }
        public string OrderCode { get; set; }

        public long UserId { get; set; }
        public long? AddressId { get; set; }
        public long PaymentTypeId { get; set; }
        public long ProductOrderStatusId { get; set; }
        public long ProductOrderStateId { get; set; }
        public double Price { get; set; }
        public double BasePrice { get; set; }
        public double DiscountPrice { get; set; }
        public double PaymentPrice { get; set; }
        public DateTime CreateDate { get; set; }
        public string BonusCode { get; set; }
        public string UserDescription { get; set; }
        public string AdminDescription { get; set; }
        public string StateDescription { get; set; }
        public bool IsPaid { get; set; }
        public long? RebateId { get; set; }
        public double RebatePrice { get; set; }
        public long? DeliveryTypeId { get; set; }
        public double DeliveryPrice { get; set; }
        public long? ClubFreeDeliveryBenefitId { get; set; }
        public double ClubDeliveryDiscount { get; set; }
        public string TrackingCode { get; set; }
        // تحویل‌گیری توسط کاربر: null = هنوز پاسخ نداده، true = تحویل گرفتم (سفارش نهایی شد)، false = تحویل نگرفتم
        public bool? UserReceived { get; set; }
        public System.DateTime? UserReceivedDate { get; set; }
        public string UserReceiveNote { get; set; }
        // true = تحویل‌گیری را سیستم خودکار (بعد از ۷ روز بدون پاسخ) ثبت کرده است
        public bool UserReceivedAuto { get; set; }
        // زمانی که اگر کاربر پاسخ ندهد سفارش خودکار «تحویل گرفته» می‌شود (فقط وقتی هنوز منتظر پاسخ است؛ برای شمارش معکوس)
        public System.DateTime? AutoConfirmDate { get; set; }
        // true = کاربر همین الان می‌تواند «تحویل گرفتم/نگرفتم» را ثبت کند (پرداخت‌شده، وضعیت «ارسال‌شده»، لغو نشده، هنوز نهایی نشده)
        public bool CanConfirmDelivery { get; set; }
        public DateTime? HasCancelRequestDate { get; set; }
        /// <summary>لحظه‌ای که وضعیت سفارش «ارسال شده» شد.</summary>
        public DateTime? SentDate { get; set; }
        /// <summary>لحظه‌ی پوش «آیا سفارش را تحویل گرفتید؟» به مشتری (UTC).</summary>
        public DateTime? ReceiptAskedAtUtc { get; set; }
        /// <summary>زمان پرداخت (فقط برای ادمین/فروشنده پر می‌شود): زمان ساخته‌شدن مرسوله بعد از پرداخت، وگرنه آخرین پرداخت موفق/برداشت کیف پول.</summary>
        public DateTime? PaidDate { get; set; }
        /// <summary>زمان لغو سفارش؛ برای لغوهای قدیمی بدون ثبت (فقط ادمین/فروشنده) از زمان برگشت پول لغو در کیف پول خوانده می‌شود.</summary>
        public DateTime? CancelledDate { get; set; }
        /// <summary>عامل لغو: 1 = ادمین، 2 = فروشگاه؛ null = نامشخص (لغوهای قدیمی).</summary>
        public int? CancelledByActor { get; set; }
        public long? CancelledStoreId { get; set; }
        /// <summary>نام فروشگاه لغوکننده (فقط برای ادمین/فروشنده).</summary>
        public string CancelledStoreName { get; set; }
        /// <summary>اولین لحظه‌ی «در حال پردازش» شدن سفارش.</summary>
        public DateTime? ProcessingDate { get; set; }
        public double WalletPrice { get; set; }
        public double SiteShare { get; set; }
        public double StoreShare { get; set; }
        /// <summary>جمع تخفیفِ سهم پاستیل این سفارش (هزینه‌ی پاستیل).</summary>
        public double PastilFundedAmount { get; set; }
        public RebateVDto Rebate { get; set; }
        public UserVDto User { get; set; }
        public AddressVDto Address { get; set; }
        public CodeVDto PaymentType { get; set; }
        public CodeVDto ProductOrderStatus { get; set; }
        public CodeVDto ProductOrderState { get; set; }
        public CodeVDto DeliveryType { get; set; }
        public List<ProductOrderStoreVDto> ProductOrderStores { get; set; }
    }
}
