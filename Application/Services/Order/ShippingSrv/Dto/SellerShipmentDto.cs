using Entities.Entities.ShippingField;
using System;

namespace Application.Services.Order.ShippingSrv.Dto
{
    // ورودی مراحل فروشنده (تأیید سفارش و «آماده تحویل به پیک»)
    public class ShipmentConfirmDto
    {
        public long ProductOrderStoreId { get; set; }
    }

    // اپ فروشنده: سفارش‌هایی که باید تأیید شوند
    public class SellerShipmentVDto
    {
        public long ProductOrderStoreId { get; set; }
        public string ProductOrderId { get; set; }
        public string OrderCode { get; set; }
        public ShipmentStatusEnum Status { get; set; }

        /// <summary>بازه‌ی تحویل به مشتری (UTC)</summary>
        public DateTime? SlotStartUtc { get; set; }
        public DateTime? SlotEndUtc { get; set; }

        /// <summary>مرحله: ۱ = منتظر «تأیید سفارش»، ۲ = در حال آماده‌سازی و منتظر «آماده تحویل به پیک».</summary>
        public int Step { get; set; }

        /// <summary>مرحله‌ی ۱: تا این ساعت باید سفارش را تأیید کنید.</summary>
        public DateTime? SellerConfirmDeadlineUtc { get; set; }

        /// <summary>مرحله‌ی ۲: تا این ساعت باید «آماده تحویل به پیک» بزنید، وگرنه سفارش لغو می‌شود.</summary>
        public DateTime? ReadyDeadlineUtc { get; set; }

        /// <summary>اگر همین الان «آماده تحویل به پیک» بزنید، پیک تقریباً این ساعت (UTC) می‌رسد.</summary>
        public DateTime PlannedPickupUtc { get; set; }
    }
}
