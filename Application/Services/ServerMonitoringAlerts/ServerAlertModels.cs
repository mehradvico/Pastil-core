using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace Application.Services.ServerMonitoringAlerts
{
    public sealed class ServerAlertOptions
    {
        public const string SectionName = "ServerMonitoring:Alerts";

        public bool Enabled { get; set; } = true;
        public int PollSeconds { get; set; } = 30;
        public double MemoryPercent { get; set; } = 90;
        public double DiskPercent { get; set; } = 90;
        public double LoadPercent { get; set; } = 90;
        public double ContainerMemoryPercent { get; set; } = 90;
        // تعداد بررسی‌های متوالیِ خارج از حد برای فعال شدن هشدار (جلوگیری از هشدار کاذب لحظه‌ای)
        public int BreachChecks { get; set; } = 3;
        public int ContainerDownChecks { get; set; } = 2;
        public int AgentDownChecks { get; set; } = 3;
        public int ClearChecks { get; set; } = 2;
        // تا وقتی مشکل پابرجاست، هر چند دقیقه یک‌بار یادآوری می‌شود (۰ = بدون یادآوری)
        public int RepeatMinutes { get; set; } = 180;
        // کانتینری که این‌قدر ساعت غایب بماند «برداشته‌شده» تلقی و فراموش می‌شود
        public int ForgetContainerAfterHours { get; set; } = 24;
        public string[] IgnoredContainers { get; set; } = Array.Empty<string>();
    }

    public sealed record ServerContainerSample(string Name, double? MemoryPercent, string MemoryUsage);

    public sealed record ServerSample(
        int? CpuCores,
        double? Load1,
        int? MemTotalMb,
        int? MemUsedMb,
        string DiskUsed,
        string DiskTotal,
        double? DiskPercent,
        IReadOnlyList<ServerContainerSample> Containers)
    {
        public static ServerSample FromJson(JsonElement root)
        {
            var containers = new List<ServerContainerSample>();
            if (root.TryGetProperty("containers", out var list) && list.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in list.EnumerateArray())
                {
                    var name = Str(item, "name");
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    containers.Add(new ServerContainerSample(name, Percent(Str(item, "memPercent")), Str(item, "memUsage")));
                }
            }

            return new ServerSample(
                Int(root, "cpuCores"),
                Dbl(root, "load1"),
                Int(root, "memTotalMb"),
                Int(root, "memUsedMb"),
                Str(root, "diskUsed"),
                Str(root, "diskTotal"),
                Percent(Str(root, "diskPercent")),
                containers);
        }

        private static string Str(JsonElement e, string name) =>
            e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? string.Empty : string.Empty;

        private static int? Int(JsonElement e, string name) =>
            e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var i) ? i : null;

        private static double? Dbl(JsonElement e, string name) =>
            e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : null;

        private static double? Percent(string text) =>
            double.TryParse(text?.TrimEnd('%', ' '), NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : null;
    }

    public enum ServerAlertKind { Firing = 1, Resolved = 2 }

    /// <summary>پیام آماده‌ی ارسال؛ Key پایدار است و برای tag/collapse هم استفاده می‌شود.</summary>
    public sealed record ServerAlertMessage(string Key, ServerAlertKind Kind, string Title, string Body);
}
