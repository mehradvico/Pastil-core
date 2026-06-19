using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.ClubRewardSrv.Dto
{
    public class ClubRewardDto : Name_FieldDto
    {
        public double RequiredScore { get; set; }
        public long RebateId { get; set; }
        public int ValidityDays { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
    }
}
