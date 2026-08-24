# مستند نام کاربری پروفایل‌های PastilMatch

## هدف

برای هر پروفایل PastilMatch یک نام کاربری عمومی شبیه Telegram اضافه شده است تا پروفایل با آن قابل جست‌وجو باشد. مقدار نام کاربری در بک‌اند همیشه trim و lowercase می‌شود.

نام کاربری فقط برای پروفایل‌های فعال یکتا است؛ پروفایل‌های قدیمی یا حذف‌شده می‌توانند مقدار `null` داشته باشند و نیازی به backfill ندارند.

## قوانین نام کاربری

- طول: حداقل ۵ و حداکثر ۳۲ کاراکتر
- کاراکتر اول: حرف انگلیسی کوچک (`a-z`)
- ادامه: فقط حروف انگلیسی کوچک، عدد و `_` (`a-z0-9_`)
- فاصله، حروف فارسی، خط تیره و حروف انگلیسی بزرگ مجاز نیستند؛ حروف بزرگ قبل از ارسال به lowercase تبدیل می‌شوند.
- در UI مقدار را به شکل `@username` نمایش دهید، اما در API فقط خود `username` را بفرستید.

## ساخت و ویرایش پروفایل

مسیرهای فعلی تغییر نکرده‌اند:

```http
POST /api/EndUser/PastilMatchProfile
PUT  /api/EndUser/PastilMatchProfile
```

فیلد جدید در body:

```json
{
  "username": "mehrad_1"
}
```

در `PUT` اگر کلید `username` ارسال نشود، مقدار قبلی حفظ می‌شود. برای تغییر مقدار، نام کاربری جدید را بفرستید. ارسال رشتهٔ خالی مقدار را پاک می‌کند و پروفایل دوباره بدون نام کاربری خواهد بود.

اگر نام کاربری نامعتبر یا تکراری باشد، پاسخ با `isSuccess: false` برمی‌گردد و متن خطا در `messages[0].item1` قرار دارد. در این حالت پیام خطا را به کاربر نشان دهید و درخواست را دوباره با مقدار معتبر ارسال کنید.

## جست‌وجو

جست‌وجوی دقیق با فیلد جدید:

```http
GET /api/EndUser/PastilMatchProfile?username=mehrad_1
```

پارامتر `username` نسبت به حروف بزرگ و کوچک حساس نیست و نتیجه فقط پروفایل فعال با همان نام کاربری است. پارامتر عمومی `q` نیز نام کاربری، نام پت و توضیحات پروفایل را جست‌وجو می‌کند:

```http
GET /api/EndUser/PastilMatchProfile?q=mehrad
```

در پاسخ‌های لیست و جزئیات، فیلد `username` در `data/list` قرار دارد و ممکن است برای پروفایل‌های قدیمی `null` باشد.

## استقرار Migration

بعد از Build پروژه‌ها، در Package Manager Console مقدار **Default project** را `Persistence` بگذارید و این دستورات را اجرا کنید:

```powershell
Add-Migration AddPastilMatchProfileUsername -Project Persistence -StartupProject Api -Context DataBaseContext
Update-Database -Project Persistence -StartupProject Api -Context DataBaseContext
```

برای اعمال دقیق همین migration (و دیدن لاگ کامل) می‌توانید نام آن را هم مشخص کنید:

```powershell
Update-Database -Migration 20260823140000_AddPastilMatchProfileUsername -Project Persistence -StartupProject Api -Context DataBaseContext -Verbose
```

در این commit migration آمادهٔ `20260823140000_AddPastilMatchProfileUsername` هم وجود دارد. بنابراین اگر فایل migration را از همین نسخه دریافت کرده‌اید، اجرای `Add-Migration` مجدد لازم نیست؛ فقط برای بررسی قابل شناسایی بودن آن اجرا کنید:

```powershell
Get-Migration -Project Persistence -StartupProject Api -Context DataBaseContext -Verbose
```

باید migration با نام `20260823140000_AddPastilMatchProfileUsername` در فهرست دیده شود. اگر فهرست قدیمی بود، PMC را ببندید و بعد از `Clean` و `Rebuild` دوباره باز کنید.

Migration ستون nullable `Username` با طول ۳۲ و ایندکس یکتای فیلترشدهٔ `IX_PastilMatchProfiles_Username` را ایجاد می‌کند؛ چون nullable است، داده‌های قبلی بدون تغییر باقی می‌مانند.
