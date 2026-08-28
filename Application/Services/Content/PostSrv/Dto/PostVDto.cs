using Application.Common.Dto.Field;
using Application.Services.CategorySrv.Dto;
using Application.Services.Content.HashtagSrv.Dto;
using Application.Services.Content.PostFileSrv.Dto;
using Application.Services.Content.PostPictureSrv.Dto;
using Application.Services.Dto;
using Application.Services.Filing.PictureSrv.Dto;
using System;
using System.Collections.Generic;

namespace Application.Services.Content.PostSrv.Dto
{
    public class PostVDto : Seo_Full_FieldDto
    {
        public string Label { get; set; }
        public string Slug { get; set; }
        public string Subject { get; set; }
        public string SubNews { get; set; }
        public long? PictureId { get; set; }
        public bool Active { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime PublishDate { get; set; }
        public bool AdminConfirm { get; set; }
        public int VisitCount { get; set; }
        public long? CategoryId { get; set; }
        public long UserId { get; set; }
        public bool IsOld { get; set; }
        public bool Edited { get; set; }
        public string PictureUrl { get; set; }
        public int CommentCount { get; set; }

        public UserVDto User { get; set; }
        public UserVDto Admin { get; set; }

        public CategoryParentVDto Category { get; set; }
        public PictureVDto Picture { get; set; }
        public List<PostFileVDto> PostFiles { get; set; }
        public List<PostPictureVDto> PostPictures { get; set; }
        public List<HashtagDto> Hashtags { get; set; }
        public List<PostVDto> Children { get; set; }

    }
}
