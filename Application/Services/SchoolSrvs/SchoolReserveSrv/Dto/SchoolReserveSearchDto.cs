using Application.Common.Dto.Result;
using AutoMapper;
using Entities.Entities.SchoolField;
using System.Linq;

namespace Application.Services.SchoolSrvs.SchoolReserveSrv.Dto
{
    public class SchoolReserveSearchDto : BaseSearchDto<SchoolReserve, SchoolReserveVDto>
    {
        public SchoolReserveSearchDto(SchoolReserveInputDto dto, IQueryable<SchoolReserve> list, IMapper mapper) : base(dto, list, mapper)
        {
            BookerId = dto.BookerId;
            SchoolCourseId = dto.SchoolCourseId;
            StatusId = dto.StatusId;
        }

        public long? BookerId { get; set; }
        public long? SchoolCourseId { get; set; }
        public int? StatusId { get; set; }
    }
}
