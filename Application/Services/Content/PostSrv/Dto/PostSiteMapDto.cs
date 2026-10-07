using Application.Common.Dto.Field;
using System;

namespace Application.Services.Content.PostSrv.Dto
{
    public class PostSiteMapDto : Id_FieldDto
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string CategoryName { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
