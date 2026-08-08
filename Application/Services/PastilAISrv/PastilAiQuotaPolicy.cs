using Entities.Entities.PastilAIField;

namespace Application.Services.PastilAISrv
{
    public static class PastilAiQuotaPolicy
    {
        public static string Validate(PastilAiPlan plan, PastilAiDailyUsage usage, PastilAiInputType inputType)
        {
            if (plan.DailyChatLimit.HasValue && usage.ChatCount >= plan.DailyChatLimit.Value)
                return "سهمیه روزانه گفت‌وگوی شما تمام شده است.";
            if (inputType == PastilAiInputType.Image && plan.DailyImageLimit.HasValue && usage.ImageCount >= plan.DailyImageLimit.Value)
                return "سهمیه روزانه ارسال تصویر شما تمام شده است.";
            if (inputType == PastilAiInputType.Audio && plan.DailyAudioLimit.HasValue && usage.AudioCount >= plan.DailyAudioLimit.Value)
                return "سهمیه روزانه ارسال صوت شما تمام شده است.";
            if (inputType == PastilAiInputType.Video && plan.DailyVideoLimit.HasValue && usage.VideoCount >= plan.DailyVideoLimit.Value)
                return "سهمیه روزانه ارسال ویدیو شما تمام شده است.";
            return null;
        }
    }
}
