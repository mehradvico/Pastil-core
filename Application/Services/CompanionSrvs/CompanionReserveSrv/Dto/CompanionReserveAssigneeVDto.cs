namespace Application.Services.CompanionSrvs.CompanionReserveSrv.Dto
{
    public class CompanionReserveAssigneeVDto
    {
        public long CompanionAssistanceUserId { get; set; }
        public long UserId { get; set; }
        public string FullName { get; set; }
        public long? PictureId { get; set; }
        public bool IsFemale { get; set; }
        public long? ExpertiseId { get; set; }
        public string ExpertiseName { get; set; }
        public bool IsAssigned { get; set; }
    }
}
