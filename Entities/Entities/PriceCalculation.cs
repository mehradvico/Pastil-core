using Entities.Entities.CommonField;

namespace Entities.Entities
{
    public class PriceCalculation : Id_Field
    {
        public int FromTime { get; set; }
        public int ToTime { get; set; }
        public double Price { get; set; }
        public double StopPrice { get; set; }
        // از پت دوم به بعد، به ازای هر پت اضافه‌ی همون سفر یک‌بار به قیمت کل اضافه می‌شود (پت اول رایگان).
        public double ExtraPetPrice { get; set; }
        // مبلغ ثابتی که فقط وقتی Trip.VehicleType به‌جای سواری، وانت باشد، یک‌بار به کل کرایه اضافه می‌شود.
        public double PickupVehicleExtraPrice { get; set; }
        public bool Deleted { get; set; }
    }
}
