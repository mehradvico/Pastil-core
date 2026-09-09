using Application.Common.Dto.Input;

namespace Application.Services.SchoolSrvs.SchoolReserveSrv.Dto
{
    public class SchoolReserveInputDto : BaseInputDto
    {
        public long? BookerId { get; set; }
        public long? SchoolCourseId { get; set; }
        public long? SchoolId { get; set; }
        public long? CompanionId { get; set; }
        public int? StatusId { get; set; }
    }
}
