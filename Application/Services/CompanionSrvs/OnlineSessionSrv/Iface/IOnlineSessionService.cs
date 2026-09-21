using Application.Common.Dto.Result;
using Application.Services.CompanionSrvs.OnlineSessionSrv.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.CompanionSrvs.OnlineSessionSrv.Iface
{
    public interface IOnlineSessionService
    {
        Task<BaseResultDto<OnlineSessionVDto>> StartAsync(OnlineSessionStartDto dto);
        Task<BaseResultDto<OnlineSessionVDto>> FindAsync(long id);
        Task<BaseResultDto<List<OnlineSessionUserVDto>>> SearchUsersAsync(string q);
        Task<BaseResultDto<List<OnlineSessionVDto>>> ActiveAsync();
        Task<BaseResultDto<List<OnlineSessionMessageVDto>>> MessagesAsync(OnlineSessionMessageInputDto dto);
        Task<BaseResultDto<OnlineSessionMessageVDto>> SendMessageAsync(OnlineSessionMessageSendDto dto);
        Task<BaseResultDto> ReadAsync(OnlineSessionMessageReadDto dto);
    }
}
