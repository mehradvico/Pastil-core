using Application.Common.Dto.Field;
using Application.Services.Filing.PictureSrv.Dto;
using System;

namespace Application.Services.MemorySrvs.MemorySrv.Dto
{
    public class MemoryVDto : Id_FieldDto
    {
        public string Text { get; set; }
        public DateTimeOffset MemoryDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public long? PictureId { get; set; }
        public PictureVDto Picture { get; set; }
        public long UserPetId { get; set; }
        public string UserPetName { get; set; }
        public PictureVDto UserPetPicture { get; set; }
        public long UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserMobile { get; set; }
    }
}
