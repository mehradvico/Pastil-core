# هشدار و پایش امنیتی پاستیل

> تاریخ: ۲۹ شهریور ۱۴۰۵

دو لایه‌ی مکمل وجود دارد. لایه‌ی اول بدون هیچ زیرساخت اضافه کار می‌کند؛ لایه‌ی دوم برای داشبورد و جست‌وجوی تاریخی است.

## لایه ۱ — هشدار لحظه‌ای داخل خود Api (بدون Grafana)
`SecurityAlertService` هر رویداد `Security.Audit` را با قواعد زیر می‌سنجد. هر هشدار همیشه در لاگ با سطح **Critical** و category `Security.Alert` نوشته می‌شود و اگر یکی از کانال‌های زیر تنظیم باشد فوراً ارسال هم می‌شود.

| قاعده | آستانه | dedupe |
| --- | --- | --- |
| سرقت/استفاده‌ی مجدد refresh token (`RefreshTokenTheft`) | هر رخداد | ۱۵ دقیقه برای هر کاربر |
| تلاش ناموفق ورود/رمز/OTP/تغییر موبایل از یک IP | ≥ ۱۰ در ۵ دقیقه | ۳۰ دقیقه برای هر IP |
| قفل موقت رمز یک حساب (`password_throttled`) | هر رخداد | ۳۰ دقیقه برای هر کاربر |
| پاسخ‌های 403/429 از یک IP | ≥ ۱۰۰ در ۵ دقیقه | ۳۰ دقیقه برای هر IP |
| نوشتن در پنل ادمین در ساعت غیرکاری (۰۰:۰۰–۰۶:۰۰ تهران) | هر رخداد | ۶۰ دقیقه برای هر کاربر |
| ورود موفق به پنل بعد از ≥ ۳ تلاش ناموفق اخیر همان کاربر | هر رخداد | ۳۰ دقیقه برای هر کاربر |

### تنظیم کانال ارسال (متغیرهای `.env` سرور)
```text
# گزینه ۱: webhook عمومی (POST JSON با کلید text؛ سازگار با Slack/Mattermost یا هر relay شما)
PASTIL_SECURITY_ALERT_WEBHOOK_URL=https://...

# گزینه ۲: ربات تلگرام
PASTIL_SECURITY_ALERT_TELEGRAM_BOT_TOKEN=<توکن BotFather>
PASTIL_SECURITY_ALERT_TELEGRAM_CHAT_ID=<شناسه گروه/کاربر>
```
- فقط `https` برای webhook پذیرفته می‌شود. خطای ارسال جریان اصلی را نمی‌شکند و فقط نوع خطا لاگ می‌شود.
- بدون هیچ‌کدام، هشدارها فقط در لاگ (`docker logs`) با عبارت `SecurityAlert` دیده می‌شوند.
- پیام‌ها شامل شناسه‌ی کاربر و IP هستند، نه شماره موبایل/رمز/توکن.
- شمارنده‌ها داخل حافظه‌ی هر instance هستند؛ با چند instance هر کدام جدا می‌شمارد.

## لایه ۲ — لاگ‌ها در OTLP / Grafana Loki
اگر `Observability__OtlpEndpoint` تنظیم باشد، حالا **لاگ‌ها هم** export می‌شوند (قبلاً فقط trace و metric بود): category‌های `Security.Audit` و `Security.Alert` از سطح Information و بقیه از Warning به بالا.

### نمونه‌ی alert در Grafana (LogQL؛ نام label را با collector خودتان تطبیق دهید)
```logql
# ≥ ۱۰ ورود ناموفق در ۵ دقیقه (هر IP جدا)
sum by (ip) (count_over_time({service_name="pastil-api"} |= "SecurityEvent SignIn failure" | regexp `Ip=(?P<ip>\S+)` [5m])) >= 10

# هر سرقت refresh token
count_over_time({service_name="pastil-api"} |= "SecurityEvent RefreshTokenTheft" [5m]) > 0

# نوشتن ادمین خارج از ساعت کاری (بعد از تنظیم timezone در Grafana)
count_over_time({service_name="pastil-api"} |= "SecurityEvent AdminWrite" [15m]) > 0
```
همان قواعد لایه ۱ را می‌توانید اینجا هم بسازید؛ برای Alert اصلی پیشنهاد می‌شود لایه ۱ فعال بماند چون به زیرساخت وابسته نیست.

## چک بعد از deploy
1. یک ورود ناموفق بزنید و در لاگ Api دنبال `SecurityEvent SignIn failure` بگردید.
2. ۱۰ بار پشت‌سرهم رمز غلط برای همان IP بزنید؛ باید `SecurityAlert` و (در صورت تنظیم کانال) پیام دریافت کنید.
