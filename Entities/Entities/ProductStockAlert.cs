using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;

namespace Entities.Entities
{
    /// <summary>
    /// درخواست کاربر برای دریافت اعلان موجودشدن محصول.
    /// </summary>
    public class ProductStockAlert : Id_Field
    {
        public long UserId { get; set; }
        public long ProductId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreateDate { get; set; }
        public DateTime? NotifiedDate { get; set; }
        public long? NotifiedStoreId { get; set; }

        public User User { get; set; }
        public Product Product { get; set; }
        public Store NotifiedStore { get; set; }
    }
}
