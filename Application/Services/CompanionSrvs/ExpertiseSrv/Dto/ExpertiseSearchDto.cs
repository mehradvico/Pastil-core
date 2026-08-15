using Application.Common.Dto.Result;
using AutoMapper;
using Entities.Entities.CompanionField;
using System.Linq;

namespace Application.Services.CompanionSrvs.ExpertiseSrv.Dto
{
    public class ExpertiseSearchDto : BaseSearchDto<Expertise, ExpertiseVDto>
    {
        public ExpertiseSearchDto(
            ExpertiseInputDto dto,
            IQueryable<Expertise> list,
            IMapper mapper)
            : base(dto, list, mapper)
        {
        }
    }
}
