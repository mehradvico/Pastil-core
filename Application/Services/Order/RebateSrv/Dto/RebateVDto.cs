using Application.Common.Dto.Field;
using Application.Services.Setting.CodeSrv.Dto;

namespace Application.Services.Order.RebateSrv.Dto
{
    public class RebateVDto : Name_FieldDto
    {
        public double MinCartPrice { get; set; }
        public string CodeValue { get; set; }
        public double PriceValue { get; set; }
        public bool IsPriceRebate { get; set; }
        public double FinalPrice { get; set; }
        public long? ProductId { get; set; }
        public long TypeId { get; set; }
        public int MaxUsePerUser { get; set; }
        public long? ClubRewardId { get; set; }
    }
}
