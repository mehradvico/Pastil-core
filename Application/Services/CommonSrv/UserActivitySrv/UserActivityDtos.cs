using System;
using System.Collections.Generic;

namespace Application.Services.CommonSrv.UserActivitySrv
{
    public class TrackActivityItemDto
    {
        public string Type { get; set; }
        public string Screen { get; set; }
        public string Target { get; set; }
        public Guid? SessionId { get; set; }
        public int? DurationSeconds { get; set; }
        /// <summary>زمان رخداد در کلاینت (UTC)؛ اگر خالی یا غیرمنطقی بود زمان سرور استفاده می‌شود.</summary>
        public DateTime? At { get; set; }
    }

    public class TrackActivityDto
    {
        public string Platform { get; set; }
        public List<TrackActivityItemDto> Events { get; set; } = new();
    }

    public class CountItemDto
    {
        public string Key { get; set; }
        public int Count { get; set; }
    }

    public class DayActivityDto
    {
        /// <summary>yyyy-MM-dd به وقت تهران</summary>
        public string Date { get; set; }
        public int Events { get; set; }
        public int Logins { get; set; }
        public int Actions { get; set; }
        public int Minutes { get; set; }
        public bool Active { get; set; }
    }

    public class ModuleActivityDto
    {
        public string Module { get; set; }
        public int Count { get; set; }
        public DateTime? LastAtUtc { get; set; }
        public List<CountItemDto> Actions { get; set; } = new();
    }

    public class TimelineItemDto
    {
        public DateTime AtUtc { get; set; }
        public string Module { get; set; }
        public string Action { get; set; }
    }

    public class FlowStepDto
    {
        public int Order { get; set; }
        public string Label { get; set; }
        public DateTime AtUtc { get; set; }
    }

    public class FlowInstanceDto
    {
        public string Flow { get; set; }
        /// <summary>app = رویداد flow ارسالی اپ | db = برداشت از رکوردهای دیتابیس</summary>
        public string Source { get; set; }
        public DateTime StartedUtc { get; set; }
        /// <summary>completed | abandoned | in_progress</summary>
        public string Status { get; set; }
        public string LastStep { get; set; }
        public List<FlowStepDto> Steps { get; set; } = new();
    }

    public class FlowSummaryDto
    {
        public string Flow { get; set; }
        public int Started { get; set; }
        public int Completed { get; set; }
        public int Abandoned { get; set; }
        public int InProgress { get; set; }
        /// <summary>مرحله‌هایی که کاربر در آن‌ها رها کرده</summary>
        public List<CountItemDto> DropOffs { get; set; } = new();
    }

    public class JourneyStepDto
    {
        public DateTime AtUtc { get; set; }
        /// <summary>screen | click | action | flow</summary>
        public string Type { get; set; }
        public string Label { get; set; }
    }

    public class JourneyDto
    {
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public List<JourneyStepDto> Steps { get; set; } = new();
    }

    public class RecentActivityDto
    {
        public DateTime AtUtc { get; set; }
        public string Type { get; set; }
        public string Screen { get; set; }
        public string Target { get; set; }
    }

    public class UserAnalyticsDto
    {
        public int Days { get; set; }
        public DateTime? RegisterDate { get; set; }
        public DateTime? LastActivityUtc { get; set; }
        public int ActiveDays { get; set; }
        public int IdleDays { get; set; }
        public int LongestIdleStreak { get; set; }
        public int CurrentActiveStreak { get; set; }
        public int TotalSessions { get; set; }
        public int TotalLogins { get; set; }
        public int TotalClicks { get; set; }
        public int TotalScreenViews { get; set; }
        public int TotalMinutes { get; set; }
        public int AvgSessionMinutes { get; set; }
        public bool HasTrackingData { get; set; }
        public List<DayActivityDto> Daily { get; set; } = new();
        /// <summary>۷ ردیف (شنبه..جمعه) × ۲۴ ستون ساعت (وقت تهران)</summary>
        public int[][] Heatmap { get; set; }
        public List<int> Weekdays { get; set; } = new();
        /// <summary>مجموع فعالیت هر ساعت (۰..۲۳، وقت تهران) در کل بازه</summary>
        public List<int> Hourly { get; set; } = new();
        public List<CountItemDto> TopScreens { get; set; } = new();
        public List<CountItemDto> TopClicks { get; set; } = new();
        public List<CountItemDto> Platforms { get; set; } = new();
        public List<CountItemDto> Devices { get; set; } = new();
        public List<ModuleActivityDto> Modules { get; set; } = new();
        public List<TimelineItemDto> Timeline { get; set; } = new();
        public List<FlowSummaryDto> FlowSummaries { get; set; } = new();
        public List<FlowInstanceDto> Flows { get; set; } = new();
        public List<JourneyDto> Journeys { get; set; } = new();
        /// <summary>بازه‌ی تحلیل (وقت تهران، yyyy-MM-dd)</summary>
        public string From { get; set; }
        public string To { get; set; }
        public List<RecentActivityDto> Recent { get; set; } = new();
    }
}
