using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Entities.Entities.PastilAIField
{
    public class PastilAiPlan : Id_Field
    {
        [Required, MaxLength(50)]
        public string Code { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public int? DailyChatLimit { get; set; }
        public int? DailyImageLimit { get; set; }
        public int? DailyAudioLimit { get; set; }
        public int? DailyVideoLimit { get; set; }
        public bool PurchaseEnabled { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreateDateUtc { get; set; }
        public DateTime UpdateDateUtc { get; set; }
        public ICollection<PastilAiSubscription> Subscriptions { get; set; }
    }
}
