using Application.Common.Dto.Result;
using Application.Services.StoreSrv.Iface;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.ProductSrvs.StoreSrv.Dto
{
    public class StoreSearchDto : BaseSearchDto<Store, StoreVDto>, IStoreSearchFields
    {
        public StoreSearchDto(StoreInputDto dto, IQueryable<Store> list, IMapper mapper) : base(dto, list, mapper)
        {
            UserId = dto.UserId;
            TypeId = dto.TypeId;
            CityId = dto.CityId;
            StateId = dto.StateId;
            ShowToSite = dto.ShowToSite;
            Approved = dto.Approved;
        }
        public long? UserId { get; set; }
        public long? TypeId { get; set; }

        public long? CityId { get; set; }
        public long? StateId { get; set; }
        public bool? ShowToSite { get; set; }
        public bool? Approved { get; set; }

    }
}
