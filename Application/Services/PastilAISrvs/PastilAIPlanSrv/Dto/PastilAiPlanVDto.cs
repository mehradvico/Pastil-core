using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilAISrvs.PastilAIPlanSrv.Dto
{
    public class PastilAiPlanVDto : Id_FieldDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public int? DailyChatLimit { get; set; }
        public int? DailyImageLimit { get; set; }
        public int? DailyAudioLimit { get; set; }
        public int? DailyVideoLimit { get; set; }
        public bool PurchaseEnabled { get; set; }
        public bool Active { get; set; }
        public int SortOrder { get; set; }
    }
}
