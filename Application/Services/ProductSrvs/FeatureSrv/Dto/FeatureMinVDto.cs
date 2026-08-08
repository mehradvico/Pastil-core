using Application.Common.Dto.Field;
using Application.Services.Setting.CodeSrv.Dto;

namespace Application.Services.ProductSrvs.FeatureSrv.Dto
{
    public class FeatureMinVDto : Name_FieldDto
    {
        public string Label { get; set; }
        public string Slug { get; set; }
        public int Priority { get; set; }
        public bool Hide { get; set; }
        public bool IsGroup { get; set; }

        public bool InSearch { get; set; }
        public long TypeId { get; set; }
        public CodeVDto Type { get; set; }

    }
}
