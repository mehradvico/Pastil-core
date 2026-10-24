using Entities.Entities.CommonField;

namespace Entities.Entities
{
    /// <summary>خرید مشاوره‌ی آنلاینِ مراکز مستقل (کلینیک/مربی) که داخل یک تسویه‌ی ارائه‌دهنده پرداخت شده است؛ هر خرید فقط یک‌بار (ایندکس یکتا).</summary>
    public class SettlementConsultation : Id_Field
    {
        public long SettlementId { get; set; }
        public long ConsultationPurchaseId { get; set; }

        public Settlement Settlement { get; set; }
        public ConsultationPurchase ConsultationPurchase { get; set; }
    }
}
