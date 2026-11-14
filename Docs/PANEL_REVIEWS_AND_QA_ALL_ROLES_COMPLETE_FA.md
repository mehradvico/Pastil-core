# مستند نظرات و پرسش‌وپاسخ پنل‌های Seller، Companion، Operator و Driver

**مبنای سند:** سورس فعلی مخزن در ۴ اکتبر ۲۰۲۶  
**مخاطب:** فرانت‌اند پنل‌ها، محصول و QA  
**محدوده:** قرارداد موجود API؛ این سند درخواست توسعهٔ endpoint جدید نیست.

## ۱. نقشهٔ قابلیت‌ها

| پنل | نظر/امتیاز قابل مدیریت | پرسش‌وپاسخ محصول | توضیح |
|---|---|---|---|
| Seller | `StoreComment`: فهرست، جزئیات، ثبت پاسخ و تغییر وضعیت | endpoint اختصاصی ندارد | نظر **محصول** نیز در Area فروشنده endpoint ندارد. |
| Companion | `CompanionComment`: فهرست، جزئیات، پاسخ و وضعیت؛ `CompanionReserveComment`: فقط فهرست و جزئیات | endpoint اختصاصی ندارد | پاسخ به نظر رزرو در Area نماینده وجود ندارد. کامنت لایو مدرسه، جریان جداگانه‌ای است. |
| Operator | ندارد | ندارد | Area اپراتور فقط رزرو و سابقهٔ پت را پوشش می‌دهد. |
| Driver | ندارد | ندارد | Area راننده فقط سفر/وضعیت و lookupهای مربوط را پوشش می‌دهد. |

**سه مفهوم مستقل:**

1. `StoreComment` و `CompanionComment` نظر عمومی دربارهٔ فروشگاه/نماینده‌اند. فیلد `answer` پاسخ صاحب پنل به همان نظر است.
2. `CompanionReserveComment` نظر مشتری دربارهٔ **یک رزرو تکمیل‌شده** است؛ امتیاز آن از پرسشنامهٔ خدمت محاسبه می‌شود. `AssistanceQuestionnaire` پرسشنامهٔ امتیازدهی است، نه انجمن پرسش‌وپاسخ.
3. `DiscussionQuestion` و `DiscussionAnswer` پرسش‌وپاسخ **محصول** هستند؛ در API عمومی/کاربر/ادمین پیاده شده‌اند، نه در چهار Area این سند. این‌ها به `ProductId` وصل‌اند و با `answer` نظر یا پرسشنامهٔ رزرو ارتباط مستقیم ندارند.

## ۲. قرارداد مشترک HTTP و داده

ریشهٔ همهٔ مسیرها `/api` است. مسیرهای پنلی زیر `[Authorize]` دارند و با `Authorization: Bearer <token>` فراخوانی می‌شوند. روی کنترلرهای Seller/Companion این بخش policy نقش مستقل دیده نمی‌شود؛ scopeهایی که در ادامه آمده‌اند را خود action/service اعمال می‌کنند. بدنهٔ `PUT`، JSON است. پاسخ موفق/ناموفقِ عملیات تکی معمولاً HTTP `200` است؛ نتیجه را از `isSuccess` بخوانید:

```json
{ "code": 0, "isSuccess": true, "messages": [], "data": {} }
```

`data` در `GET /{id}` و بعضی `POST`ها وجود دارد؛ `PUT` عموماً فقط `code`, `isSuccess`, `messages` دارد. در مقابل، `GET` فهرست **مستقیم** شیء جستجو برمی‌گرداند و `isSuccess`/`data` ندارد:

```json
{ "pageIndex": 1, "pageSize": 20, "sortBy": 1, "totalCount": 0, "list": [] }
```

نام‌های JSON به‌صورت camelCase در مثال‌ها آمده‌اند. `messages` از `Tuple<string,string>` ساخته می‌شود؛ نمایش دقیق عضو آن تابع تنظیم JSON سرور است و UI باید متن پاسخ واقعی را بخواند. `GET /{id}` در Seller و `CompanionComment` از `*CommentDto` برمی‌گردد، ولی اقلام لیست `*CommentVDto` هستند؛ شکل این دو را یکی فرض نکنید.

### ۲.۱ فیلدهای مشترک جستجو

