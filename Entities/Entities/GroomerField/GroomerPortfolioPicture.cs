using Entities.Entities.CommonField;

namespace Entities.Entities.GroomerField
{
    /// <summary>یک رسانه‌ی نمونه‌کار؛ Kind: ۰ عکس، ۱ عکس «قبل»، ۲ عکس «بعد»، ۳ ویدیو.</summary>
    public class GroomerPortfolioPicture : Id_Field
    {
        public long PostId { get; set; }
        public long PictureId { get; set; }
        public int Kind { get; set; }
        public int SortOrder { get; set; }
        public string Label { get; set; }
        public bool Deleted { get; set; }

        public GroomerPortfolioPost Post { get; set; }
        public Picture Picture { get; set; }
    }
}
