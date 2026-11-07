# کارمندان پنل ارائه‌دهنده: نقش و دسترسی (ProviderUser)

تاریخ: ۲۰۲۶-۱۱-۰۳ · فقط بک‌اند · migration دارد: `ProviderUsers` (اعمال نشده)

## خلاصه

دو نوع همکار داریم و از هم جدا هستند:

| نوع | جدول | به کجا دسترسی دارد |
|---|---|---|
| **کارمند پنل ارائه‌دهنده** | `ProviderUsers` | پنل خودِ ارائه‌دهنده، فقط بخش‌هایی که نقشش اجازه می‌دهد |
| **پرسنل / اپراتور مرکز** | `ClinicUsers`، `GroomerUsers`، `CoachUsers` (قبلاً هم بودند) | فقط پنل اپراتور (`/api/Operator/*`) و کارهای شخصی خودش (من آنلاینم، جلسه‌ی مشاوره‌ی من، نسخه، حضور و غیاب کلاس). **به پنل ارائه‌دهنده دسترسی ندارد.** |

- **فقط مالک ارائه‌دهنده** (کاربر `Provider.UserId`) نقش می‌سازد، دسترسی می‌دهد، کارمند دعوت/غیرفعال/حذف می‌کند. هیچ کارمندی، حتی با همه‌ی دسترسی‌ها، نمی‌تواند.
- دسترسیِ **مادر = نوع نمایندگی** (فروشگاه، کلینیک، آرایشگاه، مربی، پانسیون، مدرسه). زیر هر نمایندگی بخش‌ها هستند و هر بخش دو سطح دارد: «مشاهده» و «مدیریت». «مدیریت» خودش «مشاهده»ی همان بخش را هم می‌دهد.
- مثال: نقش «مسئول کلینیک» = همه‌ی بخش‌های کلینیک. نقش «حسابدار کلینیک» = فقط `Clinic.Finance.View`. حسابدار کلینیک رزرو کلینیک، حسابداری آرایشگاه یا فروشگاه را نمی‌بیند.
- دسترسی‌ها در هر درخواست از دیتابیس خوانده می‌شوند (کش ندارند). غیرفعال‌کردن کارمند، حذف او یا کم‌کردن دسترسی نقش، **از همان درخواست بعدی** اثر می‌کند.

## کاتالوگ دسترسی‌ها

کلید = `{نمایندگی}.{بخش}.{سطح}` (حساس به حروف). سطح = `View` یا `Manage`.

| نمایندگی | بخش‌ها |
|---|---|
| `Store` فروشگاه | `Profile` (اطلاعات، ساعت کاری، روش‌های ارسال) · `Product` · `Order` (سفارش، لغو، تغییر وضعیت، کد رهگیری، ارسال میاره) · `Discount` · `Review` · `Finance` (فقط View) |
| `Clinic` کلینیک | `Profile` (اطلاعات، عکس، ساعت/محدوده) · `Service` · `Reserve` · `Consultation` (بسته و برنامه‌ی مشاوره‌ی کلینیک) · `Review` · `Staff` (اپراتورهای کلینیک) · `Finance` (فقط View) |
| `Groomer` آرایشگاه | `Profile` · `Service` · `Reserve` · `Portfolio` · `Review` · `Staff` · `Finance` (فقط View) |
| `Coach` مربی | `Profile` · `Service` · `Reserve` · `Consultation` · `Review` · `Staff` · `Finance` (فقط View) |
| `Pansion` پانسیون | `Profile` · `Reserve` (تأیید/رد/لغو/تغییر وضعیت) · `Review` · `Finance` (فقط View) |
| `School` مدرسه | `Profile` · `Course` (دوره، جلسه، ویدیو، لایو، مدرس، حضور، مهارت، گواهی) · `Reserve` (ثبت‌نام‌ها، لیست انتظار) · `Finance` (تسویه + داشبورد درآمد؛ فقط View) |

