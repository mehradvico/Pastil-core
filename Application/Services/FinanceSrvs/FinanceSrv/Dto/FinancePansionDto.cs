using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.FinanceSrvs.FinanceSrv.Dto
{
    public class FinancePansionDto
    {
        public long PansionId { get; set; }
        public decimal DailyCommissionPercent { get; set; }
        public decimal HourlyCommissionPercent { get; set; }
    }
}
