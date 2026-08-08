# قرارداد استفاده از CodeGroup و Code در پنل

## هدف

پنل نباید عنوان نمایشی یا شناسه Codeها را Hardcode کند.

قاعده اصلی:

- `CodeGroup.Label` کلید ثابت فنی برای دریافت یک گروه از Codeها است.
- `Code.Label` کلید ثابت فنی برای شرط‌ها و منطق پنل است.
- `Code.Name` متن نمایشی است و همیشه باید از API خوانده شود.
- `Code.Id` مقداری است که APIهای دامنه در فیلدهایی مانند `statusId` دریافت می‌کنند.

با رعایت این قرارداد، اگر مدیر نام یک Code را تغییر دهد، بعد از دریافت مجدد اطلاعات از API، عنوان جدید در تمام Dropdownها، فیلترها، Badgeها و جدول‌های پنل نمایش داده می‌شود.

## تعریف فیلدها

### CodeGroup

| فیلد | کاربرد |
|---|---|
| `id` | شناسه دیتابیسی گروه |
| `label` | کلید فنی ثابت برای درخواست Codeهای گروه |
| `name` | عنوان نمایشی گروه |

### Code

| فیلد | کاربرد |
|---|---|
| `id` | مقدار ارسالی به API در فیلدهایی مانند `statusId` |
| `label` | کلید فنی ثابت برای شرط‌ها |
| `name` | عنوان نمایشی قابل‌تغییر |
| `value` | مقدار جانبی Code |
| `codeGroupId` | شناسه گروه |
| `priority` | ترتیب نمایش |
| `active` | وضعیت فعال Code |

## قانون ثابت پنل

| نیاز | مقدار مورد استفاده |
|---|---|
| دریافت Codeهای یک گروه | `CodeGroup.Label` |
| نمایش عنوان به کاربر | `Code.Name` دریافتی از API |
| شرط‌گذاری در کد فرانت | `Code.Label` |
| ارسال مقدار فرم به بک‌اند | `Code.Id` |

موارد زیر ممنوع هستند:

```ts
// اشتباه: نام نمایشی Hardcode شده است.
const statuses = [
  { id: 49, name: "در انتظار پاسخ" },
  { id: 50, name: "پاسخ داده شده" },
];

// اشتباه: منطق به متن فارسی وابسته شده است.
if (ticket.statusName === "در انتظار پاسخ") {
  // ...
}

// اشتباه: منطق به ID دیتابیس وابسته شده است.
if (ticket.statusId === 49) {
  // ...
}
```

الگوی صحیح:

```ts
export const CODE_GROUP_LABELS = {
  ticketStatus: "Ticket_Status",
  productStatus: "ProductStatus",
  paymentType: "PaymentType",
} as const;

export const CODE_LABELS = {
  ticketWaiting: "TicketStatus_Waiting",
  ticketAnswered: "TicketStatus_Answered",
  ticketClosed: "TicketStatus_Close",
} as const;
```

ثابت‌بودن Label در فرانت مجاز است، چون Label قرارداد فنی است. `id` و `name` نباید ثابت نوشته شوند.

## دریافت Codeها براساس Label گروه

```http
GET /api/Admin/Code?codeGroupLabel=Ticket_Status&available=true&pageIndex=1&pageSize=100
Authorization: Bearer {adminAccessToken}
```

نمونه پاسخ:

```json
{
  "pageIndex": 1,
  "pageSize": 100,
  "totalCount": 3,
  "list": [
    {
      "id": 49,
      "name": "در انتظار پاسخ",
      "label": "TicketStatus_Waiting",
      "value": "TicketStatus_Waiting",
      "codeGroupId": 13,
      "priority": 1,
      "active": true
    }
  ]
}
```

گزینه‌های خروجی API براساس `priority` مرتب می‌شوند.

## سرویس مشترک پیشنهادی

دریافت Codeها باید در یک سرویس مشترک انجام شود تا تمام صفحات پنل از یک منبع استفاده کنند.

```ts
export type CodeOption = {
  id: number;
  name: string;
  label: string;
  value: string;
  codeGroupId: number;
  priority: number;
  active: boolean;
};

export type CodeSearchResponse = {
  totalCount: number;
  list: CodeOption[];
};

export async function getCodesByGroupLabel(
  codeGroupLabel: string,
  onlyActive = true,
): Promise<CodeOption[]> {
  const query = new URLSearchParams({
    codeGroupLabel,
    available: String(onlyActive),
    pageIndex: "1",
    pageSize: "100",
  });

  const response = await api.get<CodeSearchResponse>(
    `/api/Admin/Code?${query.toString()}`,
  );

  return response.data.list;
}
```

استفاده:

```ts
const ticketStatuses = await getCodesByGroupLabel(
  CODE_GROUP_LABELS.ticketStatus,
);
```

## استفاده در Dropdown

عنوان گزینه از `name` و مقدار آن از `id` گرفته شود:

```tsx
<select
  value={form.statusId ?? ""}
  onChange={(event) =>
    setForm({
      ...form,
      statusId: Number(event.target.value),
    })
  }
>
  <option value="">انتخاب کنید</option>

  {ticketStatuses.map((code) => (
    <option key={code.label} value={code.id}>
      {code.name}
    </option>
  ))}
</select>
```

نمونه Payload:

```json
{
  "statusId": 49
}
```

## استفاده در جدول و Badge

اگر API دامنه فقط `statusId` برمی‌گرداند، نام نمایشی باید از لیست Codeهای دریافت‌شده پیدا شود:

