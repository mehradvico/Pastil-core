using Entities.Entities.CommonField;
using Entities.Entities.ClinicField;

namespace Entities.Entities
{
    /// <summary>ردیف رزرو کلینیک که داخل یک تسویه‌ی ارائه‌دهنده پرداخت شده است؛ هر رزرو فقط یک‌بار (ایندکس یکتا).</summary>
    public class SettlementClinic : Id_Field
    {
        public long SettlementId { get; set; }
        public long ClinicReserveId { get; set; }

        public Settlement Settlement { get; set; }
        public ClinicReserve ClinicReserve { get; set; }
    }
}
