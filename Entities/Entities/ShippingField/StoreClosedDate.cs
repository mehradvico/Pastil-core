using Entities.Entities.CommonField;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Entities.ShippingField
{
    /// <summary>تعطیلی استثنایی یک روز کامل فروشگاه به تاریخ محلی تهران.</summary>
    public class StoreClosedDate : Id_Field
    {
        public long StoreId { get; set; }
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }
        public Store Store { get; set; }
    }
}
