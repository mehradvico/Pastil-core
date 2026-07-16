using Application.Common.Dto.Field;
using Application.Services.Accounting.TicketItemSrv.Dto;
using Application.Services.Dto;
using Application.Services.ProductSrvs.ProductSrv.Dto;
using Application.Services.Setting.CodeSrv.Dto;
using System;

namespace Application.Services.Accounting.TicketSrv.Dto
{
    public class TicketVDto : Id_FieldDto
    {
        public string Name { get; set; }
        public long UserId { get; set; }
        public long? AdminId { get; set; }
        public long StatusId { get; set; }
        public long ImportanceId { get; set; }
        public long TicketCategoryId { get; set; }
        public long? ProductId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public bool CanReply { get; set; }
        public int UnreadCount { get; set; }
        public UserVDto User { get; set; }
        public UserVDto Admin { get; set; }
        public CodeVDto Status { get; set; }
        public CodeVDto Importance { get; set; }
        public CodeVDto TicketCategory { get; set; }
        public ProductMinVDto Product { get; set; }
        public TicketItemMinVDto LastMessage { get; set; }
    }
}