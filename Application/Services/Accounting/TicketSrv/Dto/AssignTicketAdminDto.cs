using System.ComponentModel.DataAnnotations;

namespace Application.Services.Accounting.TicketSrv.Dto
{
    public class AssignTicketAdminDto
    {
        [Range(1, long.MaxValue)]
        public long TicketId { get; set; }

        public long? AdminId { get; set; }
    }
}