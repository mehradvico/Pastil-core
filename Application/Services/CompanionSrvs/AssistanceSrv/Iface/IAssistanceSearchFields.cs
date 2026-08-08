namespace Application.Services.CompanionSrvs.AssistanceSrv.Iface
{
    public interface IAssistanceSearchFields
    {
        public bool? IsPersonal { get; set; }
        public long? AssistanceGroupId { get; set; }
    }
}
