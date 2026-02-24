using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities
{
    public class Settlement : Id_Field
    {
        public long? StoreId { get; set; }
        public long? CompanionId { get; set; }
        public DateTime CreateDate { get; set; }
        public long UserBankCardId { get; set; }
        public string TrackingCode { get; set; }
        public double PaidPrice { get; set; }
        public long ItemCount { get; set; }

        public UserBankCard UserBankCard { get; set; }
        public Companion Companion { get; set; }
        public Store Store { get; set; }
    }
}
