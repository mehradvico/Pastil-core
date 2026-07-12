using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.PastilMatchField
{
    public class PastilMatchProfileLike : Id_Field
    {
        public long LikerProfileId { get; set; }
        public long LikedProfileId { get; set; }

        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }

        public PastilMatchProfile LikerProfile { get; set; }
        public PastilMatchProfile LikedProfile { get; set; }
    }
}
