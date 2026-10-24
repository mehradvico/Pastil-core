using Application.Common.Dto.Result;
using Entities.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.UserActivitySrv
{
    public interface IUserActivityService
    {
        Task<BaseResultDto> TrackAsync(long userId, TrackActivityDto dto);
        Task<UserAnalyticsDto> AnalyzeAsync(long userId, int days, DateTime? from = null, DateTime? to = null);
    }

    public class UserActivityService : IUserActivityService
    {
        private const int MaxBatch = 100;
        private const int MaxEventsPerAnalysis = 50000;
        private static readonly string[] AllowedTypes = { "screen", "click", "action", "flow_step", "flow_done", "session_start", "heartbeat" };
        // ایران از ۱۴۰۱ ساعت تابستانی ندارد؛ آفست ثابت ۳:۳۰.
        private static readonly TimeSpan Tehran = TimeSpan.FromMinutes(210);

        private readonly IDataBaseContext _context;

        public UserActivityService(IDataBaseContext context)
        {
            _context = context;
        }

        public async Task<BaseResultDto> TrackAsync(long userId, TrackActivityDto dto)
        {
            if (userId <= 0 || dto?.Events == null || dto.Events.Count == 0)
                return new BaseResultDto(false, Resource.Notification.InvalidData);

            var now = DateTime.UtcNow;
            var platform = Clip(dto.Platform, 20);
            foreach (var e in dto.Events.Take(MaxBatch))
            {
                var type = (e.Type ?? string.Empty).Trim().ToLowerInvariant();
                if (!AllowedTypes.Contains(type)) continue;

                var at = e.At.HasValue && e.At.Value <= now.AddMinutes(5) && e.At.Value >= now.AddDays(-2)
                    ? e.At.Value
                    : now;

                _context.UserActivityEvents.Add(new UserActivityEvent
                {
                    UserId = userId,
                    SessionId = e.SessionId,
                    Type = type,
                    Screen = Clip(e.Screen, 300),
                    Target = Clip(e.Target, 200),
                    Platform = platform,
                    DurationSeconds = e.DurationSeconds is > 0 and <= 86400 ? e.DurationSeconds : null,
                    CreateDate = at
                });
            }

            await _context.SaveChangesAsync();
            return new BaseResultDto(true);
        }

        public async Task<UserAnalyticsDto> AnalyzeAsync(long userId, int days, DateTime? from = null, DateTime? to = null)
        {
            var nowUtc = DateTime.UtcNow;
            var today = (nowUtc + Tehran).Date;
            DateTime startLocal, endLocal; // endLocal انحصاری
            if (from.HasValue)
            {
                startLocal = from.Value.Date;
                endLocal = (to ?? from.Value).Date.AddDays(1);
                if (endLocal <= startLocal) endLocal = startLocal.AddDays(1);
                if ((endLocal - startLocal).TotalDays > 366) endLocal = startLocal.AddDays(366);
            }
            else
            {
                days = Math.Clamp(days, 1, 365);
                startLocal = today.AddDays(-(days - 1));
                endLocal = today.AddDays(1);
            }
            days = (int)(endLocal - startLocal).TotalDays;
            var fromUtc = startLocal - Tehran;
            var toUtc = endLocal - Tehran;

            var events = await _context.UserActivityEvents.AsNoTracking()
                .Where(x => x.UserId == userId && x.CreateDate >= fromUtc && x.CreateDate < toUtc)
                .OrderByDescending(x => x.CreateDate)
                .Take(MaxEventsPerAnalysis)
                .Select(x => new { x.Type, x.Screen, x.Target, x.Platform, x.SessionId, x.CreateDate })
                .ToListAsync();

            var tokens = await _context.UserTokens.AsNoTracking()
                .Where(x => x.UserId == userId && x.CreateDate >= fromUtc && x.CreateDate < toUtc && x.RotatedFromTokenId == null)
                .Select(x => new { x.CreateDate, x.DeviceName })
                .ToListAsync();

            var registerDate = await _context.Users.AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => (DateTime?)x.CreateDate)
                .FirstOrDefaultAsync();

            // فعالیت‌های همه‌ی بخش‌ها که بدون ردیابی کلاینت هم در دیتابیس ثبت شده‌اند.
            var acts = await CollectDomainActivityAsync(userId, fromUtc, toUtc);
            // اقدام‌هایی که اپ صریحاً گزارش کرده (مثل «ویرایش پروفایل») که ردّ دیتابیسی ندارند.
            var clientActions = await _context.UserActivityEvents.AsNoTracking()
                .Where(x => x.UserId == userId && x.Type == "action" && x.CreateDate >= fromUtc && x.CreateDate < toUtc)
                .OrderByDescending(x => x.CreateDate).Take(1000)
                .Select(x => new { x.Screen, x.Target, x.CreateDate }).ToListAsync();
            acts.AddRange(clientActions.Select(x => (x.CreateDate, string.IsNullOrWhiteSpace(x.Screen) ? "اپ" : x.Screen, x.Target ?? "اقدام")));

            var businessStamps = acts.Select(x => x.At).ToList();

            var result = new UserAnalyticsDto
            {
                Days = days,
                From = startLocal.ToString("yyyy-MM-dd"),
                To = endLocal.AddDays(-1).ToString("yyyy-MM-dd"),
                RegisterDate = registerDate,
                HasTrackingData = events.Count > 0,
                Heatmap = Enumerable.Range(0, 7).Select(_ => new int[24]).ToArray()
            };

            var perDay = new Dictionary<string, DayActivityDto>();
            for (var i = 0; i < days; i++)
            {
                var key = startLocal.AddDays(i).ToString("yyyy-MM-dd");
                perDay[key] = new DayActivityDto { Date = key };
            }

            void Stamp(DateTime utc, bool login, bool action)
            {
                var local = utc + Tehran;
                if (!perDay.TryGetValue(local.Date.ToString("yyyy-MM-dd"), out var day)) return;
                day.Events++;
                if (login) day.Logins++;
                if (action) day.Actions++;
                // شنبه=0 ... جمعه=6
                var dow = ((int)local.DayOfWeek + 1) % 7;
                result.Heatmap[dow][local.Hour]++;
            }

            foreach (var t in tokens) Stamp(t.CreateDate, true, false);
            foreach (var b in businessStamps) Stamp(b, false, true);

            var sessions = new Dictionary<Guid, (DateTime min, DateTime max)>();
            foreach (var e in events)
            {
                var isClick = e.Type == "click";
                Stamp(e.CreateDate, false, isClick);

                if (e.Type == "screen") result.TotalScreenViews++;
                if (isClick) result.TotalClicks++;

                if (e.SessionId.HasValue)
                {
                    var sid = e.SessionId.Value;
                    sessions[sid] = sessions.TryGetValue(sid, out var s)
                        ? (e.CreateDate < s.min ? e.CreateDate : s.min, e.CreateDate > s.max ? e.CreateDate : s.max)
                        : (e.CreateDate, e.CreateDate);
                }
            }

            // دقیقه‌های حضور = طول نشست‌ها (سقف ۲ ساعت برای هر نشست تا نشست‌های رهاشده عدد را باد نکنند).
            foreach (var s in sessions.Values)
            {
                var mins = (int)Math.Min(120, Math.Max(1, (s.max - s.min).TotalMinutes));
                result.TotalMinutes += mins;
                var key = (s.min + Tehran).Date.ToString("yyyy-MM-dd");
                if (perDay.TryGetValue(key, out var day)) day.Minutes += mins;
            }
            result.TotalSessions = sessions.Count;
            result.AvgSessionMinutes = sessions.Count == 0 ? 0 : result.TotalMinutes / sessions.Count;
            result.TotalLogins = tokens.Count;

            result.Daily = perDay.Values.OrderBy(x => x.Date).ToList();
            foreach (var d in result.Daily) d.Active = d.Events > 0;
            result.ActiveDays = result.Daily.Count(x => x.Active);
            result.IdleDays = days - result.ActiveDays;

            int run = 0, best = 0;
            foreach (var d in result.Daily)
            {
                run = d.Active ? 0 : run + 1;
                best = Math.Max(best, run);
            }
            result.LongestIdleStreak = best;

            var streak = 0;
            for (var i = result.Daily.Count - 1; i >= 0 && result.Daily[i].Active; i--) streak++;
            result.CurrentActiveStreak = streak;

            result.Weekdays = Enumerable.Range(0, 7).Select(i => result.Heatmap[i].Sum()).ToList();
            result.Hourly = Enumerable.Range(0, 24).Select(h => result.Heatmap.Sum(row => row[h])).ToList();

            var lastStamps = new List<DateTime>();
            if (events.Count > 0) lastStamps.Add(events[0].CreateDate);
            if (tokens.Count > 0) lastStamps.Add(tokens.Max(x => x.CreateDate));
            if (businessStamps.Count > 0) lastStamps.Add(businessStamps.Max());
            if (lastStamps.Count > 0) result.LastActivityUtc = lastStamps.Max();

            result.TopScreens = Top(events.Where(x => x.Type == "screen").Select(x => x.Screen), 10);
            result.TopClicks = Top(events.Where(x => x.Type == "click").Select(x => x.Target), 10);
            result.Platforms = Top(events.Select(x => PlatformLabel(x.Platform)), 6);
            result.Devices = Top(tokens.Select(x => DeviceLabel(x.DeviceName)), 8);

            result.Modules = acts.GroupBy(x => x.Module)
                .Select(g => new ModuleActivityDto
                {
                    Module = g.Key,
                    Count = g.Count(),
                    LastAtUtc = g.Max(x => x.At),
                    Actions = g.GroupBy(x => x.Action)
                        .Select(a => new CountItemDto { Key = a.Key, Count = a.Count() })
                        .OrderByDescending(a => a.Count).ToList()
                })
                .OrderByDescending(m => m.Count).ToList();
            result.Timeline = acts.OrderByDescending(x => x.At).Take(300)
                .Select(x => new TimelineItemDto { AtUtc = x.At, Module = x.Module, Action = x.Action }).ToList();

            result.Recent = events.Take(40).Select(x => new RecentActivityDto
            {
                AtUtc = x.CreateDate,
                Type = x.Type,
                Screen = x.Screen,
                Target = x.Target
            }).ToList();

            var flows = BuildAppFlows(events.Where(x => x.Type is "flow_step" or "flow_done")
                .Select(x => (x.Type, x.Screen, x.Target, x.SessionId, x.CreateDate)).ToList(), nowUtc);
            flows.AddRange(await CollectDbFlowsAsync(userId, fromUtc, toUtc));
            result.Flows = flows.OrderByDescending(f => f.StartedUtc).Take(200).ToList();
            result.FlowSummaries = flows.GroupBy(f => f.Flow).Select(g => new FlowSummaryDto
            {
                Flow = g.Key,
                Started = g.Count(),
                Completed = g.Count(f => f.Status == "completed"),
                Abandoned = g.Count(f => f.Status == "abandoned"),
                InProgress = g.Count(f => f.Status == "in_progress"),
                DropOffs = g.Where(f => f.Status == "abandoned" && !string.IsNullOrEmpty(f.LastStep))
                    .GroupBy(f => f.LastStep).Select(d => new CountItemDto { Key = d.Key, Count = d.Count() })
                    .OrderByDescending(d => d.Count).Take(5).ToList()
            }).OrderByDescending(x => x.Started).ToList();

            result.Journeys = BuildJourneys(events.Select(x => (x.Type, x.Screen, x.Target, x.SessionId, x.CreateDate)).ToList());

            return result;
        }

        private static List<FlowInstanceDto> BuildAppFlows(List<(string Type, string Screen, string Target, Guid? SessionId, DateTime At)> evs, DateTime nowUtc)
        {
            var list = new List<FlowInstanceDto>();
            foreach (var g in evs.Where(e => !string.IsNullOrEmpty(e.Screen))
                         .GroupBy(e => (e.Screen, Sid: e.SessionId ?? Guid.Empty)))
            {
                FlowInstanceDto cur = null;
                var lastOrder = 0;

                void Close(string status)
                {
                    if (cur == null) return;
                    cur.Status = status;
                    list.Add(cur);
                    cur = null;
                }

                string Unfinished() => (nowUtc - cur.Steps[^1].AtUtc).TotalMinutes > 30 ? "abandoned" : "in_progress";

                foreach (var e in g.OrderBy(x => x.At))
                {
                    if (e.Type == "flow_done")
                    {
                        cur ??= new FlowInstanceDto { Flow = g.Key.Screen, Source = "app", StartedUtc = e.At };
                        cur.Steps.Add(new FlowStepDto { Order = lastOrder + 1, Label = string.IsNullOrWhiteSpace(e.Target) ? "تکمیل" : e.Target, AtUtc = e.At });
                        cur.LastStep = cur.Steps[^1].Label;
                        Close("completed");
                        lastOrder = 0;
                        continue;
                    }

                    var parts = (e.Target ?? string.Empty).Split('|', 2);
                    var order = parts.Length == 2 && int.TryParse(parts[0], out var o) ? o : lastOrder + 1;
                    var label = parts.Length == 2 ? parts[1] : parts[0];
                    if (cur != null && order <= lastOrder)
                    {
                        cur.LastStep = cur.Steps[^1].Label;
                        Close(Unfinished());
                    }
                    cur ??= new FlowInstanceDto { Flow = g.Key.Screen, Source = "app", StartedUtc = e.At };
                    cur.Steps.Add(new FlowStepDto { Order = order, Label = label, AtUtc = e.At });
                    lastOrder = order;
                }

                if (cur != null)
                {
                    cur.LastStep = cur.Steps[^1].Label;
                    Close(Unfinished());
                }
            }
            return list;
        }

        // رهاشدن واقعی از رکوردهای دیتابیس: پرداخت ناموفق، سفارش پرداخت‌نشده، رزرو بدون پرداخت.
        private async Task<List<FlowInstanceDto>> CollectDbFlowsAsync(long userId, DateTime from, DateTime to)
        {
            var list = new List<FlowInstanceDto>();
            var db = _context;

            void Add(string flow, DateTime at, bool done, string firstStep, string doneStep, string dropLabel)
            {
                var f = new FlowInstanceDto
                {
                    Flow = flow,
                    Source = "db",
                    StartedUtc = at,
                    Status = done ? "completed" : "abandoned",
                    LastStep = done ? doneStep : dropLabel
                };
                f.Steps.Add(new FlowStepDto { Order = 1, Label = firstStep, AtUtc = at });
                if (done) f.Steps.Add(new FlowStepDto { Order = 2, Label = doneStep, AtUtc = at });
                list.Add(f);
            }

            var pays = await db.Payments.AsNoTracking()
                .Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to)
                .OrderByDescending(x => x.CreateDate).Take(200).Select(x => new { x.CreateDate, x.IsSuccess }).ToListAsync();
            foreach (var p in pays) Add("پرداخت", p.CreateDate, p.IsSuccess == true, "شروع پرداخت", "پرداخت موفق", "رفت به درگاه ولی پرداخت کامل نشد");

            var orders = await db.ProductOrders.AsNoTracking()
                .Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to)
                .OrderByDescending(x => x.CreateDate).Take(200).Select(x => new { x.CreateDate, x.IsPaid }).ToListAsync();
            foreach (var o in orders) Add("فروشگاه: سفارش", o.CreateDate, o.IsPaid, "ثبت سفارش", "پرداخت سفارش", "سفارش ثبت شد ولی پرداخت نشد");

            var clinic = await db.ClinicReserves.AsNoTracking()
                .Where(x => x.UserPet.UserId == userId && x.CreateDate >= from && x.CreateDate < to)
                .OrderByDescending(x => x.CreateDate).Take(100).Select(x => new { x.CreateDate, x.Status }).ToListAsync();
            foreach (var r in clinic) Add("کلینیک: رزرو", r.CreateDate, r.Status >= 1, "ثبت رزرو", "پرداخت رزرو", "رزرو ثبت شد ولی پرداخت نشد");

            var groomer = await db.GroomerReserves.AsNoTracking()
                .Where(x => x.UserPet.UserId == userId && x.CreateDate >= from && x.CreateDate < to)
                .OrderByDescending(x => x.CreateDate).Take(100).Select(x => new { x.CreateDate, x.Status }).ToListAsync();
            foreach (var r in groomer) Add("آرایشگاه: رزرو", r.CreateDate, r.Status >= 1, "ثبت رزرو", "پرداخت رزرو", "رزرو ثبت شد ولی پرداخت نشد");

            var coach = await db.CoachReserves.AsNoTracking()
                .Where(x => x.UserPet.UserId == userId && x.CreateDate >= from && x.CreateDate < to)
                .OrderByDescending(x => x.CreateDate).Take(100).Select(x => new { x.CreateDate, x.Status }).ToListAsync();
            foreach (var r in coach) Add("مربی: رزرو", r.CreateDate, r.Status >= 1, "ثبت رزرو", "پرداخت رزرو", "رزرو ثبت شد ولی پرداخت نشد");

            return list;
        }

        // مسیر حرکت: رویدادها به ترتیب زمان؛ با ۳۰ دقیقه سکوت یا تغییر نشست، سفر جدید شروع می‌شود.
        private static List<JourneyDto> BuildJourneys(List<(string Type, string Screen, string Target, Guid? SessionId, DateTime At)> evs)
        {
            var journeys = new List<JourneyDto>();
            JourneyDto cur = null;
            Guid? curSid = null;
            foreach (var e in evs.Where(x => x.Type is "screen" or "click" or "action" or "flow_step" or "flow_done").OrderBy(x => x.At))
            {
                if (cur == null || (e.At - cur.EndUtc).TotalMinutes > 30 || (e.SessionId.HasValue && curSid.HasValue && e.SessionId != curSid))
                {
                    cur = new JourneyDto { StartUtc = e.At, EndUtc = e.At };
                    journeys.Add(cur);
                }
                curSid = e.SessionId ?? curSid;
                cur.EndUtc = e.At;
                if (cur.Steps.Count >= 80) continue;

                var (type, label) = e.Type switch
                {
                    "screen" => ("screen", e.Screen),
                    "click" => ("click", e.Target),
                    "action" => ("action", e.Screen + " · " + e.Target),
                    "flow_done" => ("flow", e.Screen + ": تکمیل"),
                    _ => ("flow", e.Screen + ": " + (e.Target ?? string.Empty).Split('|').Last())
                };
                if (!string.IsNullOrWhiteSpace(label))
                    cur.Steps.Add(new JourneyStepDto { AtUtc = e.At, Type = type, Label = label });
            }
            return journeys.OrderByDescending(j => j.StartUtc).Take(12).ToList();
        }

        private const int PerSourceCap = 500;

        // هر منبع یک کوئری سبک روی «تاریخ ایجاد» رکوردهای کاربر است؛ ترتیب اجرا ترتیبی است (DbContext thread-safe نیست).
        private async Task<List<(DateTime At, string Module, string Action)>> CollectDomainActivityAsync(long userId, DateTime from, DateTime to)
        {
            var acts = new List<(DateTime At, string Module, string Action)>();
            var db = _context;

            async Task Add(string module, string action, IQueryable<DateTime> dates)
            {
                var list = await dates.OrderByDescending(d => d).Take(PerSourceCap).ToListAsync();
                foreach (var d in list) acts.Add((d, module, action));
            }

            var petIds = db.UserPets.AsNoTracking().Where(p => p.UserId == userId).Select(p => p.Id);

            // فروشگاه و مالی
            await Add("فروشگاه", "ثبت سفارش", db.ProductOrders.AsNoTracking().Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("فروشگاه", "ایجاد/به‌روزرسانی سبد خرید", db.Carts.AsNoTracking().Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("فروشگاه", "گزارش مشکل محصول", db.ProductReports.AsNoTracking().Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("فروشگاه", "درخواست اطلاع‌رسانی موجودی", db.ProductStockAlerts.AsNoTracking().Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("مالی", "پرداخت موفق", db.Payments.AsNoTracking().Where(x => x.UserId == userId && x.IsSuccess == true && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("مالی", "پرداخت ناموفق/رهاشده", db.Payments.AsNoTracking().Where(x => x.UserId == userId && x.IsSuccess != true && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("مالی", "تراکنش کیف پول", db.Wallets.AsNoTracking().Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("مالی", "ثبت کارت بانکی", db.UserBankCards.AsNoTracking().Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));

            // خدمات
            await Add("پت‌رسان", "درخواست سفر", db.Trips.AsNoTracking().Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("پت‌رسان", "ارسال محموله", db.Cargoes.AsNoTracking().Where(x => petIds.Contains(x.UserPetId) && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("پت‌رسان", "سرویس هفتگی", db.PetResanServices.AsNoTracking().Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("کلینیک", "رزرو ویزیت", db.ClinicReserves.AsNoTracking().Where(x => petIds.Contains(x.UserPetId) && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("آرایشگاه", "رزرو خدمت", db.GroomerReserves.AsNoTracking().Where(x => petIds.Contains(x.UserPetId) && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("مربی", "رزرو مربی", db.CoachReserves.AsNoTracking().Where(x => petIds.Contains(x.UserPetId) && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("پانسیون", "رزرو پانسیون", db.PansionReserves.AsNoTracking().Where(x => petIds.Contains(x.UserPetId) && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("مدرسه پت", "رزرو مدرسه", db.SchoolReserves.AsNoTracking().Where(x => petIds.Contains(x.UserPetId) && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("همیار", "رزرو خدمت همیار", db.CompanionReserves.AsNoTracking().Where(x => x.BookerId == userId && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("مشاوره", "خرید مشاوره", db.ConsultationPurchases.AsNoTracking().Where(x => x.UserId == userId && x.PaidDate != null && x.PaidDate >= from && x.PaidDate < to).Select(x => x.PaidDate.Value));
            await Add("میکروچیپ", "درخواست میکروچیپ", db.PetMicrochipRequests.AsNoTracking().Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));

            // پاستیل AI
            await Add("پاستیل AI", "شروع گفتگو", db.PastilAiConversations.AsNoTracking().Where(x => x.UserId == userId && x.CreateDateUtc >= from && x.CreateDateUtc < to).Select(x => x.CreateDateUtc));
            await Add("پاستیل AI", "خرید/فعال‌سازی اشتراک", db.PastilAiSubscriptions.AsNoTracking().Where(x => x.UserId == userId && x.CreateDateUtc >= from && x.CreateDateUtc < to).Select(x => x.CreateDateUtc));
            await Add("پاستیل AI", "ارسال پیام به AI", db.PastilAiMessages.AsNoTracking().Where(x => x.Conversation.UserId == userId && x.CreateDateUtc >= from && x.CreateDateUtc < to).Select(x => x.CreateDateUtc));

            // پاستیل مچ
            var profileIds = db.PastilMatchProfiles.AsNoTracking().Where(x => petIds.Contains(x.UserPetId)).Select(x => x.Id);
            await Add("پاستیل مچ", "ساخت/ثبت پروفایل مچ", db.PastilMatchProfiles.AsNoTracking().Where(x => petIds.Contains(x.UserPetId) && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("پاستیل مچ", "ارسال درخواست مچ", db.PastilMatchRequests.AsNoTracking().Where(x => profileIds.Contains(x.SenderProfileId) && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("پاستیل مچ", "ارسال پیام در چت مچ", db.PastilMatchMessages.AsNoTracking().Where(x => x.SenderProfileId != null && profileIds.Contains(x.SenderProfileId.Value) && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));

            // پت، خاطره، محتوا و پشتیبانی
            await Add("خاطرات", "ثبت خاطره", db.UserMemories.AsNoTracking().Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("محتوا", "نوشتن دیدگاه", db.Comments.AsNoTracking().Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("محتوا", "ثبت پست", db.Posts.AsNoTracking().Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("محتوا", "پرسش در بخش پرسش و پاسخ", db.DiscussionQuestions.AsNoTracking().Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("محتوا", "پاسخ در بخش پرسش و پاسخ", db.DiscussionAnswers.AsNoTracking().Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("پشتیبانی", "ثبت تیکت", db.Tickets.AsNoTracking().Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("پشتیبانی", "تماس با ما", db.ContactUses.AsNoTracking().Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));
            await Add("باشگاه", "دریافت/مصرف امتیاز", db.ClubPointTransactions.AsNoTracking().Where(x => x.UserId == userId && x.CreateDate >= from && x.CreateDate < to).Select(x => x.CreateDate));

            return acts;
        }

        // vico (قبلاً zand) = برچسب توکن‌های وب‌اپ/اپ موبایل؛ panel:<hash> = ورود به پنل ادمین (هش شناسه‌ی دستگاه).
        private static string DeviceLabel(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            if (name == "zand" || name == "vico") return "ورود از اپ (وب‌اپ/موبایل)";
            if (name.StartsWith("panel:", StringComparison.Ordinal)) return "ورود به پنل مدیریت";
            return name;
        }

        private static string PlatformLabel(string p) => p switch
        {
            "pwa" => "وب‌اپ (PWA)",
            "web" => "وب",
            "android" => "اندروید",
            "ios" => "iOS",
            _ => p
        };

        private static List<CountItemDto> Top(IEnumerable<string> values, int take) =>
            values.Where(v => !string.IsNullOrWhiteSpace(v))
                .GroupBy(v => v)
                .Select(g => new CountItemDto { Key = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(take)
                .ToList();

        private static string Clip(string s, int max)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();
            return s.Length <= max ? s : s[..max];
        }
    }
}
