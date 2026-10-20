using Entities.Entities.CommonField;

namespace Entities.Entities.ClinicField
{
    /// <summary>نوع کلینیک (کدهای CompanionTypeEnum: کلینیک ۴۰، آزمایشگاه ۴۲، پرستار ۴۴).</summary>
    public class ClinicType : Id_Field
    {
        public long ClinicId { get; set; }
        public long TypeId { get; set; }
        public bool Deleted { get; set; }

        public Clinic Clinic { get; set; }
        public Code Type { get; set; }
    }
}
