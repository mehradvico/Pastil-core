using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities
{
    public class UserRebate : Id_Field
    {
        public long UserId { get; set; }
        public long RebateId { get; set; }
        public int UsageCount { get; set; }

        public User User { get; set; }
        public Rebate Rebate { get; set; }
    }
}
