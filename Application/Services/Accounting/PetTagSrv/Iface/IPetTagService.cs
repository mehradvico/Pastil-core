using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Accounting.PetTagSrv.Dto;
using Entities.Entities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Application.Services.Accounting.PetTagSrv.Iface
{
    public interface IPetTagService : ICommonSrv<PetTag, PetTagDto>
    {
        Task<BaseResultDto<PetTagGenerateResultDto>> GenerateAsync(int count);
        BaseSearchDto<PetTagVDto> Search(PetTagInputDto dto);
        Task<BaseResultDto<PetTagPublicStatusDto>> GetPublicStatusAsync(string code);
        Task<BaseResultDto> ClaimAsync(string code, long userPetId, long currentUserId);
        Task<BaseResultDto<List<PetTagMineItemDto>>> GetMineAsync(long currentUserId);
        Task<MemoryStream> GetExcelAsync(PetTagExportFilterDto filter);
    }
}
