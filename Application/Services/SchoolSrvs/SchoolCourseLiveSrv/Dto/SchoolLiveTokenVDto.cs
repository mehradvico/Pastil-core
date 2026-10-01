namespace Application.Services.SchoolSrvs.SchoolCourseLiveSrv.Dto
{
    public class SchoolLiveTokenVDto
    {
        public long LiveSessionId { get; set; }
        public string RoomName { get; set; }
        public string Token { get; set; }
        public string LiveKitHost { get; set; }
        public int StatusId { get; set; }
    }
}
