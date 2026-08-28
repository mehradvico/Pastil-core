using Application.Common.Dto.Field;
using Application.Services.Content.PostFileSrv.Dto;
using Application.Services.Content.PostPictureSrv.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Application.Services.Content.PostSrv.Dto
{
    public class PostDto : Seo_Full_FieldDto
    {
        [Display(Name = nameof(Resource.Field.Label), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
          ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public string Label { get; set; }
        public string Slug { get; set; }
        public long? PictureId { get; set; }
        [Display(Name = nameof(Resource.Field.Active), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
        ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public bool Active { get; set; }
        [IgnoreDataMember]
        public DateTime CreateDate { get; set; }
        [IgnoreDataMember]
        public PictureDto Picture { get; set; }
        [Display(Name = nameof(Resource.Field.PublishDate), ResourceType = typeof(Resource.Field))]
        [Required(ErrorMessageResourceType = typeof(Resource.Pattern),
ErrorMessageResourceName = nameof(Resource.Pattern.PleaseInsertT1))]
        public DateTime PublishDate { get; set; }
        public bool AdminConfirm { get; set; }
        public int VisitCount { get; set; }
        public string Subject { get; set; }
        public string SubNews { get; set; }
        public long? CategoryId { get; set; }
        public long UserId { get; set; }
        public bool IsOld { get; set; }
        public string PictureUrl { get; set; }
        [IgnoreDataMember]
        public long ParentId { get; set; }
        public List<long> CategoryIds { get; set; }
        public List<string> HashTagList { get; set; }
        public List<PostFileDto> PostFilesList { get; set; }
        public List<PostPictureDto> PostPicturesList { get; set; }
    }
}
