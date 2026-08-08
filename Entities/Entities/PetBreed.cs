using Entities.Entities.CommonField;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities
{
    public class PetBreed : Name_Field, ISlugEntity
    {
        public long PetId { get; set; }
        public string Label { get; set; }
        public string Slug { get; set; }
        public long? PictureId { get; set; }
        public int Priority { get; set; }
        public bool Deleted { get; set; }

        public Pet Pet { get; set; }
        public Picture Picture { get; set; }

        public string GetSlugSource() => Label;
    }
}
