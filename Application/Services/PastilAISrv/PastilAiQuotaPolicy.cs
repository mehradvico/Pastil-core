using Entities.Entities.PastilAIField;

namespace Application.Services.PastilAISrv
{
    public static class PastilAiQuotaPolicy
    {
        public static string Validate(PastilAiPlan plan, PastilAiDailyUsage usage, PastilAiInputType inputType)
        {
            if (plan.DailyChatLimit.HasValue && usage.ChatCount >= plan.DailyChatLimit.Value)
                return Resource.Notification.PastilAiChatQuotaExceeded;
            if (inputType == PastilAiInputType.Image && plan.DailyImageLimit.HasValue && usage.ImageCount >= plan.DailyImageLimit.Value)
                return Resource.Notification.PastilAiImageQuotaExceeded;
            if (inputType == PastilAiInputType.Audio && plan.DailyAudioLimit.HasValue && usage.AudioCount >= plan.DailyAudioLimit.Value)
                return Resource.Notification.PastilAiAudioQuotaExceeded;
            if (inputType == PastilAiInputType.Video && plan.DailyVideoLimit.HasValue && usage.VideoCount >= plan.DailyVideoLimit.Value)
                return Resource.Notification.PastilAiVideoQuotaExceeded;
            return null;
        }
    }
}
