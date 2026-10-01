namespace Application.Common
{
    // فلگ‌های سراسری فیچرهایی که کدشان کامل و آماده است ولی فعلاً نباید در تولید فعال/قابل‌اجرا باشند.
    // عمداً اینجا نگه داشته می‌شوند (نه حذف کد) تا با تغییر یک خط، بدون هیچ کار دیگری فعال شوند.
    public static class FeatureFlags
    {
        // بخش «آنلاین» مدرسه (دوره‌ی CourseTypeId=Live: پاستیل لایو/Skype/Adobe Connect) - غیرفعال طبق
        // درخواست مستقیم؛ کد پاستیل لایو (SchoolCourseLiveSrv، هاب، وب‌هوک، مایگریشن‌ها) دست‌نخورده مانده.
        public const bool SchoolOnlineCoursesEnabled = false;
    }
}
