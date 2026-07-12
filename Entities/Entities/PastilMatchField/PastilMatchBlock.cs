using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities.PastilMatchField
{
    public class PastilMatchBlock : Id_Field
    {
        public long BlockerUserId { get; set; }
        public long BlockedUserId { get; set; }
        public long? PastilMatchId { get; set; }

        public bool Deleted { get; set; }
        public DateTime CreateDate { get; set; }

        public User BlockerUser { get; set; }
        public User BlockedUser { get; set; }
        public PastilMatch PastilMatch { get; set; }
    }
}
