using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.TripSrv.PriceCalculationSrv.Dto;
using Entities.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.TripSrv.TripAddressSrv.Iface
{
    public interface ITripAddressService : ICommonSrv<TripAddress, TripAddressDto>
    {
        TripAddressSearchDto Search(TripAddressInputDto baseSearchDto);
        Task<BaseResultDto<TripAddressVDto>> FindAsyncVDto(long id);
        Task<BaseResultDto> SelectAsync(long id, long userId);
    }
}
