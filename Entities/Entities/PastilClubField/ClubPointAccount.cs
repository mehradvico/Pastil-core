using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Entities.Entities.PastilClubField
{
    public class ClubPointAccount : Id_Field
    {
        public long UserId { get; set; }
        public long AvailablePoint { get; set; }
        public long DebtPoint { get; set; }
        public long LifetimeEarnedPoint { get; set; }
        public long LifetimeSpentPoint { get; set; }
        public long LifetimeReversedPoint { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdateDate { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public User User { get; set; }
        public ICollection<ClubPointTransaction> Transactions { get; set; }
    }
}