| Query | نوع/پیش‌فرض | کاربرد واقعی |
|---|---|---|
| `pageIndex` | عدد صحیح؛ پیش‌فرض ۱؛ clamp به ۱ تا ۱٬۰۰۰٬۰۰۰ | صفحه‌بندی |
| `pageSize` | عدد صحیح؛ پیش‌فرض ۲۰؛ ۱ تا ۱٬۰۰۰ | تعداد در صفحه؛ مقدار کمتر از ۱ به ۲۰ تبدیل می‌شود. |
| `sortBy` | enum عددی؛ پیش‌فرض `1` | `0=Default`، `1=New`، `2=Old`؛ اثر واقعی برای هر سرویس پایین آمده است. |
| `q` | رشته | در سرویس‌های نظر این سند برای جستجوی متن اعمال **نمی‌شود**. |
| `available` | boolean/null | در `CompanionReserveComment` فقط با `allStatus=false` معنی دارد؛ در دو سرویس نظر عمومی به فیلتر وضعیت وصل نیست. |

### ۲.۲ مدل پایهٔ نظر

`CommentDto`، مدل بدنهٔ ویرایش/ثبت و خروجی جزئیات عمومی است:

| فیلد | نوع | معنی و رفتار |
|---|---|---|
| `id` | `long` | شناسهٔ نظر؛ برای `PUT` لازم است. |
| `name` | `string` | نام ثبت‌کننده؛ در ثبت نظر عمومی Seller/Companion سمت سرور خالی بودن آن رد می‌شود. |
| `text` | `string` | متن نظر؛ با `[Required]` اعتبارسنجی می‌شود. |
| `rate` | `int?` | امتیاز اختیاری؛ برای ثبت نظر عمومی در صورت وجود باید ۱ تا ۵ باشد. |
| `statusId` | `long` | شناسهٔ وضعیت. |
| `answer` | `string` | پاسخ فروشگاه/نماینده یا ادمین به نظر. |
| `userId` | `long?` | کاربر ثبت‌کننده؛ در مسیرهای ایجاد کاربر از توکن تعیین می‌شود. |
| `likeCount`, `disLikeCount` | `int` | شمارنده‌های واکنش؛ در `PUT`های پنلی تغییر نمی‌کنند. |

`CommentVDto` در **آیتم‌های لیست** شامل `id`, `name`, `text`, `rate`, `statusId`, `createDate`, `upOrDownThumb`, `answer`, `userId`, `likeCount`, `disLikeCount`, `user`, `status` است. `status` یک `CodeDto` و `user` خلاصهٔ کاربر است؛ nullable بودن داده‌های مرتبط را در UI لحاظ کنید. خروجی‌های اختصاصی، فیلدهای اضافه دارند که در بخش هر پنل آمده‌اند. `name` در `CommentVDto` از مدل پایهٔ `Name_FieldDto` است؛ وجود مقدار معنادار آن را برای همهٔ رکوردها فرض نکنید.

### ۲.۳ گزینه‌های وضعیت

گروه کد `Comment` (در داده‌های مرجع مخزن `CodeGroupId=1`):

| `statusId` مرجع | label | عنوان | کاربرد |
|---:|---|---|---|
| ۱ | `Comment_NotChecked` | بررسی نشده | وضعیت اولیهٔ نظر تازه |
| ۲ | `Comment_Accept` | تأیید شده | انتشار/تأیید |
| ۳ | `Comment_Reject` | رد شده | رد نظر |

خود سرویس‌های `PUT StoreComment` و `PUT CompanionComment` **مقدار `statusId` را به این سه گزینه محدود نمی‌کنند** و صرفاً مقدار ارسالی را ذخیره می‌کنند؛ UI باید فقط شناسهٔ معتبر گروه `Comment` بفرستد. `answer` و `statusId` دو فیلد مستقل‌اند؛ پاسخ‌دادن به‌تنهایی الزاماً نظر را تأیید نمی‌کند. در `GET` فهرست پنل ممکن است وضعیت بررسی‌نشده، تأییدشده یا ردشده دیده شود، مگر فیلتر صریحی در سرویس اعمال شده باشد.

## ۳. پنل فروشگاه: Seller

### ۳.۱ endpointها

