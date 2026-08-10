using Entities.Entities.PastilClubField;

namespace Application.Services.PastilClubSrvs.PointEventSrv.Dto
{
    public class ClubPointEventDto
    {
        public long UserId { get; set; }
        public ClubPointEventTypeEnum EventType { get; set; }
        public ClubPointSourceTypeEnum SourceType { get; set; }
        public long? SourceId { get; set; }
        public string SourceKey { get; set; }
        public string Description { get; set; }
    }
}
