using Application.Common.Dto.Field;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.Content.ContactUsItemSrv.Dto
{
    public class ContactUsItemDto : Id_FieldDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Value { get; set; }

        public long ContactUsId { get; set; }
    }
}
