namespace Application.Services.ProviderSrvs.ProviderSrv.Iface
{
    public interface IProviderSearchFields
    {
        public bool? Active { get; set; }
        public bool? Approved { get; set; }
        public long? PlanId { get; set; }
        public long? ProviderRoleId { get; set; }
    }
}
