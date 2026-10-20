using Entities.Entities.CommonField;

namespace Entities.Entities.GroomerField
{
    /// <summary>گالری معمولی آرایشگاه (محیط، تجهیزات)؛ جدا از نمونه‌کار (GroomerPortfolioPost).</summary>
    public class GroomerPicture : Id_Field
    {
        public long GroomerId { get; set; }
        public long PictureId { get; set; }
        public string Label { get; set; }
        public bool Deleted { get; set; }

        public Groomer Groomer { get; set; }
        public Picture Picture { get; set; }
    }
}
