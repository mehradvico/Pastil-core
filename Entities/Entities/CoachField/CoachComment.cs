namespace Entities.Entities.CoachField
{
    /// <summary>نظر کاربر درباره‌ی مربی. نظر مربوط به رزرو (CoachReserveId) در فاز رزرو اضافه می‌شود.</summary>
    public class CoachComment : Comment
    {
        public long CoachId { get; set; }
        /// <summary>نظر ثبت‌شده بعد از رزرو تکمیل‌شده؛ برای هر رزرو حداکثر یک نظر (ایندکس یکتا).</summary>
        public long? CoachReserveId { get; set; }
        public bool IsReserved { get; set; }
        public Coach Coach { get; set; }
        public CoachReserve CoachReserve { get; set; }
    }
}
