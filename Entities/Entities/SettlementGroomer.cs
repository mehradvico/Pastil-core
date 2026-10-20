using Entities.Entities.CommonField;
using Entities.Entities.GroomerField;

namespace Entities.Entities
{
    /// <summary>ردیف رزرو آرایشگاه که داخل یک تسویه‌ی ارائه‌دهنده پرداخت شده است؛ هر رزرو فقط یک‌بار (ایندکس یکتا).</summary>
    public class SettlementGroomer : Id_Field
    {
        public long SettlementId { get; set; }
        public long GroomerReserveId { get; set; }

        public Settlement Settlement { get; set; }
        public GroomerReserve GroomerReserve { get; set; }
    }
}
