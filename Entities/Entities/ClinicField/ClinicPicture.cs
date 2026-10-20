using Entities.Entities.CommonField;

namespace Entities.Entities.ClinicField
{
    /// <summary>گالری معمولی کلینیک (محیط، تجهیزات)؛ جدا از نمونه‌کار (ClinicPortfolioPost).</summary>
    public class ClinicPicture : Id_Field
    {
        public long ClinicId { get; set; }
        public long PictureId { get; set; }
        public string Label { get; set; }
        public bool Deleted { get; set; }

        public Clinic Clinic { get; set; }
        public Picture Picture { get; set; }
    }
}
