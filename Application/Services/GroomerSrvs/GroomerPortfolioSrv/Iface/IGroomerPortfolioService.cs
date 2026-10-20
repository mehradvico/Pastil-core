using Application.Common.Dto.Result;
using Application.Services.GroomerSrvs.GroomerPortfolioSrv.Dto;
using System.Threading.Tasks;

namespace Application.Services.GroomerSrvs.GroomerPortfolioSrv.Iface
{
    /// <summary>
    /// نمونه‌کار آرایشگاه به شکل پست‌های اینستاگرامی با لایک و کامنت.
    /// ownerUserId = null یعنی ادمین؛ در غیر این صورت فقط مالک همان آرایشگاه اجازه دارد.
    /// </summary>
    public interface IGroomerPortfolioService
    {
        Task<BaseResultDto<GroomerPortfolioPostVDto>> CreateAsync(GroomerPortfolioPostInputDto dto, long? ownerUserId);
        Task<BaseResultDto<GroomerPortfolioPostVDto>> UpdateAsync(GroomerPortfolioPostInputDto dto, long? ownerUserId);
        Task<BaseResultDto> DeleteAsync(long postId, long? ownerUserId);
        Task<BaseResultDto> ModerateAsync(GroomerPortfolioModerationDto dto);

        /// <summary>فید عمومی (فقط پست‌های تأییدشده‌ی آرایشگاه‌های فعال)؛ userId برای LikedByMe.</summary>
        Task<GroomerPortfolioPage<GroomerPortfolioPostVDto>> SearchPublicAsync(GroomerPortfolioPostSearchInputDto input, long? userId);
        Task<BaseResultDto<GroomerPortfolioPostVDto>> GetPublicAsync(long postId, long? userId);
        /// <summary>لیست مدیریتی (همه‌ی وضعیت‌ها)؛ ownerUserId برای محدودکردن به آرایشگاه‌های مالک.</summary>
        Task<GroomerPortfolioPage<GroomerPortfolioPostVDto>> SearchManageAsync(GroomerPortfolioPostSearchInputDto input, long? ownerUserId);

        Task<BaseResultDto<GroomerPortfolioLikeResultDto>> LikeAsync(long postId, long userId);
        Task<BaseResultDto<GroomerPortfolioLikeResultDto>> UnlikeAsync(long postId, long userId);

        Task<BaseResultDto<GroomerPortfolioCommentVDto>> AddCommentAsync(GroomerPortfolioCommentInputDto dto, long userId);
        Task<GroomerPortfolioPage<GroomerPortfolioCommentVDto>> SearchCommentsAsync(GroomerPortfolioCommentSearchInputDto input, long? userId);
        /// <summary>حذف کامنت توسط نویسنده، مالک آرایشگاه یا ادمین (isAdmin).</summary>
        Task<BaseResultDto> DeleteCommentAsync(long commentId, long userId, bool isAdmin);
    }
}
