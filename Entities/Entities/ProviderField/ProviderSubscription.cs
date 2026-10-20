using Entities.Entities.CommonField;
using System;

namespace Entities.Entities.ProviderField
{
    /// <summary>یک دوره اشتراک؛ هر رکورد بخشی از تاریخچه است و اشتراک فعلی = آخرین دوره‌ای که پایانش در آینده است.</summary>
    public class ProviderSubscription : Id_Field
    {
        public long ProviderId { get; set; }
        public long PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long? CreatedByUserId { get; set; }
        public DateTime CreateDate { get; set; }
        public string Note { get; set; }
        public bool Deleted { get; set; }

        public Provider Provider { get; set; }
        public SubscriptionPlan Plan { get; set; }
    }
}
