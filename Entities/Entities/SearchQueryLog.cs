using Entities.Entities.CommonField;
using System;
using System.ComponentModel.DataAnnotations;

namespace Entities.Entities
{
    public class SearchQueryLog : Id_Field
    {
        [Required, MaxLength(100)]
        public string Query { get; set; }

        [Required, MaxLength(100)]
        public string NormalizedQuery { get; set; }

        [Required, MaxLength(20)]
        public string Channel { get; set; }

        public int ResultCount { get; set; }
        public long TookMilliseconds { get; set; }
        public DateTime CreateDateUtc { get; set; }
    }
}
