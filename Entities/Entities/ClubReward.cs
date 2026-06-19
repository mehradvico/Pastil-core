using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities
{
    public class ClubReward : Name_Field
    {
        public double RequiredScore { get; set; }
        public long RebateId { get; set; }
        public int ValidityDays { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public Rebate Rebate { get; set; }
    }
}
