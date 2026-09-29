# رفع باگ: خالی‌بودن عکس راننده از سمت سرور

**گزارش مرتبط:** «عکس راننده — مقدار picture از سمت سرور اصلاً خالیه، نه این‌که URL تصویر اشتباه ساخته بشه.» — گزارش‌دهنده: @Vicopv.

## ۱. تأیید و ریشه‌ی باگ
تأیید شد. `DriverVDto.ProfilePicture` یک navigation property از EF است که فقط وقتی در کوئری صریحاً `.Include(...).ThenInclude(d => d.ProfilePicture)` بخورد پر می‌شود — وگرنه AutoMapper هم چیزی برای map‌کردن ندارد و فیلد `null` برمی‌گردد (نه خطا، نه URL اشتباه — دقیقاً چیزی که گزارش شده بود).

پنج جای مختلف در `TripService.cs` فقط `Driver` را Include می‌کردند بدون `ThenInclude(ProfilePicture)`:
- `FindAsyncVDto` — کوئری عمومی یک سفر
- `Search` — لیست/جستجوی سفرها
- `GetTripForReservationAsync` — سفر پت‌رسانِ متصل به رزرو کلینیک
- `GetTripForPansionReservationAsync` — سفر پت‌رسانِ متصل به رزرو پانسیون
- `GetTripForSchoolReservationAsync` — سفر پت‌رسانِ متصل به ثبت‌نام مدرسه

(`GetUserCurrentTrip` و `GetDriverCurrentTrip` از قبل درست بودند — این دو تحت تأثیر نبودند.)

## ۲. راه‌حل
در هر پنج مورد بالا، `.ThenInclude(d => d.ProfilePicture)` بعد از `.Include(s => s.Driver)` اضافه شد. هیچ چیز دیگری (نام فیلد، ساختار پاسخ) تغییر نکرد.

## ۳. وضعیت
- بدون تغییر schema/مایگریشن — فقط تغییر کوئری.
- بیلد کامل بدون خطا، ۵۶۲/۵۶۲ تست سبز.
- بدون Breaking Change — `driver.profilePicture` دقیقاً همان ساختار قبلی را دارد (`{id, url, orginalName, guidName, extension}`)، فقط از این به بعد واقعاً پر می‌شود.

## ۴. تأثیر بر فرانت
هیچ تغییری در قرارداد لازم نیست — فرانت همان فیلد `driver.profilePicture.url` را که از قبل می‌خواند، از این به بعد واقعاً مقدار می‌گیرد. جزئیات کامل: `webapp/docs/DRIVER_PICTURE_MISSING_FIX_ARMAN_FA.md`.
