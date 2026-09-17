using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CompanionSrvs.CompanionReserveMessageAttachmentSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.CompanionReserveMessageAttachmentSrv.Iface
{
    public interface ICompanionReserveMessageAttachmentService : ICommonSrv<CompanionReserveMessageAttachment, CompanionReserveMessageAttachmentDto>
    {
        CompanionReserveMessageAttachmentSearchDto Search(CompanionReserveMessageAttachmentInputDto dto);
        Task<BaseResultDto<CompanionReserveMessageAttachmentVDto>> FindAsyncVDto(long id);
    }
}
