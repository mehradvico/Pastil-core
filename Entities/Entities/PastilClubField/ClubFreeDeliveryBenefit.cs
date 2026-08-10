using Entities.Entities.CommonField;
using Entities.Entities.LocationField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities.PastilClubField
{
    public class ClubFreeDeliveryBenefit : Id_Field
    {
        public long RewardRedemptionId { get; set; }
        public long UserId { get; set; }
        public long? StoreId { get; set; }
        public long? CityId { get; set; }
        public decimal? MaximumDeliveryAmount { get; set; }
        public int RemainingUsageCount { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTime CreateDate { get; set; }
        public byte[] RowVersion { get; set; }

        public ClubRewardRedemption RewardRedemption { get; set; }
        public User User { get; set; }
        public Store Store { get; set; }
        public City City { get; set; }
    }
}
