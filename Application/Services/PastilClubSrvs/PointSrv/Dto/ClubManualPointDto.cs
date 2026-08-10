using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.PastilClubSrvs.PointSrv.Dto
{
    public class ClubManualPointDto
    {
        public long UserId { get; set; }

        [Range(1, long.MaxValue)]
        public long Amount { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; }

        public Guid RequestId { get; set; }
    }
}
