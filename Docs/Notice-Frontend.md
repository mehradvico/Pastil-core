# مستند اتصال Notice به پنل ادمین

این مستند قرارداد فعلی بک‌اند برای لیست اعلان‌ها، خواندن تکی و گروهی، Toast لحظه‌ای و Web Push را توضیح می‌دهد.

## قواعد اصلی

- تمام APIهای Notice فقط برای کاربر دارای نقش Admin قابل استفاده‌اند.
- توکن باید با هدر `Authorization: Bearer <token>` ارسال شود.
- زمان‌ها UTC و با فرمت ISO 8601 هستند.
- اعلان‌ها بعد از هفت روز به‌صورت خودکار آرشیو می‌شوند.
- `Normal` فقط در لیست پنل نمایش داده می‌شود.
- `Important` علاوه بر لیست، از SignalR برای Toast ارسال می‌شود.
- `Critical` علاوه بر لیست و Toast، به‌صورت Web Push نیز ارسال می‌شود.
- خواندن اعلان سراسری است؛ اولین ادمینی که اعلان را می‌خواند به‌عنوان خواننده ثبت می‌شود.

## Enumها

```ts
export enum NoticeImportance {
  Normal = 1,
  Important = 2,
  Critical = 3,
}

export enum NoticeReadMode {
  Single = 1,
  BulkConfirmed = 2,
}

export enum SortEnum {
  New = 1,
  Old = 2,
}
```

## TypeScript typeها

```ts
export interface ApiMessage {
  item1: string;
  item2: string;
}

export interface ApiResult<T> {
  isSuccess: boolean;
  code: number;
  messages: ApiMessage[];
  data: T;
}

export interface NoticeType {
  id: number;
  label: string;
  name: string;
  title: string;
  importance: NoticeImportance;
  navigationTemplate: string;
  isActive: boolean;
  showToast: boolean;
  sendPush: boolean;
}

export interface NoticeRead {
  id: number;
  noticeId: number;
  adminId: number;
  adminNameSnapshot: string;
  readAtUtc: string;
  readMode: NoticeReadMode;
}

export interface NoticeActor {
  id: number;
  mobile?: string;
  email?: string;
  firstName?: string;
  lastName?: string;
  fullName?: string;
  pictureId?: number;
}

export interface Notice {
  id: number;
  noticeTypeId: number;
  actorUserId?: number | null;
  referenceType?: string | null;
  referenceId?: number | null;
  title: string;
  message: string;
  navigationUrl: string;
  metadata: Record<string, string>;
  createDateUtc: string;
  archiveDueAtUtc: string;
  archivedAtUtc?: string | null;
  isRead: boolean;
  noticeType: NoticeType;
  actorUser?: NoticeActor | null;
  read?: NoticeRead | null;
}

export interface NoticeSearchResult {
  pageIndex: number;
  pageSize: number;
  totalCount: number;
  list: Notice[];
}

export interface NoticeBulkReadResult {
  requestedCount: number;
  readCount: number;
  alreadyReadCount: number;
  notFoundCount: number;
  adminName: string;
}
```

## دریافت لیست و جستجو

```http
GET /api/Admin/Notice
```

پارامترهای Query:

| پارامتر | نوع | توضیح |
|---|---|---|
| `PageIndex` | number | شماره صفحه، پیش‌فرض 1 |
| `PageSize` | number | تعداد رکورد، پیش‌فرض 20 |
| `Q` | string | جستجو در عنوان، متن، Label، Metadata، نام و موبایل کاربر و نام ادمین خواننده |
| `SortBy` | number | `1` جدیدترین و `2` قدیمی‌ترین |
| `ActorUserId` | number | فیلتر کاربری که رویداد را ایجاد کرده |
| `ReadByAdminId` | number | فیلتر ادمین خواننده |
| `NoticeTypeId` | number | فیلتر نوع اعلان |
| `Importance` | number | `1` Normal، `2` Important، `3` Critical |
| `ReferenceType` | string | نوع رکورد مقصد، مانند ProductOrder |
| `ReferenceId` | number | شناسه رکورد مقصد |
| `IsRead` | boolean | خوانده‌شده یا خوانده‌نشده |
| `IsArchived` | boolean | آرشیوی یا فعال؛ در صورت ارسال‌نشدن مقدار پیش‌فرض `false` است |
| `FromDateUtc` | string | شروع بازه تاریخ UTC |
| `ToDateUtc` | string | پایان بازه تاریخ UTC |

نمونه:

```http
GET /api/Admin/Notice?PageIndex=1&PageSize=20&Importance=3&IsRead=false&IsArchived=false&SortBy=1
```

پاسخ مستقیماً `NoticeSearchResult` است و داخل `ApiResult` قرار ندارد.

## دریافت جزئیات

```http
GET /api/Admin/Notice/{id}
```

خروجی:

```ts
ApiResult<Notice>
```

این endpoint فقط اطلاعات را می‌خواند و وضعیت اعلان را تغییر نمی‌دهد.

## خواندن تکی

```http
POST /api/Admin/Notice/{id}/read
```

Body ندارد و خروجی `ApiResult<Notice>` است. اگر اعلان قبلاً توسط ادمین دیگری خوانده شده باشد، اطلاعات همان اولین ادمین در فیلد `read` باقی می‌ماند.

روال پیشنهادی هنگام کلیک:

