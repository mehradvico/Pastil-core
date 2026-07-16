using System.ComponentModel.DataAnnotations;

namespace Application.Services.Accounting.TicketSrv.Dto
{
    public class CreateAdminTicketDto
    {
        [Range(1, long.MaxValue)]
        public long UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string Body { get; set; }

        public long? FileId { get; set; }

        [Range(1, long.MaxValue)]
        public long TicketCategoryId { get; set; }

        public long? ProductId { get; set; }

        public long? ImportanceId { get; set; }
    }
}