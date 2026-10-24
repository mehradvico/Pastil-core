namespace Application.Services.SchoolSrvs.SchoolReserveSrv
{
    public static class SchoolReserveRules
    {
        // نام ردیف دفتر کیف پول برای برگشت وجه لغو ثبت‌نام؛ یکتا بودنش جلوی برگشت دوباره را می‌گیرد
        public static string RefundLedgerName(long reserveId) => $"SchoolReserveRefund:{reserveId}";
    }
}
