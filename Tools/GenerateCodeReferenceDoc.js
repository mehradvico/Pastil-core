const fs = require("fs");
const path = require("path");

const codeGroupsSqlPath = process.argv[2];
const codesSqlPath = process.argv[3];
const outputPath = process.argv[4];

if (!codeGroupsSqlPath || !codesSqlPath || !outputPath) {
    throw new Error(
        "Usage: node GenerateCodeReferenceDoc.js <CodeGroups.sql> <Codes.sql> <output.md>"
    );
}

const codeGroupsSql = fs.readFileSync(codeGroupsSqlPath, "utf8");
const codesSql = fs.readFileSync(codesSqlPath, "utf8");

const codeGroups = [...codeGroupsSql.matchAll(
    /^\s*\((\d+),\s*N'([^']+)',\s*N'([^']+)'\)[,;]?$/gm
)].map(match => ({
    id: Number(match[1]),
    label: match[2],
    name: match[3]
}));

const codes = [...codesSql.matchAll(
    /^\s*\((\d+),\s*N'([^']+)',\s*N'([^']+)',\s*(\d+),\s*(\d+)\)[,;]?$/gm
)].map(match => ({
    id: Number(match[1]),
    label: match[2],
    name: match[3],
    codeGroupId: Number(match[4]),
    priority: Number(match[5])
}));

if (codeGroups.length !== 34) {
    throw new Error(`Expected 34 CodeGroups, received ${codeGroups.length}.`);
}

if (codes.length !== 137) {
    throw new Error(`Expected 137 Codes, received ${codes.length}.`);
}

const groupById = new Map(codeGroups.map(group => [group.id, group]));
for (const code of codes) {
    if (!groupById.has(code.codeGroupId)) {
        throw new Error(`Code ${code.label} references missing CodeGroup ${code.codeGroupId}.`);
    }
}

function tableCell(value) {
    return String(value).replaceAll("|", "\\|").replaceAll("\n", " ");
}

const groupRows = codeGroups
    .map(group => {
        const count = codes.filter(code => code.codeGroupId === group.id).length;
        return `| ${group.id} | \`${tableCell(group.label)}\` | ${tableCell(group.name)} | ${count} |`;
    })
    .join("\n");

const codeSections = codeGroups.map(group => {
    const groupCodes = codes
        .filter(code => code.codeGroupId === group.id)
        .sort((a, b) => a.priority - b.priority || a.id - b.id);

    const rows = groupCodes
        .map(code =>
            `| ${code.id} | \`${tableCell(code.label)}\` | ${tableCell(code.name)} | ${code.priority} |`
        )
        .join("\n");

    return `### ${group.id}. ${group.name}

- CodeGroup Label: \`${group.label}\`
- CodeGroupId: \`${group.id}\`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
${rows}`;
}).join("\n\n");

