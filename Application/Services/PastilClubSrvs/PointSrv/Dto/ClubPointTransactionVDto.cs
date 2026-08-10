using Entities.Entities.PastilClubField;
using System;

namespace Application.Services.PastilClubSrvs.PointSrv.Dto
{
    public class ClubPointTransactionVDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserMobile { get; set; }
        public ClubPointTransactionTypeEnum TransactionType { get; set; }
        public long Amount { get; set; }
        public long AvailableBefore { get; set; }
        public long AvailableAfter { get; set; }
        public long DebtBefore { get; set; }
        public long DebtAfter { get; set; }
        public ClubPointSourceTypeEnum SourceType { get; set; }
        public long? SourceId { get; set; }
        public long? PointRuleId { get; set; }
        public long? ParentTransactionId { get; set; }
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }
        public long? CreatedByAdminId { get; set; }
    }
}
