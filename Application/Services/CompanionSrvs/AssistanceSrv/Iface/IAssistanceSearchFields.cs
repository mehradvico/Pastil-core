namespace Application.Services.CompanionSrvs.AssistanceSrv.Iface
{
    public interface IAssistanceSearchFields
    {
        public bool? IsPersonal { get; set; }
        public long? AssistanceGroupId { get; set; }
        public bool? ShowToSite { get; set; }
        public long? ProviderRoleId { get; set; }
        public string ProviderRole { get; set; }
    }
}
