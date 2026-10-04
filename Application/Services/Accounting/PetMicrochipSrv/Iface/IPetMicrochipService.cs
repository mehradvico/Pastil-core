using Application.Common.Dto.Result;
using Application.Services.Accounting.PetMicrochipSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.Accounting.PetMicrochipSrv.Iface
{
    public interface IPetMicrochipService
    {
        Task<BaseResultDto<PetMicrochipSearchResultVDto>> SearchAsync(PetMicrochipSearchDto dto, long? userId, string clientIp);
        Task<BaseResultDto<PetMicrochipFollowUpResultVDto>> RequestFollowUpAsync(PetMicrochipFollowUpDto dto, long? userId);

        Task<BaseResultDto<PetMicrochipRequestSearchVDto>> AdminSearchAsync(PetMicrochipRequestInputDto dto);
        Task<BaseResultDto<PetMicrochipRequestDetailVDto>> AdminGetAsync(long id);
        Task<BaseResultDto> AdminUpdateAsync(long id, PetMicrochipRequestUpdateDto dto, long adminUserId);
    }
}