| Method | مسیر | ورودی | خروجی | رفتار |
|---|---|---|---|---|
| `GET` | `/api/Seller/StoreComment` | `StoreCommentInputDto` در query | `StoreCommentSearchDto` | `storeId` ارسالی نادیده و شناسهٔ فروشگاه توکن جایگزین می‌شود. |
| `GET` | `/api/Seller/StoreComment/{id}` | `id: long` | `BaseResultDto<StoreCommentDto>` | اگر نظر متعلق به فروشگاه توکن نباشد، HTTP `200` با `isSuccess=false` و AccessDenied می‌دهد. |
| `PUT` | `/api/Seller/StoreComment` | `StoreCommentDto` در JSON | `BaseResultDto` | فقط نظر متعلق به فروشگاه توکن به‌روز می‌شود. |

`StoreCommentInputDto` علاوه بر فیلدهای مشترک، `storeId?: long` و `allStatus?: bool` دارد. در این مسیر `storeId` همیشه از توکن تعیین می‌شود. **`allStatus` در سرویس StoreComment فیلتر وضعیت نیست**: فقط اگر دقیقاً `false` باشد مرتب‌سازی `statusId` سپس `id` اعمال می‌شود؛ اگر `true` یا null باشد مرتب‌سازی صریحی وجود ندارد. `available` و `q` نیز در این سرویس نادیده گرفته می‌شوند. برای UI، تب «در انتظار/تأیید/رد» با query این endpoint قابل اتکا نیست؛ از `list[].statusId` برای تفکیک سمت نمایش استفاده کنید و در صفحه‌بندی بدانید که این کار کل دادهٔ فروشگاه را فیلتر نمی‌کند.

`StoreCommentVDto` در هر عضو `list` همان فیلدهای `CommentVDto` به‌علاوهٔ `storeId` و `storeName` را دارد. `GET /{id}` مدل ساده‌تر `StoreCommentDto` را می‌دهد: فیلدهای `CommentDto` به‌علاوهٔ `storeId`؛ آبجکت‌های `storeName`, `status`, `user`, `createDate` را از پاسخ جزئیات انتظار نداشته باشید.

نمونهٔ `PUT` حداقلی:

```http
PUT /api/Seller/StoreComment
Content-Type: application/json
Authorization: Bearer <seller-token>

{ "id": 123, "statusId": 2, "answer": "از بازخورد شما سپاسگزاریم." }
```

سرور `storeId` را از توکن جایگزین می‌کند و در service فقط `statusId` و `answer` را روی رکورد تغییر می‌دهد؛ `text`, `rate`, `name`, `userId`, شمارنده‌ها و خود `storeId` با این عملیات قابل ویرایش نیستند. پس از ذخیره، محاسبهٔ مجدد آمار امتیاز/نظر فروشگاه فراخوانی می‌شود. اکشن `POST` یا `DELETE` در Area Seller وجود ندارد.

### ۳.۲ رابطه با نظر محصول و پرسش‌وپاسخ

یک فروشگاه می‌تواند محصول داشته باشد، ولی `ProductComment` به `ProductId` وصل است و کنترلر آن فقط در `EndUser` و `Admin` دیده می‌شود. در Seller endpoint فهرست/پاسخ/تأیید **نظر محصول** وجود ندارد. `DiscussionQuestion` نیز به `ProductId` وصل است اما کنترلر Seller ندارد؛ بنابراین صفحهٔ «پرسش‌وپاسخ محصول برای فروشنده» با API اختصاصی فعلی قابل پیاده‌سازی نیست. استفاده از مسیرهای Admin برای فروشنده مبنای این قرارداد نیست.

## ۴. پنل نماینده: Companion

### ۴.۱ نظر عمومی نماینده (`CompanionComment`)

| Method | مسیر | ورودی | خروجی | رفتار |
|---|---|---|---|---|
| `GET` | `/api/Companion/CompanionComment` | `CompanionCommentInputDto` در query | `CompanionCommentSearchDto` | `companionId` با شناسهٔ نمایندهٔ توکن جایگزین می‌شود. |
| `GET` | `/api/Companion/CompanionComment/{id}` | `id: long` | `BaseResultDto<CompanionCommentDto>` | کد فعلی پس از `FindAsyncDto` مالکیت رکورد را کنترل نمی‌کند. |
| `PUT` | `/api/Companion/CompanionComment` | `CompanionCommentDto` در JSON | `BaseResultDto` | service مالکیت نظر به نمایندهٔ توکن را کنترل می‌کند. |

