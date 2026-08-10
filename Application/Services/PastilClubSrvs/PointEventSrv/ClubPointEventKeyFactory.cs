using Application.Services.PastilClubSrvs.PointEventSrv.Dto;
using System;

namespace Application.Services.PastilClubSrvs.PointEventSrv
{
    public static class ClubPointEventKeyFactory
    {
        private static readonly TimeZoneInfo TehranTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");

        public static string BuildAwardKey(ClubPointEventDto dto) =>
            $"club-point:{dto.EventType}:{dto.SourceKey.Trim()}".ToLowerInvariant();

        public static string BuildReverseKey(ClubPointEventDto dto) =>
            $"club-point:reverse:{dto.EventType}:{dto.SourceKey.Trim()}".ToLowerInvariant();

        public static string BuildMemorySourceKey(long userId, DateTimeOffset memoryDate)
        {
            var tehranDate = TimeZoneInfo.ConvertTime(memoryDate, TehranTimeZone).Date;
            return $"{userId}:{tehranDate:yyyy-MM-dd}";
        }
    }
}
