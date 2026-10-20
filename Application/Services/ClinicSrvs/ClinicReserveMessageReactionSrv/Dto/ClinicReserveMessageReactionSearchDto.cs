using Entities.Entities.ClinicField;
using Application.Common.Dto.Result;
using Application.Services.ClinicSrvs.ClinicReserveMessageReactionSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageReactionSrv.Dto
{
    public class ClinicReserveMessageReactionSearchDto : BaseSearchDto<ClinicReserveMessageReaction, ClinicReserveMessageReactionVDto>, IClinicReserveMessageReactionSearchFields
    {
        public ClinicReserveMessageReactionSearchDto(ClinicReserveMessageReactionInputDto dto, IQueryable<ClinicReserveMessageReaction> list, IMapper mapper) : base(dto, list, mapper)
        {
            ClinicReserveMessageId = dto.ClinicReserveMessageId;
            ReactorUserId = dto.ReactorUserId;
            Reaction = dto.Reaction;
        }

        public long? ClinicReserveMessageId { get; set; }
        public long? ReactorUserId { get; set; }
        public string Reaction { get; set; }
    }
}
