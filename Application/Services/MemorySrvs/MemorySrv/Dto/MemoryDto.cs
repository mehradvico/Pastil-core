using Application.Common.Dto.Field;
using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.MemorySrvs.MemorySrv.Dto
{
    public class MemoryDto : Id_FieldDto
    {
        [Required, StringLength(4000, MinimumLength = 1)]
        public string Text { get; set; }

        [Required]
        public DateTimeOffset MemoryDate { get; set; }

        public long? PictureId { get; set; }

        [Range(1, long.MaxValue)]
        public long UserPetId { get; set; }
    }
}