`CompanionCommentInputDto`: فیلدهای مشترک + `companionId?: long`, `allStatus?: bool`. `CompanionCommentVDto`: فیلدهای `CommentVDto` + `companionId`. `CompanionCommentDto`: فیلدهای `CommentDto` + `companionId`. در list، `allStatus!=true` یعنی مرتب‌سازی بر اساس `statusId` سپس `id`؛ `allStatus=true` یعنی فقط `id` نزولی/صعودی با `sortBy`. **هیچ‌یک از `allStatus`، `available` و `q` فیلتر وضعیت/متن نیستند.**

نمونهٔ `PUT`:

```json
{ "id": 456, "statusId": 2, "answer": "پاسخ نماینده به نظر شما" }
```

سرور `companionId` را از توکن قرار می‌دهد. service فقط `statusId` و `answer` را تغییر می‌دهد و سپس با stored procedure آمار نظر/امتیاز نماینده را به‌روزرسانی می‌کند. `POST` و `DELETE` در Area Companion برای این منبع وجود ندارند. ثبت نظر عمومی توسط کاربر از `POST /api/EndUser/CompanionComment` انجام می‌شود؛ آن مسیر `userId` را از توکن می‌گیرد، `text` و `name` را لازم می‌داند، `rate` را اگر حاضر باشد ۱ تا ۵ می‌خواهد، `statusId` اولیه را «بررسی نشده» و `answer` را null می‌کند.

**نکتهٔ دسترسی قابل پیگیری:** `GET /{id}` فقط `[Authorize]` دارد و در همین action/service بررسی نمی‌شود که نظر متعلق به نمایندهٔ فعلی باشد. UI نباید از این رفتار برای خواندن شناسه‌های خارج از فهرست خودش استفاده کند؛ این کنترل باید در بک‌اند تکمیل شود.

### ۴.۲ نظرِ رزرو خدمت (`CompanionReserveComment`)

| Method | مسیر | ورودی | خروجی | رفتار |
|---|---|---|---|---|
| `GET` | `/api/Companion/CompanionReserveComment` | `CompanionReserveCommentInputDto` در query | `CompanionReserveCommentSearchDto` | `companionId` از توکن، `hideRejected=true` از سرور؛ فقط رزروهای همین نماینده. |
| `GET` | `/api/Companion/CompanionReserveComment/{id}` | `id: long` | `BaseResultDto<CompanionReserveCommentVDto>` | مالکیت از مسیر `comment → reserve → companionAssistance → companionId` کنترل می‌شود؛ غیرمالک/نبودن رکورد `404` می‌گیرد. |

در Area نماینده، `POST`, `PUT`, `DELETE` برای این منبع وجود ندارد. حتی اگر خروجی فیلد `answer` و `statusId` داشته باشد، اکشن پاسخ/تأیید نظر رزرو برای Companion تعریف نشده است؛ تغییر آن در API ادمین انجام می‌شود.

فیلدهای query اختصاصی: `companionReserveId?: long`, `companionId?: long` (از توکن جایگزین می‌شود)، `userId?: long`, `allStatus?: bool`, `hideRejected: bool` (همیشه در action برابر `true`). `available` با `allStatus=false` این حالت‌ها را دارد: null=بررسی‌نشده، true=تأیید، false=رد. چون `hideRejected=true` اجباری است، حالت «رد» عملاً خروجی خالی می‌دهد. با `allStatus=true` یا null، هر دو وضعیت بررسی‌نشده و تأییدشده می‌توانند دیده شوند؛ ردشده حذف می‌شود. sort بر اساس `statusId` و سپس `id` است (`sortBy=2` قدیمی به جدید، بقیه جدید به قدیم). `q` اعمال نمی‌شود.

`CompanionReserveCommentVDto`: فیلدهای `CommentVDto` + `companionReserveId`, `companionReserve`, `companionReserveCommentRates[]`. هر عضو rates شامل `id`, `rate`, `assistanceQuestionnaireId`, `companionReserveCommentId`, `assistanceQuestionnaire` است. در detail، سرویس رابطهٔ reserve، user و rate/questionnaire را صریحاً بارگذاری می‌کند؛ در query فهرست، rates صریحاً `Include` نشده‌اند، پس برای نمایش قطعی ریزامتیازها detail را بگیرید. `GET /{id}` **برخلاف کامنت کد** وضعیت ردشده را صریحاً فیلتر نمی‌کند؛ اگر `id` رکورد متعلق به نماینده معلوم باشد، detail ممکن است آن را برگرداند.

### ۴.۳ چرخهٔ ثبت نظر رزرو و پرسشنامه

