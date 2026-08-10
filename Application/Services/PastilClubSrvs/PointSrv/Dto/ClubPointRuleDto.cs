using Entities.Entities.PastilClubField;
using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.PastilClubSrvs.PointSrv.Dto
{
    public class ClubPointRuleDto
    {
        public long Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public ClubPointEventTypeEnum EventType { get; set; }

        [Range(1, long.MaxValue)]
        public long PointAmount { get; set; }

        [Range(1, int.MaxValue)]
        public int? DailyLimit { get; set; }

        [Range(1, int.MaxValue)]
        public int? MonthlyLimit { get; set; }

        [Range(1, int.MaxValue)]
        public int? LifetimeLimit { get; set; }

        public bool Active { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }
    }
}