const markdown = `# مستند CodeGroup و Code پاستیل

این سند مرجع مشترک تیم فرانت سایت و پنل ادمین است و با Seed فعلی دیتابیس هماهنگ شده است:

- تعداد CodeGroupها: **۳۴**
- تعداد Codeها: **۱۳۷**
- بازه ID گروه‌ها: \`1..34\`
- بازه ID کدها: \`1..137\`

## مفهوم داده‌ها

### CodeGroup

هر CodeGroup یک خانواده از مقادیر ثابت سیستم است؛ برای مثال وضعیت تیکت، نوع پرداخت یا وضعیت رزرو.

| فیلد | نوع | توضیح |
|---|---|---|
| \`id\` | number | شناسه ثابت گروه |
| \`label\` | string | کلید فنی و پایدار گروه |
| \`name\` | string | عنوان نمایشی فارسی |

### Code

| فیلد | نوع | توضیح |
|---|---|---|
| \`id\` | number | شناسه ثابت کد؛ معمولاً همین مقدار در DTOهای دامنه ارسال می‌شود |
| \`label\` | string | کلید فنی و پایدار؛ مناسب شرط‌ها و enumهای فرانت |
| \`name\` | string | عنوان قابل نمایش به کاربر |
| \`value\` | string | در Seed فعلی برابر \`label\` است |
| \`codeGroupId\` | number | ارتباط با CodeGroup |
| \`priority\` | number | ترتیب نمایش داخل گروه |
| \`active\` | boolean | فعال یا غیرفعال بودن کد |

## قواعد مهم برای هر دو فرانت

1. برای منطق برنامه از \`label\` استفاده شود؛ متن \`name\` فقط برای نمایش است.
2. وقتی API دامنه فیلدی مثل \`statusId\`، \`typeId\` یا \`importanceId\` می‌خواهد، مقدار \`id\` همان Code ارسال شود.
3. ID و Labelها قرارداد ثابت بک‌اند هستند و نباید در فرانت تغییر داده یا دوباره شماره‌گذاری شوند.
4. لیست گزینه‌ها بر اساس \`priority\` نمایش داده شود.
5. گزینه‌ای که \`active=false\` دارد در فرم ایجاد آیتم جدید نمایش داده نشود؛ نمایش آن در جزئیات داده‌های قدیمی بلامانع است.
6. عنوان فارسی را برای شرط‌گذاری مقایسه نکنید؛ ممکن است متن نمایشی یا ترجمه تغییر کند.
7. غلط‌های املایی موجود در بعضی Labelهای قدیمی مانند \`Companian...\` و \`Compeleted\` بخشی از قرارداد API هستند و باید دقیقاً با همین املا استفاده شوند.

## استفاده در سایت (EndUser)

در حال حاضر Controller عمومی برای Code و CodeGroup در API سایت وجود ندارد. endpointهای \`/api/Admin/Code\` و \`/api/Admin/CodeGroup\` متعلق به پنل و دارای \`Authorize\` هستند؛ سایت نباید برای dropdownهای عمومی به آن‌ها وابسته شود.

تا زمان اضافه‌شدن endpoint عمومی read-only، سایت می‌تواند ID و Labelهای همین سند را به‌عنوان قرارداد ثابت نگه دارد. پیشنهاد بهتر برای ادامه پروژه، اضافه‌کردن endpoint عمومی فقط‌خواندنی برای دریافت Codeهای فعال یک گروه است.

الگوی پیشنهادی برای ثابت‌های فرانت:

\`\`\`ts
export const TicketStatus = {
  waiting: { id: 49, label: "TicketStatus_Waiting" },
  answered: { id: 50, label: "TicketStatus_Answered" },
  closed: { id: 51, label: "TicketStatus_Close" },
} as const;
\`\`\`

نمونه ارسال به API دامنه:

\`\`\`json
{
  "ticketCategoryId": 56,
  "importanceId": 52
}
\`\`\`

## استفاده در پنل ادمین

Base routeها:

- CodeGroup: \`/api/Admin/CodeGroup\`
- Code: \`/api/Admin/Code\`
- همه endpointها نیازمند Access Token هستند.

### جستجوی CodeGroup

\`\`\`http
GET /api/Admin/CodeGroup?pageIndex=1&pageSize=100&q=وضعیت
Authorization: Bearer {adminAccessToken}
\`\`\`

\`q\` روی \`name\` و به‌شکل Contains اعمال می‌شود.

نمونه پاسخ واقعی Search:

\`\`\`json
{
  "pageIndex": 1,
  "pageSize": 100,
  "q": null,
  "sortBy": 1,
  "available": null,
  "totalCount": 34,
  "list": [
    {
      "id": 13,
      "name": "وضعیت تیکت",
      "label": "Ticket_Status"
    }
  ]
}
\`\`\`

### جستجوی Code

\`\`\`http
GET /api/Admin/Code?codeGroupLabel=Ticket_Status&available=true&pageIndex=1&pageSize=100
Authorization: Bearer {adminAccessToken}
\`\`\`

فیلترها:

| Query parameter | توضیح |
|---|---|
| \`codeGroupLabel\` | Label دقیق CodeGroup |
| \`available\` | فیلتر بر اساس \`active\` |
| \`q\` | تطبیق دقیق با \`name\`؛ Contains نیست |
| \`pageIndex\` | پیش‌فرض ۱ |
| \`pageSize\` | پیش‌فرض ۲۰؛ برای گرفتن کل یک گروه مقدار ۱۰۰ پیشنهاد می‌شود |

خروجی بر اساس \`priority\` مرتب می‌شود:

\`\`\`json
{
  "pageIndex": 1,
  "pageSize": 100,
  "codeGroupLabel": "Ticket_Status",
  "totalCount": 3,
  "list": [
    {
      "id": 49,
      "name": "در انتظار پاسخ",
      "label": "TicketStatus_Waiting",
      "value": "TicketStatus_Waiting"
    }
  ]
}
\`\`\`

### دریافت جزئیات

\`\`\`http
GET /api/Admin/CodeGroup/{id}
GET /api/Admin/Code/{id}
\`\`\`

پاسخ داخل \`data\` برمی‌گردد:

\`\`\`json
{
  "isSuccess": true,
  "messages": [],
  "code": 0,
  "data": {
    "id": 49,
    "name": "در انتظار پاسخ",
    "label": "TicketStatus_Waiting",
    "value": "TicketStatus_Waiting",
    "codeGroupId": 13,
    "priority": 1,
    "active": true
  }
}
\`\`\`

### ایجاد و ویرایش CodeGroup

\`\`\`http
POST /api/Admin/CodeGroup
PUT /api/Admin/CodeGroup
\`\`\`

\`\`\`json
{
  "id": 0,
  "name": "عنوان گروه",
  "label": "Stable_Group_Label"
}
\`\`\`

در \`POST\` مقدار \`id\` صفر یا حذف‌شده است. در \`PUT\` ارسال \`id\` الزامی است.

### ایجاد و ویرایش Code

\`\`\`http
POST /api/Admin/Code
PUT /api/Admin/Code
\`\`\`

\`\`\`json
{
  "id": 0,
  "name": "عنوان نمایشی",
  "label": "Stable_Code_Label",
  "value": "Stable_Code_Label",
  "codeGroupId": 13,
  "priority": 4,
  "active": true
}
\`\`\`

### حذف

\`\`\`http
DELETE /api/Admin/CodeGroup?id={id}
DELETE /api/Admin/Code?id={id}
\`\`\`

حذف CodeGroup یا Codeهای Seedشده توصیه نمی‌شود، چون بخش‌های مختلف بک‌اند به Label و گاهی ID آن‌ها وابسته‌اند. برای خارج‌کردن یک Code از فرم‌ها، \`active=false\` امن‌تر است.

## فهرست CodeGroupها

| ID | Label | عنوان | تعداد Code |
|---:|---|---|---:|
${groupRows}

## فهرست کامل Codeها

${codeSections}

## چک‌لیست پیاده‌سازی فرانت

- [ ] Labelها به‌صورت ثابت و case-sensitive نگهداری شده‌اند.
- [ ] برای ارسال به بک‌اند از ID استفاده می‌شود.
- [ ] متن فارسی فقط برای نمایش استفاده می‌شود.
- [ ] Codeهای غیرفعال در فرم ایجاد نمایش داده نمی‌شوند.
- [ ] گزینه‌ها بر اساس Priority مرتب می‌شوند.
- [ ] سایت endpoint پنل ادمین را مصرف نمی‌کند.
- [ ] پنل برای مدیریت Codeها Access Token ارسال می‌کند.
`;

fs.mkdirSync(path.dirname(outputPath), { recursive: true });
fs.writeFileSync(outputPath, markdown, "utf8");

process.stdout.write(JSON.stringify({
    codeGroupCount: codeGroups.length,
    codeCount: codes.length,
    outputPath
}, null, 2));