این بخش برای فهم داده‌ای است که نماینده **می‌خواند**؛ ثبت نظر از پنل نماینده انجام نمی‌شود.

1. کاربر با `GET /api/EndUser/AssistanceQuestionnaire?assistanceId=<id>&available=true` پرسش‌های خدمت را می‌گیرد. `AssistanceQuestionnaireVDto`: `id`, `name` (متن/عنوان سؤال)، `priority`, `active`, `assistanceId`, `assistance`. query علاوه بر آن فیلدهای مشترک صفحه‌بندی/مرتب‌سازی را دارد؛ `sortBy=0` یا `6` اولویت بیشتر، `7` اولویت کمتر است. service رکوردهای deleted را حذف و `available=true` را به `active=true` تبدیل می‌کند.
2. کاربر `POST /api/EndUser/CompanionReserveComment` را با `companionReserveId`, `text` و `companionReserveCommentRates: [{assistanceQuestionnaireId, rate}]` می‌فرستد. DTO پایه فیلدهای دیگر نظر را هم دارد، اما هویت/وضعیت اولیه/پاسخ از سرور تعیین می‌شود؛ `rate` اصلی از میانگین rateهای سؤال‌ها محاسبه می‌شود.
3. سرور فقط برای رزروی اجازه می‌دهد که `bookerId` آن کاربر فعلی باشد، رزرو شده و لغو نشده باشد، و یا وضعیت اپراتوری آن `Complete` باشد یا خدمت آنلاینِ دارای `callEndDate` باشد. برای هر رزرو حداکثر یک نظر پذیرفته می‌شود.
4. باید دقیقاً به همهٔ سؤال‌های حذف‌نشدهٔ همان خدمت، هر کدام یک بار، امتیاز ۱ تا ۵ داده شود و دست‌کم یک سؤال وجود داشته باشد. در غیر این صورت پاسخ ناموفق `PleaseAnswerTheQuestions` است. امتیاز کلی از میانگین این مقادیر به `int` تبدیل می‌شود؛ UI نباید آن را مستقل محاسبه و ارسال کند.
5. نظر تازه `Comment_NotChecked` و `answer=null` می‌گیرد. پس از ثبت، نماینده آن را در فهرست خودش می‌بیند؛ تأیید/رد/پاسخ آن در API ادمین است.

نمونهٔ دادهٔ نظر رزرو که **کاربر** ارسال می‌کند:

```json
{
  "companionReserveId": 789,
  "text": "کیفیت خدمت خوب بود",
  "companionReserveCommentRates": [
    { "assistanceQuestionnaireId": 10, "rate": 5 },
    { "assistanceQuestionnaireId": 11, "rate": 4 }
  ]
}
```

### ۴.۴ موارد مجاور در Companion

- `PansionComment` نظر پانسیون است، ولی کنترلر مستقلی در Area Companion ندارد؛ فقط `EndUser` و `Admin` برای آن کنترلر دارند. دادهٔ `pansionComments` ممکن است در مدل جزئیات پانسیون بیاید؛ آن را با endpoint مدیریت نظر نماینده یکی نگیرید.
- در مدرسهٔ زنده، `GET /api/Companion/SchoolCourseLive/{liveSessionId}/comments` فهرست `SchoolLiveCommentDto[]` را در `BaseResultDto` می‌دهد. فیلدها: `id`, `schoolCourseLiveSessionId`, `userId`, `message`, `createDate`, `userName`, `userPicture`. service فقط مربی صاحب دوره یا کاربر ثبت‌نام‌شده را مجاز می‌داند. ارسال پیام از SignalR هاب `/hubs/schoolLive` با `PostComment(liveSessionId, message)` است؛ `newComment` منتشر می‌شود، متن خالی رد و متن بیش از ۵۰۰ کاراکتر کوتاه می‌شود. این جریان چت لایو است، نه نظر امتیازی یا پرسش‌وپاسخ محصول.

## ۵. پنل اپراتور: Operator

در Area `Operator` هیچ `Comment`, `Review`, `DiscussionQuestion` یا `DiscussionAnswer` controller/action وجود ندارد. کنترلرهای موجود: `CompanionReserve`, `CompanionReserveOperatorUpdate`, `UserPetRecord`. اپراتور ممکن است رزروی را تکمیل کند و این تکمیل یکی از پیش‌شرط‌های ثبت نظر رزرو توسط **کاربر** شود؛ با این حال endpoint خواندن/پاسخ‌دادن/تأیید نظرات برای پنل Operator تعریف نشده است. فیلدهای نظر که شاید در مدل رزرو یا گزارش دیده شوند، مجوز عملیات نظر نیستند.

