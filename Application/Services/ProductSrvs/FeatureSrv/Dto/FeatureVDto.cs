using Application.Common.Dto.Field;
using System.Collections.Generic;

namespace Application.Services.ProductSrvs.FeatureSrv.Dto
{
    public class FeatureVDto : Name_FieldDto
    {
        public string Label { get; set; }
        public string Slug { get; set; }
        public int Priority { get; set; }
        public bool Hide { get; set; }
        public bool InSearch { get; set; }
        public bool IsGroup { get; set; }

        public long TypeId { get; set; }
        public bool Active { get; set; }

        public List<FeatureItemVDto> FeatureItems { get; set; }


    }
}
