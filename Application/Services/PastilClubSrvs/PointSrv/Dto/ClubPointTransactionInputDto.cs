using Application.Common.Dto.Input;
using Entities.Entities.PastilClubField;
using System;

namespace Application.Services.PastilClubSrvs.PointSrv.Dto
{
    public class ClubPointTransactionInputDto : BaseInputDto
    {
        public long? UserId { get; set; }
        public ClubPointTransactionTypeEnum? TransactionType { get; set; }
        public ClubPointSourceTypeEnum? SourceType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
