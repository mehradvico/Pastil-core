# تصحیح: نمایش سفرهای پت‌رسان به راننده (پاسخ به یادداشت امیرمحسن)

**یادداشت اصلی امیرمحسن (عیناً):**
> «خودِ عملیات وجود دارد، اما به‌صورت «Trip» است؛ نه صفحه/API مستقلِ رزرو یا سرویس برای راننده.
> رزرو پت‌رسانِ متصل به خدمت، پانسیون یا مدرسه: به یک Trip تبدیل می‌شود و راننده آن را در `GET /api/Driver/TripAvailable` می‌بیند، قبول می‌کند و در TripCurrent اجرا می‌کند. تشخیص نوع رزرو از companionReserveId، pansionReserveId یا schoolReserveId است.
> سرویس دوره‌ای پت‌رسان: سیستم برای هر نوبت یک Trip می‌سازد. راننده endpoint جدا برای لیست سرویس‌ها ندارد و این‌ها در TripAvailable broadcast نمی‌شوند؛ بعد از تخصیص دستی راننده، در Trip/TripCurrent قابل اجرا هستند.
> پس «وجود دارند»، ولی پنل راننده فقط سفر اجرایی را می‌بیند؛ API فعلی جزئیات رزرو مادر یا سرویس (نام مرکز، نام مدرسه، برنامهٔ هفتگی و…) را به راننده نمی‌دهد.»

بررسی شد. **بخش اول درست است، بخش دوم (سرویس دوره‌ای پت‌رسان) با کد فعلی مطابقت ندارد.**

---

## ۱. بخشِ درست: رزرو متصل (خدمت/پانسیون/مدرسه)

تأیید شده، بدون تغییر:
- `Api/Areas/Driver/Controllers/TripAvailableController.cs` → `ITripService.GetAvailableTripsForDriverAsync`
- `Api/Areas/Driver/Controllers/TripCurrentController.cs` → `GetDriverCurrentTrip`
- تشخیص نوع رزرو از `TripVDto.CompanionReserveId`/`PansionReserveId`/`SchoolReserveId` (`Application/Services/TripSrv/TripSrv/Dto/TripVDto.cs:59-61`)

این سه فیلد **فقط شناسه‌ی خام (`long?`)** هستند — هیچ آبجکت nested (نام کلینیک، نام پانسیون، نام مدرسه، برنامه‌ی هفتگی) در `TripVDto` وجود ندارد. پس این جمله‌ی امیرمحسن («API فعلی جزئیات رزرو مادر یا سرویس را به راننده نمی‌دهد») **کاملاً درست** است — هم برای رزروهای متصل، هم برای سرویس دوره‌ای.

## ۲. بخشِ نادرست: «سرویس دوره‌ای پت‌رسان در TripAvailable broadcast نمی‌شود»

### کد واقعی
در `TripService.GenerateOnePetResanTripAsync` (خطوط ۲۲۴۰-۲۲۵۷)، Trip هر نوبت از سرویس هفتگی پت‌رسان این‌طور ساخته می‌شود:
```csharp
var trip = new Trip
{
    ...
    IsOnline = true,
    DriverStatusId = (long)DriverStatusEnum.DriverStatus_Requested,
    TripStatusId = (long)TripStatusEnum.TripStatus_Requested,
    PetResanServiceScheduleId = schedule.Id,
    // DriverId هرگز ست نمی‌شود → همیشه null
};
```

و کوئری `TripAvailable` (`TripService.GetAvailableTripsForDriverAsync`، خطوط ۱۲۶۷-۱۲۸۱) این است:
```csharp
var trips = await _context.Trips
    .Where(t => t.TripStatusId == TripStatusEnum.TripStatus_Requested
        && t.DriverId == null
        && t.IsOnline
        && (t.VehicleTypeId == null || t.VehicleTypeId == driverVehicleTypeId)
        && !_context.TripDriverExclusions.Any(...))
    ...
```

**هیچ فیلتر زمانی وجود ندارد** (مثل «فقط وقتی به زمان حرکت نزدیک شدیم») و **هیچ شرطی که Tripهای دارای `PetResanServiceScheduleId` را جدا کند** هم وجود ندارد. یعنی این Tripها دقیقاً همان سه شرط (`DriverId==null`, `IsOnline==true`, `TripStatusId==Requested`) را دارند که هر سفر لحظه‌ای معمولی دارد — پس **از همان لحظه‌ی تولید (روز قبل، طبق Job)، برای همه‌ی رانندگان در TripAvailable دیده می‌شوند و قابل قبول‌کردن‌اند**، حتی اگر ساعت حرکتشان فردا صبح باشد.

### «تخصیص دستی» چیست پس؟
تنها چیزی که در کد پیدا شد، `Api/Areas/Admin/Controllers/TripChooseDriverController.cs` است — اما این یک endpoint **عمومی برای هر Trip** است (ادمین می‌تواند برای هر سفری، از هر نوع، دستی راننده انتخاب کند)، نه چیزی مخصوص سرویس هفتگی پت‌رسان و نه مسیر انحصاری/اجباری برایش. سرویس هفتگی پت‌رسان از این نظر هیچ رفتار خاصی ندارد — کاملاً مثل هر Trip دیگری broadcast می‌شود.

### جمع‌بندی تصحیح
| ادعای امیرمحسن | وضعیت واقعی |
|---|---|
| رزرو متصل (خدمت/پانسیون/مدرسه) → Trip → دیده‌شدن در TripAvailable | ✅ درست |
| تشخیص نوع رزرو از Id های سه‌گانه، بدون جزئیات نام/برنامه | ✅ درست |
| سرویس دوره‌ای پت‌رسان در TripAvailable broadcast **نمی‌شود** | ❌ نادرست — **broadcast می‌شود**، دقیقاً مثل بقیه |
| سرویس دوره‌ای پت‌رسان نیاز به «تخصیص دستی راننده» دارد | ❌ نادرست — هیچ تخصیص دستی‌ای در مسیر عادی وجود ندارد؛ اولین راننده‌ای که در TripAvailable قبول کند می‌گیرد |

هیچ تغییری در کد لازم نشد — این فقط یک تصحیحِ توضیح فنی بود، نه یک باگ. اگر مدنظر واقعاً همین بوده که سرویس دوره‌ای پت‌رسان باید broadcast **نشود** (و فقط با تخصیص دستی اجرا شود)، این یک تغییر رفتار جدید است و باید جداگانه به‌عنوان درخواست فیچر مطرح شود.
