using Application.Common.Dto.Input;
using Application.Services.Content.BannerSrv.Iface;

namespace Application.Services.Content.BannerSrv.Dto
{
    public class BannerInputDto : BaseInputDto, IBannerSearchFields
    {
        public long? CategoryId { get; set; }
        public string CategoryLabel { get; set; }
        public bool? ShowToApp { get; set; }
        public bool? ShowToSite { get; set; }
    }
}
