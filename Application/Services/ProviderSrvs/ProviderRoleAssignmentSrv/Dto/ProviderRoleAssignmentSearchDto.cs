using Application.Common.Dto.Result;
using Application.Services.ProviderSrvs.ProviderRoleAssignmentSrv.Iface;
using AutoMapper;
using Entities.Entities.ProviderField;
using System.Linq;

namespace Application.Services.ProviderSrvs.ProviderRoleAssignmentSrv.Dto
{
    public class ProviderRoleAssignmentSearchDto : BaseSearchDto<ProviderRoleAssignment, ProviderRoleAssignmentVDto>, IProviderRoleAssignmentSearchFields
    {
        public ProviderRoleAssignmentSearchDto(ProviderRoleAssignmentInputDto dto, IQueryable<ProviderRoleAssignment> list, IMapper mapper) : base(dto, list, mapper)
        {
            ProviderId = dto.ProviderId;
            ProviderRoleId = dto.ProviderRoleId;
            Approved = dto.Approved;
        }

        public long? ProviderId { get; set; }
        public long? ProviderRoleId { get; set; }
        public bool? Approved { get; set; }
    }
}
