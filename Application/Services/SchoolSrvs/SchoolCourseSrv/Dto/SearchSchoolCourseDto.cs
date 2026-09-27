using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;

namespace Application.Services.SchoolSrvs.SchoolCourseSrv.Dto
{
    public class SearchSchoolCourseDto : Name_FieldDto
    {
        public long SchoolId { get; set; }
        public string SchoolName { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public PictureVDto Picture { get; set; }
    }
}
