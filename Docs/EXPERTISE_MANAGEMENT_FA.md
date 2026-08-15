# مدیریت تخصص کاربران نمایندگی

## ساختار جدید

`Expertise` یک موجودیت مرجع مستقل است و عنوان شغلی دیگر به‌صورت متن آزاد در `CompanionUser` ثبت نمی‌شود.

- ادمین تخصص‌ها را از بخش «مدیریت تخصص‌ها» تعریف، ویرایش، فعال/غیرفعال یا حذف می‌کند.
- هنگام افزودن یا ویرایش کاربر نمایندگی، فقط تخصص‌های فعال در Dropdown نمایش داده می‌شوند.
- ارتباط کاربر نمایندگی با فیلد `expertiseId` ذخیره می‌شود.
- پس از تأیید درخواست عضویت توسط کاربر، نام تخصص انتخاب‌شده در `User.Expertise` کپی و در پروفایل نمایش داده می‌شود.
- تغییر تخصص یک عضویت فعال و تأییدشده، عنوان پروفایل کاربر را نیز همان لحظه به‌روزرسانی می‌کند.
- تخصصی که استفاده شده قابل حذف نیست و باید غیرفعال شود.

## API عمومی Dropdown

```http
GET /api/Expertise?Available=true&PageIndex=1&PageSize=500&SortBy=6
```

نمونه آیتم پاسخ:

```json
{
  "id": 1,
  "name": "دامپزشک عمومی",
  "priority": 100,
  "active": true
}
```

## API مدیریت ادمین

```http
GET    /api/Admin/Expertise
GET    /api/Admin/Expertise/{id}
POST   /api/Admin/Expertise
PUT    /api/Admin/Expertise
DELETE /api/Admin/Expertise?id={id}
```

بدنه ثبت یا ویرایش:

```json
{
  "id": 0,
  "name": "دامپزشک عمومی",
  "priority": 100,
  "active": true
}
```

## افزودن کاربر نمایندگی

در درخواست‌های `CompanionUser` به‌جای `expertise` متنی، شناسه ارسال شود:

```json
{
  "userId": 25,
  "companionId": 7,
  "active": true,
  "expertiseId": 1
}
```

`expertiseId` باید متعلق به یک Expertise فعال و حذف‌نشده باشد؛ در غیر این صورت بک درخواست را رد می‌کند.

## مراحل انتشار

1. Migration با نام `AddExpertiseCatalog` روی دیتابیس اجرا شود.
2. API جدید Publish و آپلود شود.
3. Permissionهای Reflection/Sync اجرا شوند تا «مدیریت تخصص‌ها» زیر «مدیریت نمایندگان» ساخته شود.
4. دسترسی Expertise به Role ادمین موردنظر داده شود.
5. خروجی جدید پنل ادمین و اپ آپلود شود.
