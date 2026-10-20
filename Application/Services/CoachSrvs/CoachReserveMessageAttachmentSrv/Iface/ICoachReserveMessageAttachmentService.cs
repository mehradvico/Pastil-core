using Entities.Entities.CoachField;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.CoachSrvs.CoachReserveMessageAttachmentSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.CoachSrvs.CoachReserveMessageAttachmentSrv.Iface
{
    public interface ICoachReserveMessageAttachmentService : ICommonSrv<CoachReserveMessageAttachment, CoachReserveMessageAttachmentDto>
    {
        CoachReserveMessageAttachmentSearchDto Search(CoachReserveMessageAttachmentInputDto dto);
        Task<BaseResultDto<CoachReserveMessageAttachmentVDto>> FindAsyncVDto(long id);
    }
}
