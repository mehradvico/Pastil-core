using Entities.Entities.CommonField;
using Entities.Entities.CoachField;

namespace Entities.Entities
{
    /// <summary>ردیف رزرو مربی که داخل یک تسویه‌ی ارائه‌دهنده پرداخت شده است؛ هر رزرو فقط یک‌بار (ایندکس یکتا).</summary>
    public class SettlementCoach : Id_Field
    {
        public long SettlementId { get; set; }
        public long CoachReserveId { get; set; }

        public Settlement Settlement { get; set; }
        public CoachReserve CoachReserve { get; set; }
    }
}
