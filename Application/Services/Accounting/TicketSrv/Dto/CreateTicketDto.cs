using System.ComponentModel.DataAnnotations;

namespace Application.Services.Accounting.TicketSrv.Dto
{
    public class CreateTicketDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        public string Body { get; set; }
        public long? FileId { get; set; }

        [Range(1, long.MaxValue)]
        public long TicketCategoryId { get; set; }
        public long? ProductId { get; set; }
    }
}