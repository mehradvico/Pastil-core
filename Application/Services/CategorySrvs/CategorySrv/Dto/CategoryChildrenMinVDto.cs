using Application.Common.Dto.Field;
using System.Collections.Generic;

namespace Application.Services.CategorySrv.Dto
{
    public class CategoryChildrenMinVDto : Name_FieldDto
    {
        public long? ParentId { get; set; }
        public string Label { get; set; }
        public string Slug { get; set; }
        public int Priority { get; set; }
        public string IconUrl { get; set; }
        public long IconId { get; set; }
        public List<CategoryChildrenMinVDto> Children { get; set; } = new List<CategoryChildrenMinVDto>();
    }
}
