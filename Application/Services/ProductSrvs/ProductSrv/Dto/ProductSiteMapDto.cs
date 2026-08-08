using Application.Common.Dto.Field;
using System;

namespace Application.Services.ProductSrv.Dto
{
    public class ProductSiteMapDto : Id_FieldDto
    {
        public string Slug { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
