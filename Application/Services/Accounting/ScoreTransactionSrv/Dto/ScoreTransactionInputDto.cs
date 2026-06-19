using Application.Common.Dto.Input;
using Application.Common.Enumerable;
using Application.Services.Accounting.ScoreTransactionSrv.Iface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.ScoreTransactionSrv.Dto
{
    public class ScoreTransactionInputDto : BaseInputDto, IScoreTransactionSearchFields
    {
        public long? UserId { get; set; }
        public long? TransactionTypeId { get; set; }
    }
}
