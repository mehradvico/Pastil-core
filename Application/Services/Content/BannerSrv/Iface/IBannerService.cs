using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Content.BannerSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.Content.BannerSrv.Iface
{
    public interface IBannerService : ICommonSrv<Banner, BannerDto>
    {
        Task<BaseResultDto<BannerVDto>> FindAsyncVDto(long id);

        BaseSearchDto<BannerVDto> Search(BannerInputDto searchDto);
    }
}
