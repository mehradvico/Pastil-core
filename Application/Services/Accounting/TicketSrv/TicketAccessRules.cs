namespace Application.Services.Accounting.TicketSrv
{
    /// <summary>
    /// قاعده‌ی دسترسی ادمین‌ها به تیکت‌های پشتیبانی (محرمانه): «ادمین اصلی» (SuperAdminIdentity) همه‌ی تیکت‌ها را می‌بیند؛
    /// هر ادمین دیگر (از جمله نقش «مدیر») فقط تیکتی که به خودش واگذار شده؛ تیکت بدون مسئول را هیچ ادمین غیراصلی نمی‌بیند.
    /// </summary>
    public static class TicketAccessRules
    {
        public static bool CanAdminAccess(bool isSuperAdmin, long? ticketAdminId, long? currentUserId) =>
            isSuperAdmin ||
            (ticketAdminId.HasValue && currentUserId.HasValue && ticketAdminId.Value == currentUserId.Value);
    }
}