قاعده‌ی هر endpoint: `GET` = `…View`، بقیه‌ی متدها = `…Manage`. حسابداری `Manage` ندارد، پس هیچ متد نوشتنی‌ای روی حسابداری برای کارمند باز نمی‌شود. نگاشت کامل تک‌تک کنترلرها: `Api/Authorization/ProviderEndpointPolicy.cs`.

### همیشه فقط مالک (حتی با همه‌ی دسترسی‌ها)

- ساخت/ویرایش/حذف نقش، دادن دسترسی، دعوت و مدیریت کارمند (`ProviderUserRole`، `ProviderUser`، `ProviderPermission`)
- ساختن مرکز جدید و حذف مرکز (`POST`/`DELETE` روی `Clinic`، `Coach`، `Groomer`، `Pansion`، `School`)
- نمایندگی قدیمی و مالی سطح مالک: `Companion*`، `FinanceCompanion`، `Settlement` (ناحیه‌ی Companion)، `ConsultationAvailability`/`ConsultationPackage` بدون مرکز، `CompanionUserGetMobile`
- ثبت دریافت بدهی رزرو (`CompanionReserveDebt/MarkPaid`): نوع رزرو در بدنه است و قابل نگاشت دقیق نیست. مالک و پرسنلِ مسئولِ همان رزرو (با هویت خودشان) می‌توانند.
- هر کنترلر جدیدی که هنوز در `ProviderEndpointPolicy` نیامده (پیش‌فرض بسته؛ تست جلوی merge را می‌گیرد).

## API مالک (ناحیه‌ی Companion، بدون هدر)

| متد | مسیر | کار |
|---|---|---|
| `GET` | `/api/Companion/ProviderPermission` | درخت دسترسی‌ها برای فرم نقش؛ فقط نمایندگی‌هایی که ارائه‌دهنده دارد (نقش تأییدشده‌ی ادمین یا مرکز ثبت‌شده) |
| `GET` | `/api/Companion/ProviderUserRole` | نقش‌ها + `permissionKeys` + `userCount` |
| `POST` | `/api/Companion/ProviderUserRole` | `{ name, description, permissionKeys: [] }` |
| `PUT` | `/api/Companion/ProviderUserRole` | `{ id, name, description, permissionKeys }` — دسترسی‌ها کامل جایگزین می‌شوند |
| `DELETE` | `/api/Companion/ProviderUserRole?id=` | نقشی که به کارمندی داده شده حذف نمی‌شود |
| `GET` | `/api/Companion/ProviderUser` | کارمندان (موبایل، نقش، وضعیت دعوت) |
| `POST` | `/api/Companion/ProviderUser` | دعوت: `{ mobile, providerUserRoleId }` |
| `PUT` | `/api/Companion/ProviderUser/Role` | `{ id, providerUserRoleId }` |
| `PUT` | `/api/Companion/ProviderUser/Active?id=&active=` | فعال/غیرفعال |
| `DELETE` | `/api/Companion/ProviderUser?id=` | حذف کارمند |

خطاها (همه HTTP 200 با `isSuccess=false` مگر رد فیلتر): `ProviderOwnerOnlyAction`، `ProviderUserRoleNameRequired`، `ProviderUserRoleNameDuplicate`، `ProviderUserRolePermissionInvalid` (کلید ناشناخته یا نمایندگی‌ای که ارائه‌دهنده ندارد؛ کل درخواست رد می‌شود)، `ProviderUserRoleInUse`، `ProviderUserRoleNotFound`، `ProviderUserAlreadyExists`، `ProviderUserCannotInviteSelf`، `ProviderUserNotFound`، `ProviderUserLimitReached` (۵۰ نقش / ۲۰۰ کارمند).

## API خود کارمند (ناحیه‌ی EndUser)

