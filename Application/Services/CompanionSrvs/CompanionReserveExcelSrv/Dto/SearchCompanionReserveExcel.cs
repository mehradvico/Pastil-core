using Application.Common.Enumerable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveExcelSrv.Dto
{
    public class SearchCompanionReserveExcelDto
    {
        public long? CompanionId { get; set; }
        public long? PansionId { get; set; }
        public long? BookerId { get; set; }
        public long? CompanionAssistanceId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public ReserveStateEnum? ReserveState { get; set; }
    }
}
