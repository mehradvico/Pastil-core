using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class GalleryItem : FullName_Field
    {
        public string Link { get; set; }
        public long? PictureId { get; set; }
        public int Priority { get; set; }
        public long GalleryId { get; set; }
        public bool Active { get; set; }
        public Picture Picture { get; set; }
        public Gallery Gallery { get; set; }
    }
}
