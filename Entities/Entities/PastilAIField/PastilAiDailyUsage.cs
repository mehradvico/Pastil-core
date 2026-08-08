using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities.PastilAIField
{
    public class PastilAiDailyUsage : Id_Field
    {
        public long UserId { get; set; }
        public DateTime UsageDate { get; set; }
        public int ChatCount { get; set; }
        public int ImageCount { get; set; }
        public int AudioCount { get; set; }
        public int VideoCount { get; set; }
        public User User { get; set; }
    }
}
