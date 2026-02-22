using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ProductSrvs.StoreSrv.Dto
{
    public class StoreFinanceVDto : Name_FieldDto
    {
        public string Mobile { get; set; }
        public decimal CommissionPercent { get; set; }
        public bool HasCommission { get; set; }
        public int OrderCount { get; set; }
    }
}
