using Entities.Entities.CommonField;

namespace Entities.Entities
{
    public class Banner : FullName_Field, ISlugEntity
    {

        public string Url { get; set; }
        public string Label { get; set; }
        public string Slug { get; set; }
        public int Priority { get; set; }
        public long? CategoryId { get; set; }
        public long? PictureId { get; set; }
        public long? Picture2Id { get; set; }
        public int ClickCount { get; set; }
        public bool Active { get; set; }
        public bool ShowToApp { get; set; }
        public bool ShowToSite { get; set; }
        public bool Deleted { get; set; }
        public Category Category { get; set; }
        public Picture Picture { get; set; }
        public Picture Picture2 { get; set; }

        public string GetSlugSource() => Label;
    }
}
