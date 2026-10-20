namespace Entities.Entities.GroomerField
{
    /// <summary>کامنت کاربر زیر پست نمونه‌کار (از Comment پایه: متن، وضعیت تأیید، لایک). ParentId برای پاسخ‌های تودرتو.</summary>
    public class GroomerPortfolioComment : Comment
    {
        public long PostId { get; set; }
        public long? ParentId { get; set; }
        public bool Deleted { get; set; }

        public GroomerPortfolioPost Post { get; set; }
        public GroomerPortfolioComment Parent { get; set; }
    }
}
