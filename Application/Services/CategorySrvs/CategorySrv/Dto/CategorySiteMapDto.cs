using Application.Common.Dto.Field;
using System;

namespace Application.Services.CategorySrv.Dto
{
    public class CategorySiteMapDto : Id_FieldDto
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Slug { get; set; }
        public DateTime UpdateDate { get; set; }

    }
}
