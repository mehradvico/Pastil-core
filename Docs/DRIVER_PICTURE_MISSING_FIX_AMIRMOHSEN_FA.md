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

---

## ۵. پیگیری بعدی (همان روز): همین باگ برای `certificatePicture`/`vehicleCardPicture` هم بود

وب‌اپ‌نویس با یک نمونه‌ی واقعی از پاسخ `GetUserCurrentTrip` مستندسازی کرد:
```jsonc
"driver": {
  "profilePictureId": null,          // این یکی واقعاً چیزی ست نشده — دیتا، نه باگ
  "certificatePictureId": 11007,     // این ID دارد...
  "vehicleCardPictureId": 11008,     // این هم همین‌طور...
  "profilePicture": null,
  "certificatePicture": null,        // ...ولی آبجکتش هنوز null بود!
  "vehicleCardPicture": null,        // همین‌طور این یکی
  ...
}
```
یعنی همان کلاس باگ (Include ناقص)، این‌بار برای دو فیلد دیگر — `Driver.CertificatePicture` و `Driver.VehicleCardPicture` — که در فیکس اول اصلاً به آن‌ها فکر نشده بود (فقط `ProfilePicture` رفع شد).

### رفع شد
`.ThenInclude(d => d.CertificatePicture)` و `.ThenInclude(d => d.VehicleCardPicture)` در **هشت** کوئری‌ای که `Driver` را Include می‌کنند اضافه شد (همان پنج‌تای قبلی + `GetUserCurrentTrip`، `GetLive...` مسیرهای مرتبط نبودند، پس دست‌نخورده ماندند). `GetDriverCurrentTrip` عمداً دست‌نخورد ماند — آنجا راننده دارد سفر خودش را می‌بیند، مدارک خودش را از قبل می‌داند، نیازی به این دو فیلد نیست.

### `profilePictureId: null` — این یکی باگ نیست
تأیید می‌شود: این بخشِ گزارش وب‌اپ‌نویس **درست تشخیص داده شده بود**. راننده‌ی id=1 (امیرعلی محمدی) واقعاً هیچ‌وقت عکس پروفایل آپلود/ثبت نکرده — این یک وضعیت داده‌ای واقعی است، نه یک باگ کد. باید از طریق پنل ادمین (`Driver` → ویرایش راننده) یا از اپ راننده (اگر فرم پروفایل چنین فیلدی دارد) این عکس برای این راننده‌ی خاص ثبت شود.

### درباره‌ی 404 روی «driver-info/1»
این یک باگ بک‌اند نیست. تنها Endpoint مشابه، `GET /api/EndUser/Driver/{id}`، عمداً محدود به **مالکِ همان رکورد راننده** است:
```csharp
if (!Driver.IsSuccess || Driver.Data?.OwnerId != _currentUser.CurrentUser.UserId)
    return NotFound(...);
```
یعنی این Endpoint برای زمانی است که خودِ راننده (به‌عنوان کاربر) پروفایل رانندگی خودش را می‌بیند، نه برای اینکه یک مشتری اطلاعات راننده‌ی سفرش را از این مسیر بگیرد. اگر فرانت برای نمایش «راننده‌ی این سفر» به این مسیر زده، آدرس اشتباه است — اطلاعات راننده باید از همان پاسخ `TripCurrent`/`TripReservation`/... (فیلد `driver` روی خودِ Trip) خوانده شود، که همین چیزی است که این مستند و مستند قبلی رفعش کرده‌اند.
