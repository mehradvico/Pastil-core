# پایش API و Payment

هر دو سرویس، دو endpoint عمومی و بدون نیاز به توکن دارند:

| سرویس | liveness | readiness |
| --- | --- | --- |
| API | `/health/live` | `/health/ready` |
| Payment | `/health/live` | `/health/ready` |

`live` فقط زنده‌بودن پردازش را می‌سنجد. `ready` یک اتصال واقعی به SQL Server می‌زند؛ در نبود اتصال پاسخ `503` می‌دهد. پاسخ هر دو JSON است و نام check و وضعیت آن را دارد، اما جزئیات یا credential دیتابیس را فاش نمی‌کند.

## اتصال به APM

پروژه از OpenTelemetry با پروتکل OTLP استفاده می‌کند و traceهای درخواست ورودی، HTTP خروجی و SQL به‌علاوهٔ متریک‌های HTTP را تولید می‌کند. برای ارسال آن‌ها به یک Collector داخلی، این متغیر را برای **هر دو** سرویس تنظیم کنید:

```text
Observability__OtlpEndpoint=http://otel-collector:4317
```

یا از نام استاندارد OpenTelemetry استفاده کنید:

```text
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
```

مقدار پیش‌فرض نمونه‌برداری trace در production برابر `0.10` است. برای تست فشار staging آن را موقتاً کامل کنید:

```text
Observability__TraceSampleRatio=1.0
```

Collector باید در شبکهٔ داخلی باشد؛ URL یا header دارای credential را داخل `appsettings.json` commit نکنید. اگر endpoint تنظیم نشده یا معتبر نباشد، telemetry فقط export نمی‌شود و هیچ‌یک از flowهای کاربر، پرداخت یا سفر متوقف نخواهد شد.

## Probe و alert پیشنهادی

برای container/orchestrator، `live` را با interval ده ثانیه و timeout دو ثانیه، و `ready` را با timeout پنج ثانیه تنظیم کنید. بعد از سه پاسخ ناموفق متوالی از `ready` alert ایجاد کنید. در APM نیز برای نرخ `5xx`، latency صدک ۹۵، خطاهای dependency SQL و HTTP، و پاسخ‌های `429` OTP/price alert جدا داشته باشید.

نمونهٔ بررسی پس از deploy:

```powershell
Invoke-WebRequest https://api.pastil.pet/health/live
Invoke-WebRequest https://api.pastil.pet/health/ready
Invoke-WebRequest https://payment.pastil.pet/health/ready
```
