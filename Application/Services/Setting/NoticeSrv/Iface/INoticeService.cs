using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Setting.NoticeSrv.Dto;
using Entities.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Setting.NoticeSrv.Iface
{
    public interface INoticeService : ICommonSrv<Notice, NoticeDto>
    {
        NoticeSearchDto Search(NoticeInputDto dto);
        Task<BaseResultDto<NoticeDto>> ReadAsync(long id);
        Task<BaseResultDto<NoticeDto>> CreateAsync(NoticeCreateDto dto);
        Task<BaseResultDto<NoticeBulkReadVDto>> ReadBulkAsync(NoticeBulkReadDto dto);
        Task<List<NoticeTypeVDto>> GetTypesAsync(bool activeOnly = true);
        Task<int> GetUnreadCountAsync();
        Task<int> ArchiveExpiredAsync();
    }
}
