using Application.Common.Dto.Field;
using Entities.Entities;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Accounting.ScoreTransactionSrv.Dto
{
    public class ScoreTransactionDto : Id_FieldDto
    {
        public long UserId { get; set; }
        public double Amount { get; set; }
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }
        public long TransactionTypeId { get; set; }
        public string ReferenceId { get; set; }
    }
}
