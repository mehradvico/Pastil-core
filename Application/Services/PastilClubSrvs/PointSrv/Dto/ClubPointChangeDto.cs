using Entities.Entities.PastilClubField;

namespace Application.Services.PastilClubSrvs.PointSrv.Dto
{
    public class ClubPointChangeDto
    {
        public long UserId { get; set; }
        public long Amount { get; set; }
        public ClubPointTransactionTypeEnum TransactionType { get; set; }
        public ClubPointSourceTypeEnum SourceType { get; set; }
        public long? SourceId { get; set; }
        public long? PointRuleId { get; set; }
        public int? DailyLimit { get; set; }
        public int? MonthlyLimit { get; set; }
        public int? LifetimeLimit { get; set; }
        public long? ParentTransactionId { get; set; }
        public string Description { get; set; }
        public string IdempotencyKey { get; set; }
        public long? CreatedByUserId { get; set; }
        public long? CreatedByAdminId { get; set; }
    }
}