| متد | مسیر | کار |
|---|---|---|
| `GET` | `/api/EndUser/ProviderUserInvite` | دعوت‌نامه‌های منتظر من |
| `PUT` | `/api/EndUser/ProviderUserInvite?id=&accept=` | پذیرش/رد (تا نپذیرد هیچ دسترسی ندارد) |
| `GET` | `/api/EndUser/ProviderMembership` | ارائه‌دهنده‌هایی که کارمندشان هستم: `{ providerUserId, providerId, providerName, roleName, permissions[], scopes[] }` |
| `DELETE` | `/api/EndUser/ProviderMembership?id=` | ترک همکاری |

`GET /api/EndUser/CompanionPanelAccess` حالا `isProviderUser` هم دارد (فقط برای نمایش منو؛ `canEnter` را عوض نمی‌کند).

## کار کردن کارمند در پنل

هر درخواست پنل (ناحیه‌های `Companion` و `Seller`) هدر می‌گیرد:

```
X-Provider-Id: <providerId از ProviderMembership>
```

`ProviderPanelAccessFilter` به ترتیب چک می‌کند: عدد معتبر → endpoint «فقط مالک» نباشد → کاربر کارمند فعال، پذیرفته و حذف‌نشده‌ی **همین** ارائه‌دهنده باشد (ارائه‌دهنده فعال و تأییدشده، مالک قفل/حذف نشده، نقش حذف نشده) → نقش کلید همین endpoint را داشته باشد. هر کدام نشد: **HTTP 403** با `ProviderActingAccessDenied` / `ProviderOwnerOnlyAction` / `YouHaveNotPermission`، و رویداد `ProviderStaffDenied` در `Security.Audit`.

اگر قبول شد، هویت **همین یک درخواست** مالک ارائه‌دهنده می‌شود تا همان چک‌های مالکیتِ موجود هر endpoint داده را به ارائه‌دهنده‌ی خودش محدود کنند (داده‌ی ارائه‌دهنده‌ی دیگر هرگز دیده نمی‌شود). کاربر واقعی در `CurrentUser.ActorUserId` می‌ماند، نقش سیستمی همان نقش خود کارمند است (اگر مالک ادمین باشد کارمند ادمین نمی‌شود)، و هر درخواست غیر GET با رویداد `ProviderStaffAction` (کارمند، ارائه‌دهنده، endpoint، کلید) ثبت می‌شود. در ناحیه‌ی Seller فروشگاهِ درخواست، فروشگاه همان ارائه‌دهنده است.

endpointهای شخصی (`ConsultationOnline`، `ClinicConsultationOnline`، `CoachConsultationOnline`، `ConsultationSession`، `OnlinePrescription`، `CompanionDaySchedule`، `SchoolTeaching/MyCourses`) هدر را نادیده می‌گیرند و همیشه با هویت خود کاربر اجرا می‌شوند.

## تغییر رفتار برای پرسنل/اپراتور مراکز (مهم)

قبلاً هر عضو پذیرفته‌شده‌ی `ClinicUsers`/`GroomerUsers`/`CoachUsers` به **همه‌ی** endpointهای `api/Companion/*` راه داشت (فقط چک مالکیت داخل هر endpoint جلویش را می‌گرفت). حالا پرسنلی که خودش مالک/ارائه‌دهنده نیست فقط به این‌ها می‌رسد: endpointهای شخصی بالا، `SchoolTeaching` و `SchoolCourseLive` (مدرس دوره)، `CompanionReserveDebt/MarkPaid` (پرسنل مسئول رزرو) و جدول پایه‌ی `WeekDay`. پنل اپراتور (`/api/Operator/*`) تغییری نکرده. اگر اپ پرسنل جای دیگری از `api/Companion/*` را صدا می‌زند، 403 می‌گیرد؛ قبل از deploy با تیم اپ چک شود.

## کار اپ و وب‌اپ (کدنویسی نشده)

