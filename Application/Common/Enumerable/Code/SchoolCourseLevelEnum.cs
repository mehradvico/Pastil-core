namespace Application.Common.Enumerable.Code
{
    // سطح دوره‌ی مدرسه (SchoolCourse.Level)
    public enum SchoolCourseLevelEnum
    {
        None = 0,
        Beginner = 1,
        Intermediate = 2,
        Advanced = 3
    }

    // ثبت‌نام بعد از شروع جلسه‌ی اول (SchoolCourse.LateEnrollmentMode)
    public enum SchoolLateEnrollmentEnum
    {
        // پیش‌فرض و رفتار قبلی: بعد از شروع جلسه‌ی اول ثبت‌نام بسته است
        Closed = 0,
        // تا شروع جلسه‌ی آخر باز، با قیمت کامل
        OpenFullPrice = 1,
        // تا شروع جلسه‌ی آخر باز، قیمت به نسبت جلسه‌های باقی‌مانده
        OpenProrated = 2
    }
}
