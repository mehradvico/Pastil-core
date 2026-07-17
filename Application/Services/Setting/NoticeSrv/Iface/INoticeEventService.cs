using Application.Common.Dto.Result;
using Application.Services.Setting.NoticeSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.Setting.NoticeSrv.Iface
{
    public interface INoticeEventService
    {
        Task<BaseResultDto<NoticeDto>> CreateAsync(NoticeCreateDto dto);
    }
}
