using System.Globalization;
using System.Net.Sockets;
using System.Text.Json;

namespace MonitorAgent.Services;

public sealed class HostSnapshotService : IDisposable
{
    private const string HostProcDirectory = "/host/proc";
    private readonly HttpClient _dockerClient;

    public HostSnapshotService()
    {
        var handler = new SocketsHttpHandler
        {
            ConnectCallback = async (_, cancellationToken) =>
            {
                var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                try
                {
                    await socket.ConnectAsync(
                        new UnixDomainSocketEndPoint("/var/run/docker.sock"),
                        cancellationToken);
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            }
        };

        _dockerClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://docker/"),
            Timeout = TimeSpan.FromSeconds(4)
        };
    }

    public async Task<HostSnapshot> CollectAsync(CancellationToken cancellationToken)
    {
        var memory = ReadMemory();
        var disk = ReadDisk();
        var containers = await ReadContainersAsync(cancellationToken);

        return new HostSnapshot(
            DateTimeOffset.UtcNow,
            ReadCpuCoreCount(),
            ReadLoadAverage(),
            memory.TotalMb,
            memory.UsedMb,
            disk.Used,
            disk.Total,
            disk.Percent,
            ReadUptimeSeconds(),
            containers);
    }

    private int? ReadCpuCoreCount()
    {
        try
        {
            var cpuInfo = File.ReadAllLines(Path.Combine(HostProcDirectory, "cpuinfo"));
            var count = cpuInfo.Count(line => line.StartsWith("processor", StringComparison.Ordinal));
            return count > 0 ? count : null;
        }
        catch
        {
            return null;
        }
    }

