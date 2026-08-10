using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;

namespace Entities.Entities.PastilClubField
{
    public class ClubPointRule : Id_Field
    {
        public string Name { get; set; }
        public ClubPointEventTypeEnum EventType { get; set; }
        public long PointAmount { get; set; }
        public int? DailyLimit { get; set; }
        public int? MonthlyLimit { get; set; }
        public int? LifetimeLimit { get; set; }
        public bool Active { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        public ICollection<ClubPointTransaction> Transactions { get; set; }
    }
}
