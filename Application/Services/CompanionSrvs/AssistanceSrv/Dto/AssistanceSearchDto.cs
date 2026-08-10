using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.AssistanceSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.CompanionSrvs.AssistanceSrv.Dto
{
    public class AssistanceSearchDto : BaseSearchDto<Assistance, AssistanceVDto>, IAssistanceSearchFields
    {
        public AssistanceSearchDto(AssistanceInputDto dto, IQueryable<Assistance> list, IMapper mapper) : base(dto, list, mapper)
        {
            IsPersonal = dto.IsPersonal;
            AssistanceGroupId = dto.AssistanceGroupId;
            ShowToSite = dto.ShowToSite;
        }

        public bool? IsPersonal { get; set; }
        public long? AssistanceGroupId { get; set; }
        public bool? ShowToSite { get; set; }
    }
}
