using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.CompanionReserveCommentSrv.Iface;
using AutoMapper;
using Entities.Entities.CompanionField;
using System.Linq;

namespace Application.Services.CompanionSrvs.CompanionReserveCommentSrv.Dto
{
    public class CompanionReserveCommentSearchDto : BaseSearchDto<CompanionReserveComment, CompanionReserveCommentVDto>, ICompanionReserveCommentSearchFields
    {
        public CompanionReserveCommentSearchDto(CompanionReserveCommentInputDto dto, IQueryable<CompanionReserveComment> list, IMapper mapper) : base(dto, list, mapper)
        {
            this.CompanionReserveId = dto.CompanionReserveId;
            this.CompanionId = dto.CompanionId;
            this.AllStatus = dto.AllStatus;
            this.UserId = dto.UserId;
        }
        public long? CompanionReserveId { get; set; }
        public long? CompanionId { get; set; }
        public bool? AllStatus { get; set; }
        public long? UserId { get; set; }

    }
}
