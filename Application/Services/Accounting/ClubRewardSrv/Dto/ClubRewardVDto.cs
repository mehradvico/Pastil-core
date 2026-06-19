using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.ClubRewardSrv.Dto
{
    public class ClubRewardVDto : Name_FieldDto
    {
        public double RequiredScore { get; set; }
        public int ValidityDays { get; set; }
        public double DiscountValue { get; set; }
        public bool IsPriceRebate { get; set; }
        public bool CanUserRedeem { get; set; }
        public string RebateTypeName { get; set; }
    }
}
