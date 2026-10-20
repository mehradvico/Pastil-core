namespace Application.Services.ProviderSrvs.ProviderSubscriptionSrv.Iface
{
    public interface IProviderSubscriptionSearchFields
    {
        public long? ProviderId { get; set; }
        public long? PlanId { get; set; }
        public bool? ActiveNow { get; set; }
    }
}
