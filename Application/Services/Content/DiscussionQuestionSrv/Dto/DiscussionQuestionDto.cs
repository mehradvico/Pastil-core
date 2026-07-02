using Application.Common.Dto.Field;
using Application.Services.Dto;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using System;
using System.Text.Json.Serialization;

namespace Application.Services.Content.DiscussionQuestionSrv.Dto
{
    public class DiscussionQuestionDto : Id_FieldDto
    {
        public string Content { get; set; }
        public DateTime CreateDate { get; set; }
        public long ProductId { get; set; }
        public long UserId { get; set; }
        public bool? AdminConfirm { get; set; }
        public bool Active { get; set; }
        public int AnswerCount { get; set; }
    }
}
