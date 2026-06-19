using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.ScoreTransactionSrv.Iface
{
    public interface IScoreTransactionSearchFields
    {
        public long? UserId { get; set; }
        public long? TransactionTypeId { get; set; }

    }
}
