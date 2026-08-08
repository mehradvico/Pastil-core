using Application.Common.Dto.Field;

namespace Application.Services.Accounting.PetSrv.Dto
{
    public class PetVDto : Name_FieldDto
    {
        public string Label { get; set; }
        public string Slug { get; set; }
        public int Priority { get; set; }
    }
}
