using Entities.Entities.CommonField;
using System.Collections.Generic;

namespace Entities.Entities.Security
{
    public class Role : Id_Field, ISlugEntity
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Slug { get; set; }
        public ICollection<Permission> Permissions { get; set; }
        public ICollection<User> Users { get; set; }

        public string GetSlugSource() => Label;
    }
}