1. **مالک:** صفحه‌ی «کارمندان و نقش‌ها» در پنل ارائه‌دهنده: لیست نقش‌ها، فرم نقش با درخت `ProviderPermission` (نمایندگی ← بخش ← چک‌باکس مشاهده/مدیریت؛ تیک مدیریت، مشاهده را هم تیک بزند)، لیست کارمندان، دعوت با موبایل و انتخاب نقش.
2. **کارمند:** بعد از ورود `ProviderUserInvite` (بنر دعوت) و `ProviderMembership` را بگیرید. اگر عضویتی هست، ورودی «پنل {providerName}» نشان دهید. با انتخاب آن، `providerId` را نگه دارید و روی **همه‌ی** درخواست‌های `api/Companion/*` و `api/Seller/*` هدر `X-Provider-Id` بفرستید.
3. **منو:** فقط بخش‌هایی را بسازید که کلیدشان (یا `Manage` همان بخش) در `permissions` هست؛ نمایندگی‌ای که در `scopes` نیست اصلاً نمایش داده نشود؛ دکمه‌های ویرایش/تأیید فقط با `…Manage`. منوی «کارمندان و نقش‌ها» برای کارمند هرگز نمایش داده نشود. (این فقط برای تجربه‌ی کاربری است؛ امنیت را بک‌اند تضمین می‌کند.)
4. پاسخ 403 در حالت کارمند یعنی دسترسی عوض شده: `ProviderMembership` را دوباره بگیرید و منو را بازسازی کنید.
5. **وب‌اپ:** هدر `X-Provider-Id` باید در BFF عبور داده شود و مسیرهای جدید به allow-list پنل (`webapp/app/server/utils/clinicRoutes.js`) اضافه شوند. CORS هم‌اکنون `AllowAnyHeader` است.

## دیتابیس

- `ProviderUserRoles` (`ProviderId`, `Name` یکتا در هر ارائه‌دهنده، `Description`, `Deleted`, `CreateDate`)
- `ProviderUserRolePermissions` (`ProviderUserRoleId`, `PermissionKey`؛ یکتا روی هر دو)
- `ProviderUsers` (`ProviderId`, `UserId` یکتا در هر ارائه‌دهنده، `ProviderUserRoleId`, `UserAccept`, `InviteDate`, `Active`, `Deleted`, `CreateDate`)

ترتیب deploy: اول `backend/scripts/ProviderUsers.sql` (تکرارپذیر)، بعد Api. Payment تغییری نکرده.

## فایل‌ها

- `Entities/Entities/ProviderField/ProviderUser*.cs`
- `Application/Services/ProviderSrvs/ProviderUserSrv/` (`ProviderPermissionCatalog`، `ProviderUserService`، DTOها)
- `Application/Common/CurrenUser/ProviderActingContext.cs` + `CurrentUserHelper` + فیلدهای `ActingProviderId`/`ActingProviderUserId`/`ActorUserId` در `CurrentUserDto`
- `Api/Authorization/ProviderEndpointPolicy.cs`، `Api/Filters/ProviderPanelAccessFilter.cs` (جایگزین `CompanionAreaAccessFilter`)، `AreaMembershipAuthorizationHandler` (درخواست با هدر را به فیلتر می‌سپارد)
- کنترلرها: `Api/Areas/Companion/Controllers/Provider{Permission,UserRole,User}Controller.cs`، `Api/Areas/EndUser/Controllers/Provider{UserInvite,Membership}Controller.cs`
- تست: `Application.Tests/Security/ProviderStaffAccessTests.cs` (نگاشت همه‌ی کنترلرها، کاتالوگ، فیلتر، سرویس روی LocalDB)

## محدودیت‌ها / بعداً

- محدوده‌ی دسترسی **نوع نمایندگی** است، نه یک مرکز خاص: «مسئول کلینیک» به همه‌ی کلینیک‌های همان ارائه‌دهنده دسترسی دارد. محدود کردن به یک کلینیک مشخص نیازمند فیلتر شناسه‌ی مرکز در تک‌تک سرویس‌هاست (فاز بعد).
- دعوت کارمند فعلاً پوش ندارد؛ کاربر دعوت را در `ProviderUserInvite` می‌بیند.
- استثنای چند نشستیِ مالک (`Security:CompanionOwnerMaxSessions`) برای این بود که پرسنل با حساب مالک کار می‌کردند؛ بعد از راه افتادن کارمندان پنل در اپ می‌شود آن را خاموش کرد.

