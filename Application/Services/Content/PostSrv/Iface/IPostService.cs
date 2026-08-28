using Application.Common.Dto.Result;
using Application.Common.Interface;
using Application.Services.Content.PostSrv.Dto;
using Entities.Entities;
using System.Threading.Tasks;

namespace Application.Services.Content.PostSrv.Iface
{
    public interface IPostService : ICommonSrv<Post, PostDto>
    {
        PostSearchDto Search(PostInputDto baseSearchDto);

        Task<BaseResultDto<PostVDto>> FindAsyncVDto(long id, bool visit);
        Task<BaseResultDto<PostVDto>> GetByUrlAsyncVDto(string url, bool visit);
        BaseResultDto Confirm(PostConfirmDto dto);
        void UpdatePostCommentCount(long postId);
        BaseResultDto ChangeUser(ChangePostUserDto dto);
        BaseResultDto GetSiteMap();
        Task<BaseResultDto<bool>> CheckLabelAvailableAsync(string label, long? excludeId);
    }
}
