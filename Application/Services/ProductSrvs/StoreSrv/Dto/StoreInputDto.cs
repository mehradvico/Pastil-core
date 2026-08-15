using Application.Common.Dto.Input;
using Application.Services.StoreSrv.Iface;

namespace Application.Services.ProductSrvs.StoreSrv.Dto
{
    public class StoreInputDto : BaseInputDto, IStoreSearchFields
    {
        public long? UserId { get; set; }
        public long? TypeId { get; set; }
        public long? CityId { get; set; }
        public long? StateId { get; set; }
        public bool? ShowToSite { get; set; }
        public bool? Approved { get; set; }

    }
}
