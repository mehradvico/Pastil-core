using Application.Services.PastilAISrv;
using Entities.Entities.PastilAIField;
using Xunit;

namespace Application.Tests.PastilAI;

public class PastilAiQuotaPolicyTests
{
    [Fact]
    public void Free_plan_blocks_fourth_chat()
    {
        var plan = new PastilAiPlan { DailyChatLimit = 3, DailyImageLimit = 1 };
        var usage = new PastilAiDailyUsage { ChatCount = 3 };

        var error = PastilAiQuotaPolicy.Validate(plan, usage, PastilAiInputType.Text);

        Assert.NotNull(error);
    }

    [Fact]
    public void Free_plan_blocks_second_image()
    {
        var plan = new PastilAiPlan { DailyChatLimit = 3, DailyImageLimit = 1 };
        var usage = new PastilAiDailyUsage { ChatCount = 1, ImageCount = 1 };

        var error = PastilAiQuotaPolicy.Validate(plan, usage, PastilAiInputType.Image);

        Assert.NotNull(error);
    }

    [Fact]
    public void Pro_plan_with_null_limits_is_unlimited()
    {
        var plan = new PastilAiPlan { DailyChatLimit = null, DailyImageLimit = null };
        var usage = new PastilAiDailyUsage { ChatCount = int.MaxValue, ImageCount = int.MaxValue };

        var error = PastilAiQuotaPolicy.Validate(plan, usage, PastilAiInputType.Image);

        Assert.Null(error);
    }

    [Fact]
    public void Free_plan_blocks_audio_when_limit_is_zero()
    {
        var plan = new PastilAiPlan { DailyChatLimit = 3, DailyAudioLimit = 0 };
        var usage = new PastilAiDailyUsage();

        var error = PastilAiQuotaPolicy.Validate(plan, usage, PastilAiInputType.Audio);

        Assert.NotNull(error);
    }
}
