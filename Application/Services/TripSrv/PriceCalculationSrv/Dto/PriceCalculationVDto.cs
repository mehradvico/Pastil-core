using Application.Common.Dto.Field;

namespace Application.Services.TripSrv.PriceCalculationSrv.Dto
{
    public class PriceCalculationVDto : Id_FieldDto
    {
        public int FromTime { get; set; }
        public int ToTime { get; set; }
        public double Price { get; set; }
        public double StopPrice { get; set; }
        public double ExtraPetPrice { get; set; }
        public double PickupVehicleExtraPrice { get; set; }

    }
}
