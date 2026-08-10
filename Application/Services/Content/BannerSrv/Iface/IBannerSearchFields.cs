namespace Application.Services.Content.BannerSrv.Iface
{
    public interface IBannerSearchFields
    {
        public long? CategoryId { get; set; }
        public string CategoryLabel { get; set; }
        public bool? ShowToApp { get; set; }
        public bool? ShowToSite { get; set; }

    }
}
