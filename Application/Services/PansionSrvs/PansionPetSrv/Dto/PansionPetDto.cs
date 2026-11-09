using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PansionSrvs.PansionPetSrv.Dto
{
    public class PansionPetDto : Id_FieldDto
    {
        public long PansionId { get; set; }
        public long PetId { get; set; }
        /// <summary>Small/Medium/Large؛ خالی = همه‌ی سایزهای این نوع پت.</summary>
        public string PetSize { get; set; }
        /// <summary>تعرفه‌ی هر شب برای این پت/سایز؛ خالی یا صفر = تعرفه‌ی پایه‌ی مرکز.</summary>
        public double? PansionPrice { get; set; }
        /// <summary>تعرفه‌ی هر ساعت مهد برای این پت/سایز؛ خالی یا صفر = تعرفه‌ی پایه‌ی مرکز.</summary>
        public double? SchoolPrice { get; set; }
    }
}
