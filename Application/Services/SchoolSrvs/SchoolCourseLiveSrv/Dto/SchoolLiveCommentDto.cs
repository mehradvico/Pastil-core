using Application.Common.Dto.Field;
using System;

namespace Application.Services.SchoolSrvs.SchoolCourseLiveSrv.Dto
{
    public class SchoolLiveCommentDto : Id_FieldDto
    {
        public long SchoolCourseLiveSessionId { get; set; }
        public long UserId { get; set; }
        public string Message { get; set; }
        public DateTime CreateDate { get; set; }
        public string UserName { get; set; }
        public string UserPicture { get; set; }
    }
}