```ts
async function openNotice(notice: Notice) {
  await api.post(`/api/Admin/Notice/${notice.id}/read`);
  navigate(notice.navigationUrl);
}
```

`navigationUrl` مسیر آماده پنل است و لازم نیست فرانت با `referenceType` مسیر را حدس بزند.

## خواندن گروهی

```http
POST /api/Admin/Notice/read/bulk
Content-Type: application/json
```

خواندن موارد انتخاب‌شده:

```json
{
  "noticeIds": [10, 11, 12],
  "all": false,
  "confirmed": true
}
```

خواندن تمام اعلان‌های فعال:

```json
{
  "noticeIds": [],
  "all": true,
  "confirmed": true
}
```

قبل از ارسال باید Modal تأیید به کاربر نمایش داده شود. اگر `confirmed` برابر `false` باشد عملیات انجام نمی‌شود.

خروجی:

```ts
ApiResult<NoticeBulkReadResult>
```

## تعداد خوانده‌نشده‌ها

```http
GET /api/Admin/Notice/unread-count
```

خروجی یک عدد ساده است:

```json
7
```

فقط اعلان‌های فعال و آرشیونشده شمرده می‌شوند.

## دریافت نوع‌های اعلان

```http
GET /api/Admin/Notice/types?activeOnly=true
```

خروجی مستقیماً `NoticeType[]` است. برای فیلتر UI بهتر است `activeOnly=true` استفاده شود.

## SignalR و Toastify

آدرس Hub:

```text
{API_BASE_URL}/hubs/notices
```

نام Event:

```text
noticeCreated
```

نصب پکیج:

```bash
npm install @microsoft/signalr react-toastify
```

نمونه اتصال:

```ts
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { toast } from "react-toastify";

export function createNoticeConnection(apiBaseUrl: string, getToken: () => string, readNotice: (id: number) => Promise<void>, navigate: (url: string) => void) {
  const connection = new HubConnectionBuilder()
    .withUrl(`${apiBaseUrl}/hubs/notices`, { accessTokenFactory: getToken })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();

  connection.on("noticeCreated", (notice: Notice) => {
    toast(notice.message, {
      toastId: `notice-${notice.id}`,
      type: notice.noticeType.importance === NoticeImportance.Critical ? "error" : "info",
      onClick: async () => {
        await readNotice(notice.id);
        navigate(notice.navigationUrl);
      },
    });
  });

  return connection;
}
```

اتصال باید بعد از ورود ادمین Start و هنگام Logout متوقف شود:

```ts
const connection = createNoticeConnection(API_BASE_URL, () => accessToken, id => api.post(`/api/Admin/Notice/${id}/read`), navigate);
await connection.start();

// logout / unmount
await connection.stop();
```

بعد از reconnect بهتر است `unread-count` و صفحه اول لیست دوباره دریافت شوند، چون ممکن است هنگام قطع ارتباط Eventی از دست رفته باشد.

## فعال‌کردن Web Push برای Critical

### 1. دریافت Public Key

```http
GET /api/EndUser/push/public-key
```

```json
{
  "publicKey": "..."
}
```

### 2. ساخت Subscription

یک `deviceKey` از نوع UUID بسازید و در LocalStorage نگه دارید. سپس با Service Worker و VAPID Public Key یک PushSubscription مرورگر ایجاد کنید.

### 3. ثبت Subscription

```http
POST /api/EndUser/push/subscribe
Content-Type: application/json
```

```json
{
  "deviceKey": "4f0794d1-80d5-47fa-8264-020adcc5043d",
  "endpoint": "https://push-service.example/...",
  "keys": {
    "p256dh": "...",
    "auth": "..."
  },
  "userAgent": "Mozilla/5.0 ..."
}
```

### 4. اتصال Subscription به ادمین واردشده

```http
POST /api/EndUser/push/attach
Authorization: Bearer <token>
Content-Type: application/json
```

```json
{
  "deviceKey": "4f0794d1-80d5-47fa-8264-020adcc5043d"
}
```

Payload ارسالی به Service Worker:

```ts
interface NoticePushPayload {
  title: string;
  body: string;
  url: string;
  icon?: string;
  tag: string;
}
```

نمونه Service Worker:

```js
self.addEventListener("push", event => {
  const data = event.data?.json();
  event.waitUntil(self.registration.showNotification(data.title, {
    body: data.body,
    icon: data.icon,
    tag: data.tag,
    data: { url: data.url },
  }));
});

self.addEventListener("notificationclick", event => {
  event.notification.close();
  const url = event.notification.data?.url || "/";
  event.waitUntil(clients.openWindow(url));
});
```

## نکات UI

- برای جلوگیری از Toast تکراری از `toastId: notice-{id}` استفاده شود.
- رنگ پیشنهادی: Normal خاکستری، Important نارنجی، Critical قرمز.
- نام اولین ادمین خواننده از `read.adminNameSnapshot` نمایش داده شود.
- برای اعلان خوانده‌نشده از `isRead === false` استفاده شود.
- `metadata` برای نمایش اطلاعات کمکی است؛ مسیریابی فقط با `navigationUrl` انجام شود.
- در صفحه آرشیو `IsArchived=true` ارسال شود.
- خطای 401 یعنی توکن معتبر نیست و خطای 403 یعنی کاربر ادمین نیست.
