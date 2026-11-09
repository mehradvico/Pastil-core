using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.PansionField
{
    public class PansionPet : Id_Field
    {
        public long PansionId { get; set; }
        public long PetId { get; set; }
        // سایز همین ردیف (Small/Medium/Large)؛ null = همه‌ی سایزهای این نوع پت.
        [System.ComponentModel.DataAnnotations.MaxLength(16)]
        public string PetSize { get; set; }
        // تعرفه‌ی اختصاصی این پت/سایز؛ null = همان تعرفه‌ی پایه‌ی مرکز (Pansion.PansionPrice / SchoolPrice).
        public double? PansionPrice { get; set; }
        public double? SchoolPrice { get; set; }
        public bool Deleted { get; set; }

        public Pansion Pansion { get; set; }
        public Pet Pet { get; set; }
    }
}
