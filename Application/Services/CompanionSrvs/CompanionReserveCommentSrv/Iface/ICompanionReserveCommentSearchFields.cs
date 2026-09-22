namespace Application.Services.CompanionSrvs.CompanionReserveCommentSrv.Iface
{
    public interface ICompanionReserveCommentSearchFields
    {
        public long? CompanionReserveId { get; set; }
        public long? CompanionId { get; set; }
        public bool? AllStatus { get; set; }
        public long? UserId { get; set; }
    }
}