```ts
const codeById = new Map(
  ticketStatuses.map((code) => [code.id, code]),
);

function getStatusName(statusId: number): string {
  return codeById.get(statusId)?.name ?? "نامشخص";
}
```

```tsx
<Badge>{getStatusName(ticket.statusId)}</Badge>
```

اگر API دامنه آبجکت Code را برمی‌گرداند، مستقیماً `name` همان آبجکت نمایش داده شود:

```tsx
<Badge>{ticket.status?.name ?? "نامشخص"}</Badge>
```

هیچ‌وقت عنوان Badge با شرط روی ID یا متن فارسی تعیین نشود.

## شرط‌گذاری با Code.Label

برای منطق UI ابتدا Code انتخاب‌شده را از روی ID پیدا کنید، سپس Label آن را بررسی کنید:

```ts
const selectedStatus = codeById.get(ticket.statusId);

const canAnswer =
  selectedStatus?.label === CODE_LABELS.ticketWaiting;
```

این روش باعث می‌شود تغییر `name` روی منطق پنل اثر نگذارد.

در مواردی که یک Code مشخص لازم است، ID آن را در زمان اجرا از روی Label پیدا کنید:

```ts
const waitingCode = ticketStatuses.find(
  (code) => code.label === CODE_LABELS.ticketWaiting,
);

if (!waitingCode) {
  throw new Error("TicketStatus_Waiting was not returned by API.");
}

const payload = {
  statusId: waitingCode.id,
};
```

## Cache و بروزرسانی نام‌ها

Codeها را می‌توان Cache کرد، اما Cache نباید دائمی باشد.

قواعد پیشنهادی:

1. Codeها بعد از ورود به پنل یا هنگام بازشدن صفحه موردنیاز دریافت شوند.
2. برای Cache مدت‌دار، زمان کوتاه مانند ۵ تا ۱۵ دقیقه در نظر گرفته شود.
3. بعد از `POST`، `PUT` یا `DELETE` روی Code یا CodeGroup، Cache مربوط به Codeها پاک شود.
4. بعد از ویرایش `Code.Name`، Query گروه مربوطه دوباره Fetch شود.
5. هنگام خروج مدیر از حساب، Cacheهای Code پاک شوند.

نمونه با Query Client:

```ts
await api.put("/api/Admin/Code", form);

await queryClient.invalidateQueries({
  queryKey: ["codes", codeGroupLabel],
});
```

اگر نام Code در پنل ویرایش شد ولی Cache پاک نشود، صفحات دیگر تا زمان Fetch بعدی ممکن است نام قبلی را نمایش دهند.

## مدیریت Code و CodeGroup

### ویرایش Code

```http
PUT /api/Admin/Code
Authorization: Bearer {adminAccessToken}
Content-Type: application/json
```

```json
{
  "id": 49,
  "name": "منتظر پاسخ کارشناس",
  "label": "TicketStatus_Waiting",
  "value": "TicketStatus_Waiting",
  "codeGroupId": 13,
  "priority": 1,
  "active": true
}
```

در این مثال فقط `name` تغییر کرده است. `label` باید ثابت باقی بماند.

### ویرایش CodeGroup

```http
PUT /api/Admin/CodeGroup
Authorization: Bearer {adminAccessToken}
Content-Type: application/json
```

```json
{
  "id": 13,
  "name": "وضعیت پاسخ‌گویی تیکت",
  "label": "Ticket_Status"
}
```

در CodeGroup نیز فقط `name` قابل تغییر است و `label` قرارداد فنی باقی می‌ماند.

## رفتار فرم مدیریت

در صفحه ویرایش Code و CodeGroup:

- فیلد `name` قابل ویرایش باشد.
- فیلد `label` بعد از ایجاد، Readonly نمایش داده شود.
- تغییر Label فقط با هماهنگی بک‌اند و فرانت انجام شود.
- در لیست‌ها عنوان اصلی از `name` نمایش داده شود.
- `label` می‌تواند به‌عنوان اطلاعات فنی در ستون جداگانه نمایش داده شود.
- Code غیرفعال در فرم ایجاد داده جدید نمایش داده نشود.
- Code غیرفعال مربوط به رکورد قدیمی همچنان با نام خودش قابل نمایش باشد.

## مدیریت Code غیرفعال در فرم ویرایش

ممکن است رکورد قدیمی از Codeای استفاده کند که اکنون `active=false` است. برای اینکه مقدار فرم خالی نشود:

1. Codeهای فعال گروه را دریافت کنید.
2. اگر ID فعلی بین Codeهای فعال نبود، جزئیات آن را جداگانه دریافت کنید.
3. Code غیرفعال فعلی را فقط برای نمایش به گزینه‌ها اضافه کنید.

```http
GET /api/Admin/Code/{id}
Authorization: Bearer {adminAccessToken}
```

## چک‌لیست نهایی پنل

- [ ] Codeها با `CodeGroup.Label` دریافت می‌شوند.
- [ ] عنوان‌ها فقط از `Code.Name` API نمایش داده می‌شوند.
- [ ] فرم‌ها `Code.Id` را برای بک‌اند ارسال می‌کنند.
- [ ] شرط‌های UI با `Code.Label` نوشته شده‌اند.
- [ ] هیچ ID یا نام فارسی در کد فرانت Hardcode نشده است.
- [ ] Label بعد از ایجاد در فرم Readonly است.
- [ ] بعد از ویرایش Code، Cache گروه پاک و اطلاعات دوباره دریافت می‌شود.
- [ ] Codeهای غیرفعال در فرم ایجاد پنهان هستند.
- [ ] Code غیرفعال رکورد قدیمی همچنان قابل نمایش است.
