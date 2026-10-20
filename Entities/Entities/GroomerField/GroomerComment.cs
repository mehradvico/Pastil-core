namespace Entities.Entities.GroomerField
{
    /// <summary>نظر کاربر درباره‌ی آرایشگاه. نظر مربوط به رزرو (GroomerReserveId) در فاز رزرو اضافه می‌شود.</summary>
    public class GroomerComment : Comment
    {
        public long GroomerId { get; set; }
        /// <summary>نظر ثبت‌شده بعد از رزرو تکمیل‌شده؛ برای هر رزرو حداکثر یک نظر (ایندکس یکتا).</summary>
        public long? GroomerReserveId { get; set; }
        public bool IsReserved { get; set; }
        public Groomer Groomer { get; set; }
        public GroomerReserve GroomerReserve { get; set; }
    }
}
