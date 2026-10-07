using Application.Common.Dto.Result;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Order.ProductOrderSrv.Iface
{
    // لغو کامل سفارش یا کم/حذف کردن آیتم توسط فروشنده/ادمین (قبل از ارسال)؛ مبلغ به کیف پول مشتری برمی‌گردد.
    // مشتری بعد از پرداخت هیچ‌چیز را نمی‌تواند لغو یا حذف کند. طراحی: backend/Docs/PRODUCT_ORDER_CANCEL_ADJUST_FA.md
    public interface IProductOrderAdjustmentService
    {
        // storeId: فقط برای actor = Store (مالکیت)؛ برای ادمین نادیده گرفته می‌شود
        Task<BaseResultDto> CancelOrderAsync(string orderId, OrderAdjustActor actor, long storeId, string reason,
            CancellationToken cancellationToken = default);

        Task<BaseResultDto> AdjustItemAsync(long productOrderItemId, int newCount, OrderAdjustActor actor, long storeId, string reason,
            CancellationToken cancellationToken = default);
    }
}
