using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Gallery : Seo_Full_Field, ISlugEntity
    {
        public string Label { get; set; }
        public string Slug { get; set; }
        public long? PictureId { get; set; }
        public long? CategoryId { get; set; }
        public int Priority { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public Category Category { get; set; }
        public Picture Picture { get; set; }
        public ICollection<GalleryItem> GalleryItems { get; set; }

        public string GetSlugSource() => Label;
    }
}
