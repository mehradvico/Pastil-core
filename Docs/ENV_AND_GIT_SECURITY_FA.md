# راهنمای تنظیمات محرمانه و Push امن

## فایل‌های محیطی

فایل `.env` در ریشه پروژه برای اطلاعات محرمانه ساخته شده و توسط Git نادیده گرفته می‌شود. فایل `.env.example` فقط نام متغیرها را دارد و قابل Push است.

مقدارهای واقعی را فقط بعد از علامت `=` در فایل `.env` وارد کنید:

```dotenv
PASTIL_API_CONNECTION=
PASTIL_PAYMENT_CONNECTION=
PASTIL_FILE_CONNECTION=
PASTIL_REALTIME_CONNECTION=
PASTIL_JWT_KEY=
PASTIL_PASSWORD_PEPPER=
PASTIL_VAPID_PUBLIC_KEY=
PASTIL_VAPID_PRIVATE_KEY=
PASTIL_AI_GEMINI_API_KEY=
PASTIL_AI_GROQ_API_KEY=
PASTIL_AI_OPENAI_API_KEY=
PASTIL_AI_DEEPSEEK_API_KEY=
PASTIL_AI_AVALAI_API_KEY=
PASTIL_AI_GAPGPT_API_KEY=
```

## امنیت رمز عبور کاربران

رمز کاربران با `ASP.NET Core PasswordHasher`، الگوریتم PBKDF2، Salt اختصاصی و
`600000` iteration ذخیره می‌شود. Hashهای قدیمی SHA-256 در اولین ورود موفق کاربر
به‌صورت خودکار به فرمت جدید ارتقا پیدا می‌کنند و نیازی به Reset گروهی رمزها نیست.

برای فعال‌کردن Pepper روی سرور، یک مقدار تصادفی حداقل 32 بایتی بسازید و فقط در
فایل `.env` سرویس API قرار دهید:

```bash
openssl rand -base64 48
```

```env
PASTIL_PASSWORD_PEPPER=GENERATED_RANDOM_VALUE
```

این مقدار نباید داخل Git، دیتابیس یا `appsettings.json` واقعی قرار بگیرد. بعد از
استقرار نیز Pepper را حذف یا عوض نکنید؛ تغییر آن باید همراه با برنامه مهاجرت یا
نگهداری Pepper قبلی انجام شود، وگرنه Hashهایی که با مقدار قبلی ساخته شده‌اند قابل
اعتبارسنجی نخواهند بود.

مقدارهای داخل `.env` را داخل کوتیشن قرار ندهید مگر اینکه خود مقدار واقعاً با کوتیشن شروع یا تمام شود. وجود `=` یا `;` داخل Connection String مشکلی ایجاد نمی‌کند.

## اجرای محلی

پروژه‌های `Api`، `Payment`، `File` و `RealTime` هنگام شروع فایل `.env` را از پوشه جاری یا پوشه‌های والد پیدا و بارگذاری می‌کنند. متغیر محیطی واقعی سیستم همیشه بر مقدار فایل `.env` اولویت دارد.

## استقرار روی سرور

روش پیشنهادی روی سرور، تعریف Environment Variable در IIS، Docker یا سرویس سیستم‌عامل است. اگر قرار است فایل مشترک استفاده شود، مسیر آن را با متغیر زیر مشخص کنید:

```text
PASTIL_ENV_FILE=D:\secure\pastil.env
```

فایل محیطی سرور را داخل پوشه Git یا مسیر قابل دانلود سایت قرار ندهید.

## کنترل قبل از Commit

```powershell
.\scripts\Test-NoTrackedSecrets.ps1
```

این دستور مقدار محرمانه را چاپ نمی‌کند و فقط در صورت پیدا شدن مورد مشکوک، نوع مورد و نام فایل را گزارش می‌دهد.

## نکته مهم درباره تاریخچه Git

خالی‌کردن `appsettings.json` فقط نسخه فعلی را امن می‌کند. اگر کلید قبلاً Commit شده باشد، باید Commitهای محلی حاوی آن بازنویسی شوند و کلیدهای افشاشده نیز از پنل Provider باطل و دوباره ساخته شوند.
