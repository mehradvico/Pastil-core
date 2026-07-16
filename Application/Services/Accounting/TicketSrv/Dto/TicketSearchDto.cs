using Application.Common.Dto.Result;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Services.Accounting.TicketSrv.Iface;
using System;
using System.Collections.Generic;

namespace Application.Services.Accounting.TicketSrv.Dto
{
    public class TicketSearchDto : BaseSearchDto<TicketVDto>, ITicketSearchFields
    {
        public TicketSearchDto(TicketInputDto dto) : base(dto)
        {
            DateFrom = dto.DateFrom;
            DateTo = dto.DateTo;
            UserId = dto.UserId;
            AdminId = dto.AdminId;
            AllAdminId = dto.AllAdminId;
            Status = dto.Status;
            Importance = dto.Importance;
            TicketCategory = dto.TicketCategory;
            ProductId = dto.ProductId;
            IsAssigned = dto.IsAssigned;
            HasUnreadMessages = dto.HasUnreadMessages;
            List = new List<TicketVDto>();
        }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public long? UserId { get; set; }
        public long? AdminId { get; set; }
        public bool AllAdminId { get; set; }
        public TicketStatusEnum? Status { get; set; }
        public TicketImportanceEnum? Importance { get; set; }
        public TicketCategoryEnum? TicketCategory { get; set; }
        public long? ProductId { get; set; }
        public bool? IsAssigned { get; set; }
        public bool? HasUnreadMessages { get; set; }
        public int TotalUnreadCount { get; set; }
    }
}