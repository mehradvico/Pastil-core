using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities
{
    public class ScoreTransaction : Id_Field
    {
        public long UserId { get; set; }
        public double Amount { get; set; }
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }
        public long TransactionTypeId { get; set; }
        public string ReferenceId { get; set; }

        public User User { get; set; }
        public Code TransactionType { get; set; }
    }
}
