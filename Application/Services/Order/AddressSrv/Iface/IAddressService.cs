using Application.Common.Interface;
using Application.Services.Order.AddressSrv.Dto;
using Entities.Entities;

namespace Application.Services.Order.AddressSrv.iface
{
    public interface IAddressService : ICommonSrv<Address, AddressDto>
    {
        AddressSearchDto Search(AddressInputDto baseSearchDto);
        AdminAddressSearchDto SearchAdmin(AdminAddressInputDto searchDto);
    }
}