## نقش‌های آماده‌ی ادمین زیر هر نمایندگی (۲۰۲۶-۱۱-۰۷)

- ادمین اصلی برای هر نمایندگی (Store/Clinic/Groomer/Coach/Pansion/School) نقش آماده تعریف می‌کند؛ ردیف `ProviderUserRoles` با `ProviderId = NULL` و `Scope` = همان نمایندگی (+ `Priority` برای ترتیب). check constraint `CK_ProviderUserRoles_TemplateScope`: نقش آماده Scope دارد، نقش مالک ندارد. نام نقش آماده در هر نمایندگی یکتاست (`IX_ProviderUserRoles_Template_Scope_Name`).
- **نمایش و اعتبار فقط با نمایندگی فعال:** مالک در `GET api/Companion/ProviderUserRole` نقش‌های آماده‌ی همان نمایندگی‌هایی را می‌بیند که الان دارد (همان قاعده‌ی `ScopesOfProviderAsync`: نقش تأییدشده‌ی ادمین یا مرکز ثبت‌شده). نقش کلینیک فعال نشده = «مسئول کلینیک» دیده نمی‌شود و قابل انتساب نیست؛ به‌محض فعال شدن نمایش داده می‌شود. اگر نمایندگی بعداً برداشته شود، کارمندِ آن نقش آماده در همان درخواست بعدی دسترسی‌اش را از دست می‌دهد (`ResolveActingAsync`/`ListMembershipsAsync`).
- نقش آماده فقط کلیدهای همان Scope را می‌دهد (حتی اگر به‌اشتباه کلید دیگری در دیتابیس باشد).
- مالک نقش آماده را فقط به کارمند می‌دهد (`IsTemplate = true` در خروجی)؛ ویرایش/حذف آن فقط از ادمین. پت‌رسان نقش ندارد.
- API ادمین: `api/Admin/ProviderUserRoleTemplate` — `GET ?scope=`، `GET Catalog`، `POST`، `PUT` (نمایندگی عوض نمی‌شود)، `DELETE ?id=` (نقش در حال استفاده حذف نمی‌شود). بعد از دیپلوی Permission Sync لازم است.
- SQL: `backend/scripts/ProviderRoleTemplates.sql` (اسکیما + ۳۰ نقش آماده؛ idempotent). migration: `ProviderRoleTemplates`. سرویس: `ProviderUserSrv/ProviderUserRoleTemplateService.cs`.

## شناسه‌ی فروشگاه برای کارمند (۲۰۲۶-۱۱-۰۷)

- `GET api/EndUser/ProviderMembership` حالا `StoreId` هم دارد: فقط برای کارمندی که دسترسی فروشگاه (`Store.*`) دارد پر می‌شود، وگرنه `null`.
- `GET api/Seller/Store/Current` (بدون شناسه): برای مالک فروشگاه خودش، برای کارمند (با هدر `X-Provider-Id`) فروشگاه همان ارائه‌دهنده. دسترسی لازم `Store.Profile.View`.
- قاعده‌ی انتخاب فروشگاه یکی است (`ProviderUserService.ResolveStoreIdAsync`، هم در `ProviderPanelAccessFilter` هم در membership): از فروشگاه‌های همین ارائه‌دهنده، آنی که مالک عضوش است (کمترین شناسه)، وگرنه اولین فروشگاه ارائه‌دهنده.
- اپ: صفحه‌ی خانه‌ی فروشگاه برای کارمند را با `Store/Current` (یا `Store/{membership.StoreId}`) باز کند؛ بقیه‌ی endpointهای Seller شناسه‌ی فروشگاه نمی‌خواهند و فقط هدر لازم دارند.
