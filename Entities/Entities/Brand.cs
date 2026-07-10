using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;

namespace Entities.Entities
{
    public class Brand : Seo_Full_Field
    {
        public string SecondName { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public long? PictureId { get; set; }
        public long? IconId { get; set; }
        public int Priority { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public ICollection<Product> Products { get; set; }
        public Picture Picture { get; set; }
        public Picture Icon { get; set; }
        public ICollection<Category> Categories { get; set; }

    }
}
