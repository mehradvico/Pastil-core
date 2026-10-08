namespace Application.Common.Enumerable.Code
{
    /// <summary>
    /// تصمیم مرکز (پانسیون/مهد) درباره‌ی رزروِ پرداخت‌شده. مستقل از PansionReserveStatusEnum (۴۶/۴۷/۴۸) است و آن را جایگزین نمی‌کند.
    /// NotRequired = رزروهای قبل از این قابلیت (و رزروهای پرداخت‌نشده)؛ مثل قبل بدون نیاز به تأیید.
    /// </summary>
    public enum PansionReserveOwnerDecisionEnum
    {
        NotRequired = 0,
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Expired = 4
    }
}
