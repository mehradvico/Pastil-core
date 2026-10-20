using Entities.Entities.CommonField;

namespace Entities.Entities.CoachField
{
    /// <summary>گالری معمولی مربی (محیط، تجهیزات)؛ جدا از نمونه‌کار (CoachPortfolioPost).</summary>
    public class CoachPicture : Id_Field
    {
        public long CoachId { get; set; }
        public long PictureId { get; set; }
        public string Label { get; set; }
        public bool Deleted { get; set; }

        public Coach Coach { get; set; }
        public Picture Picture { get; set; }
    }
}
