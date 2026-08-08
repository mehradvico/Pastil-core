using Application.Common.Dto.Result;
using AutoMapper;
using Entities.Entities;
using System.Linq;

namespace Application.Services.Order.AddressSrv.Dto
{
    public class AdminAddressSearchDto : BaseSearchDto<Address, AdminAddressVDto>
    {
        public AdminAddressSearchDto(
            AdminAddressInputDto dto,
            IQueryable<Address> list,
            IMapper mapper) : base(dto, list, mapper)
        {
            UserId = dto.UserId;
            CityId = dto.CityId;
            StateId = dto.StateId;
            PostalCode = dto.PostalCode;
        }

        public long? UserId { get; set; }
        public long? CityId { get; set; }
        public long? StateId { get; set; }
        public string PostalCode { get; set; }
    }
}
