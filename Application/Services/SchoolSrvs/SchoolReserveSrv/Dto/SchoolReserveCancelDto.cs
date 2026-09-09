using Application.Common.Dto.Field;

namespace Application.Services.SchoolSrvs.SchoolReserveSrv.Dto
{
    public class SchoolReserveCancelDto : Id_FieldDto
    {
        public bool IsCancel { get; set; }
        public string CancelDetail { get; set; }
    }
}
