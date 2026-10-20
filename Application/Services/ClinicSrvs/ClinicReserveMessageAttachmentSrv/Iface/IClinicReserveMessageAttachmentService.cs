using Entities.Entities.ClinicField;
using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.ClinicSrvs.ClinicReserveMessageAttachmentSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.ClinicSrvs.ClinicReserveMessageAttachmentSrv.Iface
{
    public interface IClinicReserveMessageAttachmentService : ICommonSrv<ClinicReserveMessageAttachment, ClinicReserveMessageAttachmentDto>
    {
        ClinicReserveMessageAttachmentSearchDto Search(ClinicReserveMessageAttachmentInputDto dto);
        Task<BaseResultDto<ClinicReserveMessageAttachmentVDto>> FindAsyncVDto(long id);
    }
}
