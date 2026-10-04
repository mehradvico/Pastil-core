using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Application.Services.ServerMonitoringAlerts
{
    /// <summary>
    /// ماشین حالت هشدارهای سرور. خالص و بدون I/O است: هر بار با نتیجه‌ی یک بررسی (نمونه یا «عامل در دسترس نیست»)
    /// صدا زده می‌شود و فقط پیام‌های لازم (شروع مشکل / یادآوری / برطرف شدن) را برمی‌گرداند.
    /// </summary>
    public sealed class ServerAlertEvaluator
    {
        private sealed class State
        {
            public int Breaches;
            public int Clears;
            public bool Active;
            public DateTime LastSentUtc;
            public string Title = string.Empty;
        }

        private readonly ServerAlertOptions _options;
        private readonly Dictionary<string, State> _states = new();
        private readonly Dictionary<string, DateTime> _knownContainers = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _ignored;

        public ServerAlertEvaluator(ServerAlertOptions options)
        {
            _options = options;
            _ignored = new HashSet<string>(options.IgnoredContainers ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<ServerAlertMessage> EvaluateUnavailable(DateTime nowUtc)
        {
            var messages = new List<ServerAlertMessage>();
            Step(messages, nowUtc, "agent-down", true, _options.AgentDownChecks,
                Resource.Notification.MonitorAlertAgentDownName,
                Resource.Notification.MonitorAlertAgentDownBody);
            // وقتی عامل در دسترس نیست درباره‌ی بقیه چیزی نمی‌دانیم؛ حالتشان را دست نمی‌زنیم.
            return messages;
        }

        public IReadOnlyList<ServerAlertMessage> Evaluate(ServerSample sample, DateTime nowUtc)
        {
            var messages = new List<ServerAlertMessage>();
            Step(messages, nowUtc, "agent-down", false, _options.AgentDownChecks, string.Empty, string.Empty);

            var memPercent = sample.MemTotalMb is > 0 && sample.MemUsedMb.HasValue
                ? (double)sample.MemUsedMb.Value / sample.MemTotalMb.Value * 100
                : (double?)null;
            Step(messages, nowUtc, "memory", memPercent >= _options.MemoryPercent, _options.BreachChecks,
                Resource.Notification.MonitorAlertHighMemoryName,
                memPercent.HasValue
                    ? string.Format(CultureInfo.InvariantCulture, Resource.Notification.MonitorAlertHighMemoryBody,
                        Round(memPercent.Value), sample.MemUsedMb, sample.MemTotalMb)
                    : string.Empty);

            Step(messages, nowUtc, "disk", sample.DiskPercent >= _options.DiskPercent, _options.BreachChecks,
                Resource.Notification.MonitorAlertHighDiskName,
                sample.DiskPercent.HasValue
                    ? string.Format(CultureInfo.InvariantCulture, Resource.Notification.MonitorAlertHighDiskBody,
                        Round(sample.DiskPercent.Value), sample.DiskUsed, sample.DiskTotal)
                    : string.Empty);

            var loadPercent = sample.Load1.HasValue && sample.CpuCores is > 0
                ? sample.Load1.Value / sample.CpuCores.Value * 100
                : (double?)null;
            Step(messages, nowUtc, "load", loadPercent >= _options.LoadPercent, _options.BreachChecks,
                Resource.Notification.MonitorAlertHighLoadName,
                loadPercent.HasValue
                    ? string.Format(CultureInfo.InvariantCulture, Resource.Notification.MonitorAlertHighLoadBody,
                        Round(sample.Load1!.Value), sample.CpuCores, Round(loadPercent.Value))
                    : string.Empty);

            EvaluateContainers(messages, sample, nowUtc);
            return messages;
        }

        private void EvaluateContainers(List<ServerAlertMessage> messages, ServerSample sample, DateTime nowUtc)
        {
            var present = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var container in sample.Containers)
            {
                if (_ignored.Contains(container.Name)) continue;
                present.Add(container.Name);
                _knownContainers[container.Name] = nowUtc;

                Step(messages, nowUtc, "container-mem:" + container.Name,
                    container.MemoryPercent >= _options.ContainerMemoryPercent, _options.BreachChecks,
                    string.Format(CultureInfo.InvariantCulture, Resource.Notification.MonitorAlertContainerMemoryName, container.Name),
                    string.Format(CultureInfo.InvariantCulture, Resource.Notification.MonitorAlertContainerMemoryBody,
                        container.Name, Round(container.MemoryPercent ?? 0), container.MemoryUsage));
            }

            var forgetAfter = TimeSpan.FromHours(Math.Max(1, _options.ForgetContainerAfterHours));
            foreach (var (name, lastSeen) in _knownContainers.ToList())
            {
                var missing = !present.Contains(name);
                if (missing && nowUtc - lastSeen > forgetAfter)
                {
                    _knownContainers.Remove(name);
                    _states.Remove("container-down:" + name);
                    _states.Remove("container-mem:" + name);
                    continue;
                }

                Step(messages, nowUtc, "container-down:" + name, missing, _options.ContainerDownChecks,
                    string.Format(CultureInfo.InvariantCulture, Resource.Notification.MonitorAlertContainerDownName, name),
                    string.Format(CultureInfo.InvariantCulture, Resource.Notification.MonitorAlertContainerDownBody, name));

                // کانتینر غایب دیگر حافظه ندارد؛ هشدار حافظه‌اش بی‌صدا بسته می‌شود.
                if (missing) _states.Remove("container-mem:" + name);
            }
        }

        private void Step(List<ServerAlertMessage> messages, DateTime nowUtc, string key, bool breached,
            int requiredBreaches, string title, string body)
        {
            if (!_states.TryGetValue(key, out var state))
            {
                if (!breached) return;
                state = _states[key] = new State();
            }

            if (breached)
            {
                state.Clears = 0;
                state.Breaches++;
                if (state.Breaches < Math.Max(1, requiredBreaches)) return;

                var repeat = _options.RepeatMinutes > 0
                    && nowUtc - state.LastSentUtc >= TimeSpan.FromMinutes(_options.RepeatMinutes);
                if (!state.Active || repeat)
                {
                    state.Active = true;
                    state.LastSentUtc = nowUtc;
                    state.Title = title;
                    messages.Add(new ServerAlertMessage(key, ServerAlertKind.Firing, title, body));
                }
                return;
            }

            state.Breaches = 0;
            if (!state.Active)
            {
                _states.Remove(key);
                return;
            }

            state.Clears++;
            if (state.Clears < Math.Max(1, _options.ClearChecks)) return;

            var resolvedName = string.IsNullOrEmpty(title) ? state.Title : title;
            messages.Add(new ServerAlertMessage(key, ServerAlertKind.Resolved,
                string.Format(CultureInfo.InvariantCulture, Resource.Notification.MonitorAlertResolvedName, resolvedName),
                Resource.Notification.MonitorAlertResolvedBody));
            _states.Remove(key);
        }

        private static string Round(double value) => value.ToString("0.#", CultureInfo.InvariantCulture);
    }
}
