using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;

namespace Application.Services.SchoolSrvs.SchoolPictureSrv.Dto
{
    public class SchoolPictureVDto : Id_FieldDto
    {
        public long SchoolId { get; set; }
        public long PictureId { get; set; }
        public string Label { get; set; }

        public PictureVDto Picture { get; set; }
    }
}
