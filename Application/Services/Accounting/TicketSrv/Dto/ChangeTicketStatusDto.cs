using System.ComponentModel.DataAnnotations;

namespace Application.Services.Accounting.TicketSrv.Dto
{
    public class ChangeTicketStatusDto
    {
        [Range(1, long.MaxValue)]
        public long TicketId { get; set; }

        [Range(1, long.MaxValue)]
        public long StatusId { get; set; }
    }
}