## ۶. پنل راننده: Driver

در Area `Driver` نیز هیچ controller/action برای نظر، امتیاز، پرسش یا پاسخ وجود ندارد. `GET /api/Driver/Code` فقط دیکشنری عمومی کدها است و از وجود آن نمی‌توان قابلیت مدیریت نظر استنباط کرد. سفرهای Pet-resan در این API رابطهٔ endpointی با `StoreComment`, `CompanionComment`, `CompanionReserveComment` یا `DiscussionQuestion` ندارند. برای صفحهٔ نظرات/پرسش‌وپاسخ راننده در قرارداد فعلی، endpoint و DTO اختصاصی وجود ندارد.

## ۷. پرسش‌وپاسخ محصول: ارتباط با پنل‌ها و API موجود

`DiscussionQuestion` به یک `productId` وصل است؛ هر `DiscussionAnswer` به `discussionQuestionId` وصل می‌شود. این مسیر **مستقل از نظر/امتیاز** است. برای شفاف‌شدن مرز Seller/دیگر پنل‌ها، قرارداد فعلی آن چنین است:

| نقش API | endpointهای موجود | توضیح |
|---|---|---|
| عمومی | `GET /api/DiscussionQuestion` | `available=true` اجباری؛ فقط سؤال‌های `active=true` و `adminConfirm=true` در فهرست. |
| EndUser | `GET /api/EndUser/DiscussionQuestion`, `GET .../{id}`, `POST /api/EndUser/DiscussionQuestion`; `GET/POST /api/EndUser/DiscussionAnswer`; `POST /api/EndUser/DiscussionAnswerLike` | ساخت سؤال/پاسخ با `userId` توکن؛ پاسخ تازه `active=false` است. |
| Admin | `GET/POST/PUT/DELETE /api/Admin/DiscussionQuestion`; `GET/POST /api/Admin/DiscussionAnswer`; `PUT /api/Admin/DiscussionAnswerActive` | تأیید سؤال و فعال‌سازی پاسخ از این Area است. |
| Seller/Companion/Operator/Driver | **هیچ endpoint اختصاصی** | اتصال پرسش‌ها به محصول به‌تنهایی دسترسی مدیریت پنلی ایجاد نمی‌کند. |

فیلدهای `DiscussionQuestionDto`: `id`, `content`, `createDate`, `productId`, `userId`, `adminConfirm?: bool`, `active: bool`, `answerCount: int`. خروجی فهرست `DiscussionQuestionVDto` این‌ها را به‌علاوهٔ `product`, `user`, `discussionAnswers[]` دارد. query سؤال: `productId?`, `adminConfirm?`, `active?`, `fromDate?`, `toDate?` و فیلدهای مشترک صفحه‌بندی؛ `sortBy=2` قدیمی به جدید و بقیه جدید به قدیم. در جستجوی سؤال، `q` اعمال نمی‌شود. پاسخ `DiscussionAnswerDto`: `id`, `content`, `discussionQuestionId`, `userId`, `active`; خروجی `VDto` علاوه بر این‌ها `createDate`, `likeCount`, `disLikeCount`, `userIsLike`, `user` دارد. query پاسخ: `discussionQuestionId?`, `userId?` و فیلدهای مشترک؛ `available=true` فقط پاسخ‌های active را برمی‌گرداند. فعال‌سازی پاسخ توسط Admin با `{ "id": 12, "active": true }` انجام و `answerCount` سؤال دوباره محاسبه می‌شود.

**تفاوت مهم:** در کد فعلی `GET /api/EndUser/DiscussionQuestion` برخلاف مسیر عمومی، `available=true` را اجباری نمی‌کند و `GET /{id}` از `FindAsyncDto` استفاده می‌کند؛ نباید رفتار فیلتر مسیر عمومی را به این دو تعمیم داد. فهرست پاسخ EndUser `available=true` را تحمیل می‌کند. این مسیرها از نظر نام در Area کاربرند و جایگزین مدیریت Seller نیستند.

## ۸. نکات پیاده‌سازی و ناسازگاری‌های مشاهده‌شده

