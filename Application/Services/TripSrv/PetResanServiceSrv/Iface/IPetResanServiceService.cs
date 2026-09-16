using Application.Common.Dto.Result;
using Application.Services.TripSrv.PetResanServiceSrv.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.TripSrv.PetResanServiceSrv.Iface
{
    public interface IPetResanServiceService
    {
        Task<BaseResultDto<PetResanServiceVDto>> InsertAsyncDto(PetResanServiceCreateDto dto, long userId);
        Task<BaseResultDto<List<PetResanServiceVDto>>> GetMyListAsync(long userId);
        Task<BaseResultDto<PetResanServiceVDto>> FindAsyncVDto(long id, long userId);
        Task<BaseResultDto> CancelAsync(long id, long userId);

        /// <summary>پیش‌نمایش قیمت هر رفت‌وبرگشت، قبل از ثبت نهایی سرویس.</summary>
        Task<BaseResultDto<double>> PreviewPriceAsync(PetResanServiceCreateDto dto);
    }
}
