using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities.ProviderField
{
    /// <summary>پلن اشتراک ارائه‌دهنده (نقره‌ای، طلایی، الماس). مزایا بعداً تعریف می‌شود؛ فعلاً فقط نام و سطح.</summary>
    public class SubscriptionPlan : Name_Field
    {
        /// <summary>هرچه بزرگ‌تر، سطح بالاتر (نقره‌ای=1، طلایی=2، الماس=3).</summary>
        public int Level { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public ICollection<ProviderSubscription> ProviderSubscriptions { get; set; }
    }
}
