using Application.Common.Dto.Field;

namespace Application.Services.TripSrv.TripSrv.Dto
{
    public class TripDriverCancelDto : Id_FieldDto
    {
        public long CancelReasonCodeId { get; set; }
        public string CancelDetail { get; set; }
    }
}
