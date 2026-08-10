using Entities.Entities.CommonField;
using System;
using System.ComponentModel.DataAnnotations;

namespace Entities.Entities
{
    public class Memory : Id_Field
    {
        [Required, MaxLength(4000)]
        public string Text { get; set; }

        public DateTimeOffset MemoryDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public long? PictureId { get; set; }
        public bool Deleted { get; set; }

        public Picture Picture { get; set; }
        public UserMemory UserMemory { get; set; }
    }
}
