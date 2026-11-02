namespace Application.Services.FinanceSrvs.FinanceSrv.Dto
{
    // یک مرکز در تب «مراکز و متخصصین» حسابداری؛ هر نوع از جدول خودش (نه Companion قدیمی)
    public class FinanceVenueVDto
    {
        /// <summary>clinic | coach | groomer | pansion | school</summary>
        public string Kind { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public string PictureUrl { get; set; }
        public bool Active { get; set; }

        /// <summary>کلینیک/مربی/آرایشگاه: درصد کمیسیون مرکز</summary>
        public decimal? CommissionPercent { get; set; }

        /// <summary>پانسیون: کمیسیون روزانه و ساعتی</summary>
        public decimal? DailyCommissionPercent { get; set; }
        public decimal? HourlyCommissionPercent { get; set; }

        /// <summary>مدرسه: تعداد دوره‌ها و دوره‌های دارای کمیسیون</summary>
        public int CourseCount { get; set; }
        public int CoursesWithCommissionCount { get; set; }

        public bool HasCommission { get; set; }

        /// <summary>رزروهای پرداخت‌شده و لغونشده</summary>
        public int ReserveCount { get; set; }

        /// <summary>پانسیون/مدرسه: نمایندگی ظرف (برای صفحه‌ی حسابداری قدیمی)</summary>
        public long? CompanionId { get; set; }
    }
}
