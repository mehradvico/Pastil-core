using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolPictureSrv.Iface;
using AutoMapper;
using Entities.Entities.SchoolField;
using System.Linq;

namespace Application.Services.SchoolSrvs.SchoolPictureSrv.Dto
{
    public class SchoolPictureSearchDto : BaseSearchDto<SchoolPicture, SchoolPictureVDto>, ISchoolPictureSearchFields
    {
        public SchoolPictureSearchDto(SchoolPictureInputDto dto, IQueryable<SchoolPicture> list, IMapper mapper) : base(dto, list, mapper)
        {
            SchoolId = dto.SchoolId;
            CompanionId = dto.CompanionId;
        }
        public long? SchoolId { get; set; }
        public long? CompanionId { get; set; }
    }
}
