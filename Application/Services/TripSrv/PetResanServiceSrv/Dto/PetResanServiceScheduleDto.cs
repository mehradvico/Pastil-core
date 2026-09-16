namespace Application.Services.TripSrv.PetResanServiceSrv.Dto
{
    public class PetResanServiceScheduleDto
    {
        public long WeekDayId { get; set; }

        /// <summary>قالب "HH:mm" — دقیقاً مثل CompanionTime.StartTime.</summary>
        public string Time { get; set; }
    }
}
