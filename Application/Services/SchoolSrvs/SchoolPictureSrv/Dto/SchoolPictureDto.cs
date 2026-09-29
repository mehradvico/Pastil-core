using Application.Common.Dto.Field;

namespace Application.Services.SchoolSrvs.SchoolPictureSrv.Dto
{
    public class SchoolPictureDto : Id_FieldDto
    {
        public long SchoolId { get; set; }
        public long PictureId { get; set; }
        public string Label { get; set; }
    }
}