1. **Seller و CompanionComment:** پارامتر `allStatus` در دو سرویس نظر عمومی فقط ترتیب را عوض می‌کند، نه مجموعهٔ وضعیت‌ها را. `available` نادیده گرفته می‌شود. حتی endpoint عمومی `GET /api/EndUser/StoreComment` و `GET /api/EndUser/CompanionComment` که `available=true` می‌گذارند، به‌واسطهٔ همین سرویس‌ها فیلتر تأییدشده را واقعاً اجرا نمی‌کنند. این مسئله برای انتشار نظر نیاز به اصلاح بک‌اند دارد.
2. **CompanionComment detail:** مالکیت در `GET /{id}` بررسی نشده است؛ در list و PUT بررسی می‌شود. این ناهماهنگی باید سمت سرور اصلاح شود.
3. **CompanionReserveComment detail:** کامنت کد وعدهٔ پنهان‌بودن نظر ردشده را می‌دهد، اما شرط detail فقط مالکیت رزرو را می‌سنجد. list نظر ردشده را پنهان می‌کند.
4. **جستجو و شمارش:** فیلترسازی سمت UI روی `list` فقط همان صفحه را پوشش می‌دهد و `totalCount` همچنان کل نتیجهٔ سرور است. برای تب‌های سروری دقیق یا جستجوی متن، endpoint/سرویس باید توسعه یابد.
5. **نبود قابلیت:** برای Seller نظر محصول/پرسش‌وپاسخ محصول، برای Companion پاسخ به نظر رزرو و نظر پانسیون، و برای Operator/Driver تمام عملیات نظر و پرسش‌وپاسخ، endpoint پنلی فعلی وجود ندارد. UI نباید برای این عملیات مسیر حدسی بسازد.
6. **شناسهٔ پنلِ خالی:** `GET` فهرست Seller و `CompanionComment` شناسهٔ فروشگاه/نماینده را از توکن می‌گذارند، اما سرویس فقط وقتی شناسه مقدار داشته باشد شرط مالکیت را به query اضافه می‌کند. چون کنترلرها تنها `[Authorize]` دارند، توکن معتبری که `storeId` یا `companionId` نداشته باشد می‌تواند فهرست بدون scope بگیرد. این وضعیت نیاز به کنترل صریح سمت بک‌اند دارد؛ فرانت فقط در پنلِ دارای شناسهٔ معتبر این فهرست‌ها را فراخوانی کند. `CompanionReserveComment` برای شناسهٔ نمایندهٔ خالی `404` می‌دهد.

## ۹. منابع اصلی در مخزن

- کنترلرهای پنلی: [`Seller/StoreCommentController.cs`](../Api/Areas/Seller/Controllers/StoreCommentController.cs)، [`Companion/CompanionCommentController.cs`](../Api/Areas/Companion/Controllers/CompanionCommentController.cs)، [`Companion/CompanionReserveCommentController.cs`](../Api/Areas/Companion/Controllers/CompanionReserveCommentController.cs).
- سرویس‌های نظر: [`StoreCommentService.cs`](../Application/Services/ProductSrvs/StoreCommentSrv/StoreCommentService.cs)، [`CompanionCommentService.cs`](../Application/Services/CompanionSrvs/CompanionCommentSrv/CompanionCommentService.cs)، [`CompanionReserveCommentService.cs`](../Application/Services/CompanionSrvs/CompanionReserveCommentSrv/CompanionReserveCommentService.cs).
- DTO مشترک: [`CommentDto.cs`](../Application/Services/CommonSrv/CommentSrv/Dto/CommentDto.cs)، [`CommentVDto.cs`](../Application/Services/CommonSrv/CommentSrv/Dto/CommentVDto.cs)، [`BaseInputDto.cs`](../Application/Common/Dto/Input/BaseInputDto.cs)، [`BaseSearchDto.cs`](../Application/Common/Dto/Result/BaseSearchDto.cs).
- پرسش‌وپاسخ: [`DiscussionQuestionService.cs`](../Application/Services/Content/DiscussionQuestionSrv/DiscussionQuestionService.cs)، [`DiscussionAnswerService.cs`](../Application/Services/Content/DiscussionAnswerSrv/DiscussionAnswerService.cs).
- پرسشنامه/لایو: [`AssistanceQuestionnaireService.cs`](../Application/Services/CompanionSrvs/AssistanceQuestionnaireSrv/AssistanceQuestionnaireService.cs)، [`SchoolLiveHub.cs`](../Api/Hubs/SchoolLiveHub.cs).
