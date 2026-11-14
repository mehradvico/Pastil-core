using Application.Common.Dto.Field;
using System;

namespace Application.Services.ProductSrvs.DiscountSrv.Dto
{
    public class DiscountDto : Id_FieldDto
    {
        public long TypeId { get; set; }
        public long DiscountGroupId { get; set; }
        public DateTime? EndDate { get; set; }
        public long StoreId { get; set; }
        public int Percent { get; set; }
        public bool Synced { get; set; }
        public long? CategoryId { get; set; }
        public long? BrandId { get; set; }
        public long? ProductId { get; set; }
        public long? ProductItemId { get; set; }
        public bool Active { get; set; }
        /// <summary>فقط ادمین می‌تواند true بفرستد؛ برای فروشنده همیشه false در نظر گرفته می‌شود.</summary>
        public bool FundedByPastil { get; set; }

    }
}