    private double? ReadLoadAverage()
    {
        try
        {
            var value = File.ReadAllText(Path.Combine(HostProcDirectory, "loadavg"))
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var load)
                ? load
                : null;
        }
        catch
        {
            return null;
        }
    }

    private (int? TotalMb, int? UsedMb) ReadMemory()
    {
        try
        {
            var values = File.ReadLines(Path.Combine(HostProcDirectory, "meminfo"))
                .Select(line => line.Split(':', 2))
                .Where(parts => parts.Length == 2)
                .ToDictionary(
                    parts => parts[0],
                    parts => ParseKilobytes(parts[1]),
                    StringComparer.Ordinal);

            if (!values.TryGetValue("MemTotal", out var totalKb)) return (null, null);

            // MemAvailable reflects reclaimable page cache and is a more useful
            // "used" figure than MemFree for an operations dashboard.
            values.TryGetValue("MemAvailable", out var availableKb);
            var usedKb = Math.Max(0, totalKb - availableKb);

            return ((int)(totalKb / 1024), (int)(usedKb / 1024));
        }
        catch
        {
            return (null, null);
        }
    }

    private static long ParseKilobytes(string value)
    {
        var number = value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return long.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;
    }

    private static (string? Used, string? Total, string? Percent) ReadDisk()
    {
        try
        {
            // /host/root is a read-only bind mount of the server's root file
            // system, so this measures the host disk—not the agent overlay.
            var root = new DriveInfo("/host/root");
            var total = root.TotalSize;
            if (total <= 0) return (null, null, null);

            var used = Math.Max(0, total - root.AvailableFreeSpace);
            var percent = Math.Round((double)used / total * 100, 1);
            return (FormatBytes(used), FormatBytes(total), $"{percent.ToString(CultureInfo.InvariantCulture)}%");
        }
        catch
        {
            return (null, null, null);
        }
    }

    private static string FormatBytes(long bytes)
    {
        var units = new[] { "B", "KiB", "MiB", "GiB", "TiB" };
        var value = (double)bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value.ToString(value >= 10 || unit == 0 ? "0" : "0.0", CultureInfo.InvariantCulture)} {units[unit]}";
    }

    private double? ReadUptimeSeconds()
    {
        try
        {
            var value = File.ReadAllText(Path.Combine(HostProcDirectory, "uptime"))
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)
                ? seconds
                : null;
        }
        catch
        {
            return null;
        }
    }

    private async Task<IReadOnlyList<ContainerSnapshot>> ReadContainersAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _dockerClient.GetAsync("v1.41/containers/json", cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var snapshots = new List<ContainerSnapshot>();
            foreach (var container in document.RootElement.EnumerateArray())
            {
                var id = ReadString(container, "Id");
                if (string.IsNullOrWhiteSpace(id)) continue;

                try
                {
                    snapshots.Add(await ReadContainerAsync(id, container, cancellationToken));
                }
                catch
                {
                    // A container can disappear between listing and sampling.
                    // Keep the remaining snapshot usable rather than failing it.
                }
            }

            return snapshots;
        }
        catch
        {
            return Array.Empty<ContainerSnapshot>();
        }
    }

    private async Task<ContainerSnapshot> ReadContainerAsync(
        string id,
        JsonElement container,
        CancellationToken cancellationToken)
    {
        using var response = await _dockerClient.GetAsync(
            $"v1.41/containers/{id}/stats?stream=false",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var stats = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var root = stats.RootElement;
        var cpuTotal = ReadLong(root, "cpu_stats", "cpu_usage", "total_usage");
        var previousCpuTotal = ReadLong(root, "precpu_stats", "cpu_usage", "total_usage");
        var systemTotal = ReadLong(root, "cpu_stats", "system_cpu_usage");
        var previousSystemTotal = ReadLong(root, "precpu_stats", "system_cpu_usage");
        var onlineCpus = ReadLong(root, "cpu_stats", "online_cpus");
        if (onlineCpus <= 0) onlineCpus = ReadArrayLength(root, "cpu_stats", "cpu_usage", "percpu_usage");

        var cpu = "0%";
        var cpuDelta = cpuTotal - previousCpuTotal;
        var systemDelta = systemTotal - previousSystemTotal;
        if (cpuDelta > 0 && systemDelta > 0 && onlineCpus > 0)
        {
            cpu = $"{((double)cpuDelta / systemDelta * onlineCpus * 100).ToString("0.##", CultureInfo.InvariantCulture)}%";
        }

        var usage = ReadLong(root, "memory_stats", "usage");
        var limit = ReadLong(root, "memory_stats", "limit");
        var memoryUsage = limit > 0 ? $"{FormatBytes(usage)} / {FormatBytes(limit)}" : FormatBytes(usage);
        var memoryPercent = limit > 0
            ? $"{((double)usage / limit * 100).ToString("0.##", CultureInfo.InvariantCulture)}%"
            : "—";

        var names = container.TryGetProperty("Names", out var namesNode) && namesNode.ValueKind == JsonValueKind.Array
            ? namesNode.EnumerateArray().Select(item => item.GetString()).FirstOrDefault()
            : null;

        return new ContainerSnapshot(
            (names ?? id[..Math.Min(id.Length, 12)]).TrimStart('/'),
            cpu,
            memoryUsage,
            memoryPercent);
    }

    private static string? ReadString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static long ReadLong(JsonElement element, params string[] path)
    {
        var current = element;
        foreach (var segment in path)
        {
            if (!current.TryGetProperty(segment, out current)) return 0;
        }

        return current.ValueKind == JsonValueKind.Number && current.TryGetInt64(out var value) ? value : 0;
    }

    private static long ReadArrayLength(JsonElement element, params string[] path)
    {
        var current = element;
        foreach (var segment in path)
        {
            if (!current.TryGetProperty(segment, out current)) return 0;
        }

        return current.ValueKind == JsonValueKind.Array ? current.GetArrayLength() : 0;
    }

    public void Dispose() => _dockerClient.Dispose();
}

public sealed record HostSnapshot(
    DateTimeOffset CollectedAtUtc,
    int? CpuCores,
    double? Load1,
    int? MemTotalMb,
    int? MemUsedMb,
    string? DiskUsed,
    string? DiskTotal,
    string? DiskPercent,
    double? UptimeSeconds,
    IReadOnlyList<ContainerSnapshot> Containers);

public sealed record ContainerSnapshot(
    string Name,
    string Cpu,
    string MemUsage,
    string MemPercent);
