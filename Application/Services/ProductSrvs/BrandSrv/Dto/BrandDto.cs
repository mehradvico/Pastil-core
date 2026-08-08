using Application.Common.Dto.Field;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.ProductSrvs.BrandSrv.Dto
{
    public class BrandDto : Seo_Full_FieldDto
    {
        public string Slug { get; set; }
        public long? PictureId { get; set; }
        public long? IconId { get; set; }
        [Display(Name = nameof(Resource.Field.Priority), ResourceType = typeof(Resource.Field))]

        public int Priority { get; set; }
        [Display(Name = nameof(Resource.Field.Active), ResourceType = typeof(Resource.Field))]

        public bool Active { get; set; }
        public string SecondName { get; set; }


    }
}
