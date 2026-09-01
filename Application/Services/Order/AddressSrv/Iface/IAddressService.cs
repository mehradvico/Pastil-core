using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Order.AddressSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.Order.AddressSrv.iface
{
    public interface IAddressService : ICommonSrv<Address, AddressDto>
    {
        AddressSearchDto Search(AddressInputDto baseSearchDto);
        AdminAddressSearchDto SearchAdmin(AdminAddressInputDto searchDto);
        Task<BaseResultDto> SelectAsync(long id, long userId);
    }
}
