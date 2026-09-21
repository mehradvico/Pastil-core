using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities
{
    public class UserBankCard : Id_Field
    {
        public long UserId { get; set; }
        public string CardNumber { get; set; }
        public string ShebaNumber { get; set; }
        /// <summary>شاخص کور (HMAC-SHA256 hex) شماره کارت برای چک تکراری‌بودن؛ خود CardNumber رمزشده ذخیره می‌شود.</summary>
        public string CardNumberHash { get; set; }
        public long BankCardId { get; set; }
        public string CardHolderName { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public bool Approved { get; set; }
        public string AdminDetail { get; set; }
        public bool Deleted { get; set; }

        public BankCard BankCard { get; set; }
        public User User { get; set; }
    }
}
