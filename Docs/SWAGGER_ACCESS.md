# محافظت از Swagger — API و File

## چرا Basic Auth، نه JWT ثابت، نه فقط IP

سه گزینه رو مقایسه کردم:

| گزینه | مشکل |
|---|---|
| **یک JWT طولانی مشترک توی .env** | یک secret مشترک بین چند نفره — اگه لو بره (چسبیده به یه URL، توی history مرورگر، Slack، هرجا) دسترسی نامحدود می‌ده و راهی برای لغو دسترسی فقط یک نفر بدون بی‌اثر کردن بقیه نیست. هیچ audit trail نداره (نمی‌فهمی کدوم نفر). چون Swagger UI با navigate ساده‌ی مرورگر باز می‌شه (نه یه API client)، هدر `Authorization: Bearer` طبیعتاً attach نمی‌شه — باید یه صفحه لاگین/کوکی سفارشی می‌ساختیم، پیچیدگی اضافه برای یه نیاز ساده. |
| **فقط IP Allowlist** | خب کار می‌کنه، ولی اگه فرانت‌کارها از خونه/موبایل/جای دیگه کار کنن، IP عوض می‌شه و می‌مونن بیرون — نیاز به آپدیت مداوم لیست داره. هویت هم مشخص نمی‌کنه (دو نفر پشت یه IP مشترک = نمی‌دونی کی بوده). |
| **HTTP Basic Auth با ۲ تا credential جدا (چیزی که پیاده شد)** | همون چیزیه که خیلی از شرکت‌ها برای محافظت از Swagger/Grafana/Kibana داخلی استفاده می‌کنن. مرورگر خودش popup لاگین می‌ده، هیچ کد فرانت لازم نیست. هر نفر credential خودشو داره → قابل لغو جدا جدا، توی لاگ سرور مشخصه کدوم username وارد شده. |

**نتیجه**: Basic Auth با ۲ تا credential مجزا پیاده شد، و یک لایه‌ی IP Allowlist هم *اختیاری* روش گذاشتم — اگه IP ثابت (دفتر/VPN) دارید، پرش کنید تا هر دو لایه با هم چک بشن؛ اگه ندارید، خالی بذارید و فقط Basic Auth کار می‌کنه.

## رفتار پیش‌فرض: بسته

اگه هیچ credential‌ای ست نشده باشه، `/swagger` روی هر دو سرویس (Api و File) **404** برمی‌گردونه — دقیقاً همون وضعیتی که الان دارید، بدون نیاز به هیچ تغییری. یعنی این تغییر خودش به‌تنهایی چیزی رو باز نمی‌کنه.

## چطور بازش کنید

**هیچ‌وقت مقدار واقعی رمز رو توی `appsettings.json` ننویسید** — این فایل commit می‌شه. مقدارها رو با Environment Variable روی خود سرور ست کنید (همون الگویی که برای JWT key و بقیه‌ی secret ها توی این پروژه استفاده شده):

```bash
# سرویس Api
export SwaggerAccess__Credentials__0__Username="frontend1"
export SwaggerAccess__Credentials__0__Password="یک-پسورد-قوی-و-یکتا"
export SwaggerAccess__Credentials__1__Username="frontend2"
export SwaggerAccess__Credentials__1__Password="یک-پسورد-قوی-و-یکتای-دیگه"

# اختیاری — اگه IP ثابت دارید، این‌ها رو هم اضافه کنید (وگرنه خالی بذارید)
export SwaggerAccess__AllowedIps__0="1.2.3.4"
export SwaggerAccess__AllowedIps__1="5.6.7.8"
```

همین متغیرها رو برای سرویس **File** هم جدا ست کنید (می‌تونید همون username/password رو تکرار کنید یا جدا بدید — فرقی نداره، دو تا سرویس مستقل‌ان).

اگه از Docker/docker-compose استفاده می‌کنید، همین‌ها رو به بخش `environment:` سرویس‌های `api` و `file` توی compose file اضافه کنید (نه توی appsettings، همونجا که بقیه‌ی secret ها ست شدن).

## بعد از ست‌کردن

- `https://api.pastil.pet/swagger` و `https://file.pastil.pet/swagger` رو باز می‌کنید → مرورگر یه popup لاگین می‌ده (استاندارد مرورگر، نه صفحه‌ی سفارشی) → با یکی از دو تا username/password وارد می‌شید → Swagger UI عادی باز می‌شه.
- اگه رمز اشتباه بزنید یا اصلاً وارد نکنید → همون response استاندارد ۴۰۱ با prompt دوباره.
- اگه IP Allowlist هم پر کرده باشید و از IP خارج از لیست بیاید → ۴۰۴ (حتی قبل از رسیدن به مرحله‌ی لاگین).

## فایل‌های مرتبط

- `backend/Api/Middleware/SwaggerAccessMiddleware.cs`
- `backend/File/Middleware/SwaggerAccessMiddleware.cs`
- کانفیگ (ساختار خالی، بدون مقدار واقعی): `backend/Api/appsettings.json` و `backend/File/appsettings.json` → کلید `SwaggerAccess`
