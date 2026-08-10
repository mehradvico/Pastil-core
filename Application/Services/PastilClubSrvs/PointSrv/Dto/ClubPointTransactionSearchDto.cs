using Application.Common.Dto.Result;

namespace Application.Services.PastilClubSrvs.PointSrv.Dto
{
    public class ClubPointTransactionSearchDto : BaseSearchDto<ClubPointTransactionVDto>
    {
        public ClubPointTransactionSearchDto(ClubPointTransactionInputDto dto)
            : base(dto)
        {
            UserId = dto.UserId;
            TransactionType = dto.TransactionType;
            SourceType = dto.SourceType;
            FromDate = dto.FromDate;
            ToDate = dto.ToDate;
        }

        public long? UserId { get; set; }
        public Entities.Entities.PastilClubField.ClubPointTransactionTypeEnum? TransactionType { get; set; }
        public Entities.Entities.PastilClubField.ClubPointSourceTypeEnum? SourceType { get; set; }
        public System.DateTime? FromDate { get; set; }
        public System.DateTime? ToDate { get; set; }
    }
}
