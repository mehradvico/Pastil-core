namespace Application.Services.StoreSrv.Iface
{
    public interface IStoreSearchFields
    {
        public long? UserId { get; set; }
        public long? TypeId { get; set; }
        public long? CityId { get; set; }
        public long? StateId { get; set; }
        public bool? ShowToSite { get; set; }

    }
}
