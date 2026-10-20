using Entities.Entities.Security;
using Entities.Entities.CommonField;
using System;

namespace Entities.Entities.GroomerField
{
    /// <summary>لایک کاربر روی پست نمونه‌کار؛ هر کاربر فقط یک لایک برای هر پست (ایندکس یکتا).</summary>
    public class GroomerPortfolioLike : Id_Field
    {
        public long PostId { get; set; }
        public long UserId { get; set; }
        public DateTime CreateDate { get; set; }

        public GroomerPortfolioPost Post { get; set; }
        public User User { get; set; }
    }
}
