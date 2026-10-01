using Application.Common.Dto.Result;
using Application.Services.SchoolSrvs.SchoolCourseLiveSrv.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.SchoolSrvs.SchoolCourseLiveSrv.Iface
{
    public interface ISchoolCourseLiveService
    {
        Task<BaseResultDto<SchoolCourseLiveSessionVDto>> GetOrCreateForCompanionAsync(long schoolCourseSessionId, long companionId);
        Task<BaseResultDto<SchoolCourseLiveSessionVDto>> GetForEnrolledUserAsync(long schoolCourseSessionId, long userId);
        Task<BaseResultDto<SchoolLiveTokenVDto>> StartAsync(SchoolLiveStartDto dto, long companionId);
        Task<BaseResultDto> EndAsync(long liveSessionId, long companionId);
        Task<BaseResultDto<SchoolLiveTokenVDto>> GetViewerTokenAsync(long liveSessionId, long userId);
        Task<BaseResultDto<List<SchoolLiveParticipantVDto>>> GetParticipantsAsync(long liveSessionId, long companionId);
        Task<BaseResultDto<SchoolLiveCommentDto>> PostCommentAsync(long liveSessionId, long userId, string message);
        Task<BaseResultDto<List<SchoolLiveCommentDto>>> GetCommentsAsync(long liveSessionId, long userId);
        Task<bool> IsCompanionOwnerAsync(long liveSessionId, long companionId);
        Task<bool> IsCompanionOwnerByUserIdAsync(long liveSessionId, long userId);
        Task<bool> IsEnrolledViewerAsync(long liveSessionId, long userId, long userPetId);
        Task HandleEgressWebhookAsync(string egressId, bool succeeded, string fileUrl);

        // Hangfire recurring jobs
        Task SendClassReminderPushesAsync(CancellationToken cancellationToken);
    }
}
