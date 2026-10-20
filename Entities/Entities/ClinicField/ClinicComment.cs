namespace Entities.Entities.ClinicField
{
    /// <summary>نظر کاربر درباره‌ی کلینیک. نظر مربوط به رزرو (ClinicReserveId) در فاز رزرو اضافه می‌شود.</summary>
    public class ClinicComment : Comment
    {
        public long ClinicId { get; set; }
        /// <summary>نظر ثبت‌شده بعد از رزرو تکمیل‌شده؛ برای هر رزرو حداکثر یک نظر (ایندکس یکتا).</summary>
        public long? ClinicReserveId { get; set; }
        public bool IsReserved { get; set; }
        public Clinic Clinic { get; set; }
        public ClinicReserve ClinicReserve { get; set; }
    }
}
