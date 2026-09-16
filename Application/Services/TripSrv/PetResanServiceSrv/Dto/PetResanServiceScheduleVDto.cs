namespace Application.Services.TripSrv.PetResanServiceSrv.Dto
{
    public class PetResanServiceScheduleVDto
    {
        public long Id { get; set; }
        public long WeekDayId { get; set; }
        public string WeekDayName { get; set; }
        public int WeekDayNumber { get; set; }
        public string Time { get; set; }
    }
}
