# خدمت ← نقش (Assistance.ProviderRoleId)

**تاریخ:** ۲۰۲۶-۱۰-۲۴ · **Migration:** `AssistanceProviderRole` (اجرا نشده) · **SQL:** `backend/scripts/AssistanceProviderRole.sql`

## مشکل
کاتالوگ خدمات (`Assistance`) مشترک است ولی تنها تفکیکش فلگ `IsGrooming` بود. هر مرکز غیر-آرایشگاه (کلینیک، مربی، نماینده‌ی قدیمی) *همه‌ی* خدمات غیر-گرومینگ را در لیست انتخاب می‌دید و می‌توانست ثبت کند؛ یعنی مربی خدمات کلینیک را هم می‌دید.

## مدل
- `Assistance.ProviderRoleId` (nullable، FK به `ProviderRoles`، Restrict) — نقش مالک خدمت. `IsGrooming` برای سازگاری نگه داشته شده و همیشه برابر «نقش = Groomer» است (سرور هنگام ذخیره تنظیم می‌کند).
- قاعده‌ی واحد: `Application/Services/CompanionSrvs/AssistanceSrv/AssistanceRoleRules.cs`
  - Clinic / Coach / Pansion / School: فقط خدمتی که نقشش دقیقاً همان است.
  - Groomer: `IsGrooming` یا نقش Groomer.
  - `Companion` (نماینده‌ی قدیمی، شبه‌نقش): بدون نقش + Clinic/Pansion/School؛ نه Groomer و نه Coach.

## بک‌اند
- ادمین: `Admin/Assistance` — `POST/PUT` اعتبارسنجی نقش (`InsertValidatedAsync/UpdateValidatedAsync`)؛ نقش برای خدمت جدید الزامی است؛ خدمت قدیمیِ بدون نقش را می‌شود بدون نقش ذخیره کرد ولی نقش موجود برداشته نمی‌شود؛ تغییر نقشِ خدمتی که مراکز نقش دیگر هم‌اکنون استفاده می‌کنند رد می‌شود (`AssistanceProviderRoleInUse`).
- فیلتر لیست (Admin/EndUser/عمومی `api/Assistance`): `?ProviderRole=Clinic|Coach|Groomer|Pansion|School|Companion` (بر `IsGrooming` غلبه دارد) و `?ProviderRoleId=` (`0` = بدون نقش). `AssistanceVDto` حاوی `providerRoleId/Name/Label`.
- اعمال سمت سرور هنگام تعریف خدمتِ مرکز (کنترل نقش، نه فقط فیلتر UI): `ClinicCatalogService`/`CoachCatalogService`/`GroomerCatalogService.AddServiceAsync` و `CompanionAssistanceService` Insert/Update → `Notification.AssistanceNotForRole`.
- نقش‌های قابل انتخاب: `GET Admin/Expertise/groups` (همان فهرستِ گروه‌های تخصص).

## پنل ادمین (`panel/`)
- `components/assistance/AssistanceEditor.vue`: انتخاب «نقش مالک خدمت» (جایگزین چک‌باکس گرومینگ)؛ `pages/admin/assistance/index.vue`: فیلتر و بج نقش؛ `store/useAssistanceStore.ts` (`fetchRoles`, `providerRoleId`).
- `AssistanceSearchSelect` prop جدید `provider-role` — در `Clinic/Coach/GroomerServicesPanel` و `CompanionAssistanceFormModal` اعمال شد.

## اپ نماینده (`webapp/`)
- `useCenterApi().pub.assistances()` (کلینیک/مربی) اضافه شد — قبلاً وجود نداشت و صفحه‌ی خدمات کلینیک/مربی لیست کاتالوگ را نمی‌گرفت؛ route `GET Assistance` به `server/utils/clinicRoutes.js` اضافه شد. `useGroomerApi().pub.assistances()` و صفحه‌ی `companionProfile/companionAssistance/insert[prId]` با `ProviderRole` فیلتر می‌شوند.

## دیپلوی
1. SQL `scripts/AssistanceProviderRole.sql` را روی دیتابیس اجرا کنید (ستون + backfill: گرومینگ→Groomer، خدماتِ فقط-مربی→Coach، بقیه→Clinic).
2. Api را دیپلوی کنید (Payment تغییری ندارد). 3. پنل و webapp.
4. در پنل ادمین خدمات را نقش‌دهی/اصلاح کنید (به‌ویژه خدماتی که مربی/پانسیون/مدرسه باید ببینند و پیش‌فرض Clinic شده‌اند).
