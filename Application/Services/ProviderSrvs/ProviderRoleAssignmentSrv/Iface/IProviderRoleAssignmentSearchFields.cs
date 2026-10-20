namespace Application.Services.ProviderSrvs.ProviderRoleAssignmentSrv.Iface
{
    public interface IProviderRoleAssignmentSearchFields
    {
        public long? ProviderId { get; set; }
        public long? ProviderRoleId { get; set; }
        public bool? Approved { get; set; }
    }
}
