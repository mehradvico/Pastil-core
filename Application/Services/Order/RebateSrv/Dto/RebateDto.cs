using Application.Common.Dto.Field;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Application.Services.Order.RebateSrv.Dto
{
    public class RebateDto : Name_FieldDto
    {

        [Display(Name = nameof(Resource.Field.Code), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
 ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public string CodeValue { get; set; }
        [Display(Name = nameof(Resource.Field.PriceValue), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
 ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]

        public double PriceValue { get; set; }
        public DateTime StartDatetime { get; set; }
        public DateTime EndDatetime { get; set; }
        public bool IsPriceRebate { get; set; }
        public bool Active { get; set; }
        [Display(Name = nameof(Resource.Field.UseCount), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
        ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]

        public int UseCount { get; set; }
        public int MaxUsePerUser { get; set; }
        [IgnoreDataMember]
        public int UsedCount { get; set; }
        public long? ProductId { get; set; }
        public long? UserId { get; set; }
        public long? ClubRewardId { get; set; }
        public long TypeId { get; set; }
        public double MinCartPrice { get; set; }

    }
}
