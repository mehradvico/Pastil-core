using Application.Common.Dto.Result;
using Application.Services.Order.ShippingSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ShippingSrv.Iface
{
    public interface IShipmentService
    {
        Task CreateForPaidOrderAsync(
            ProductOrder productOrder,
            CancellationToken cancellationToken = default);

        Task CancelForOrderAsync(
            string productOrderId,
            CancellationToken cancellationToken = default);

        // مرحله‌ی ۱ فروشنده: سفارش را تأیید می‌کند (در حال آماده‌سازی). سفر میاره هنوز ساخته نمی‌شود.
        Task<BaseResultDto> ConfirmBySellerAsync(
            long storeId,
            long productOrderStoreId,
            CancellationToken cancellationToken = default);

        // مرحله‌ی ۲ فروشنده: «آماده تحویل به پیک»؛ همین لحظه سفر میاره با pickup.deadline مناسب بازه‌ی مشتری ساخته می‌شود.
        Task<BaseResultDto> MarkReadyBySellerAsync(
            long storeId,
            long productOrderStoreId,
            CancellationToken cancellationToken = default);

        // مرسوله‌های منتظر اقدام فروشنده (مرحله‌ی ۱ و ۲) این فروشگاه (برای اپ فروشنده)
        Task<BaseResultDto<List<SellerShipmentVDto>>> GetAwaitingForStoreAsync(
            long storeId,
            CancellationToken cancellationToken = default);

        // job (هر ۵ دقیقه): یادآوری ۱۵ دقیقه مانده به مهلت و شکست مرسوله‌ی تأییدنشده؛ تعداد مرسوله‌ی منقضی را برمی‌گرداند
        Task<int> ProcessUnconfirmedAsync(CancellationToken cancellationToken = default);

        // مشتری بعد از «تحویل شدن» با میاره «تحویل نگرفتم» زد: به ادمین اطلاع می‌دهد (فرصت گزارش به میاره ۳ ساعت)
        Task ReportNotReceivedAsync(
            string productOrderId,
            CancellationToken cancellationToken = default);

        Task HandleMiareWebhookAsync(
            string payload,
            CancellationToken cancellationToken = default);
    }
}
