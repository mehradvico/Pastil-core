using Application.Common.Dto.Field;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PastilAISrvs.Dto
{
    public class PastilAiPlanUpdateDto : Id_FieldDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        [Range(1, 3650)]
        public int DurationDays { get; set; }
        [Range(1, int.MaxValue)]
        public int? DailyChatLimit { get; set; }
        [Range(0, int.MaxValue)]
        public int? DailyImageLimit { get; set; }
        [Range(0, int.MaxValue)]
        public int? DailyAudioLimit { get; set; }
        [Range(0, int.MaxValue)]
        public int? DailyVideoLimit { get; set; }
        public bool PurchaseEnabled { get; set; }
        public bool Active { get; set; }
        public int SortOrder { get; set; }
    }
}
