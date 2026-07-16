using Application.Common.Dto.Input;
using Application.Common.Enumerable;
using Application.Common.Enumerable.Code;
using Application.Services.Accounting.TicketSrv.Iface;
using System;

namespace Application.Services.Accounting.TicketSrv.Dto
{
    public class TicketInputDto : BaseInputDto, ITicketSearchFields
    {
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
    }
}