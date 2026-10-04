using Application.Common.Enumerable;
using System;

namespace Application.Services.Order.ProductOrderSrv.Iface
{
    public interface IProductOrderSearchFields
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public ProductOrderStateEnum? ProductOrderStateEnum { get; set; }
        public ProductOrderStatusEnum? ProductOrderStatusEnum { get; set; }
        public long? UserId { get; set; }
        public long? StoreId { get; set; }
        public string TrackingCode { get; set; }
        public bool? HasCancelRequestDate { get; set; }
        public bool? HasParentOrderId { get; set; }
        public bool? HasChildOrderId { get; set; }
        public bool? HasReserveDate { get; set; }
        // فیلتر تحویل‌گیری کاربر: 0 = منتظر پاسخ کاربر (ارسال‌شده و هنوز پاسخ نداده)، 1 = تحویل گرفته، 2 = تحویل نگرفته
        public int? UserDelivery { get; set; }
    }
}
