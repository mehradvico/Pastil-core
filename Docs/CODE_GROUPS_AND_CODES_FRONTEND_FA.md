# مستند CodeGroup و Code پاستیل

این سند مرجع مشترک تیم فرانت سایت و پنل ادمین است و با Seed فعلی دیتابیس هماهنگ شده است:

- تعداد CodeGroupها: **۳۴**
- تعداد Codeها: **۱۳۷**
- بازه ID گروه‌ها: `1..34`
- بازه ID کدها: `1..137`

## مفهوم داده‌ها

### CodeGroup

هر CodeGroup یک خانواده از مقادیر ثابت سیستم است؛ برای مثال وضعیت تیکت، نوع پرداخت یا وضعیت رزرو.

| فیلد | نوع | توضیح |
|---|---|---|
| `id` | number | شناسه ثابت گروه |
| `label` | string | کلید فنی و پایدار گروه |
| `name` | string | عنوان نمایشی فارسی |

### Code

| فیلد | نوع | توضیح |
|---|---|---|
| `id` | number | شناسه ثابت کد؛ معمولاً همین مقدار در DTOهای دامنه ارسال می‌شود |
| `label` | string | کلید فنی و پایدار؛ مناسب شرط‌ها و enumهای فرانت |
| `name` | string | عنوان قابل نمایش به کاربر |
| `value` | string | در Seed فعلی برابر `label` است |
| `codeGroupId` | number | ارتباط با CodeGroup |
| `priority` | number | ترتیب نمایش داخل گروه |
| `active` | boolean | فعال یا غیرفعال بودن کد |

## قواعد مهم برای هر دو فرانت

1. برای منطق برنامه از `label` استفاده شود؛ متن `name` فقط برای نمایش است.
2. وقتی API دامنه فیلدی مثل `statusId`، `typeId` یا `importanceId` می‌خواهد، مقدار `id` همان Code ارسال شود.
3. ID و Labelها قرارداد ثابت بک‌اند هستند و نباید در فرانت تغییر داده یا دوباره شماره‌گذاری شوند.
4. لیست گزینه‌ها بر اساس `priority` نمایش داده شود.
5. گزینه‌ای که `active=false` دارد در فرم ایجاد آیتم جدید نمایش داده نشود؛ نمایش آن در جزئیات داده‌های قدیمی بلامانع است.
6. عنوان فارسی را برای شرط‌گذاری مقایسه نکنید؛ ممکن است متن نمایشی یا ترجمه تغییر کند.
7. غلط‌های املایی موجود در بعضی Labelهای قدیمی مانند `Companian...` و `Compeleted` بخشی از قرارداد API هستند و باید دقیقاً با همین املا استفاده شوند.

## استفاده در سایت (EndUser)

در حال حاضر Controller عمومی برای Code و CodeGroup در API سایت وجود ندارد. endpointهای `/api/Admin/Code` و `/api/Admin/CodeGroup` متعلق به پنل و دارای `Authorize` هستند؛ سایت نباید برای dropdownهای عمومی به آن‌ها وابسته شود.

تا زمان اضافه‌شدن endpoint عمومی read-only، سایت می‌تواند ID و Labelهای همین سند را به‌عنوان قرارداد ثابت نگه دارد. پیشنهاد بهتر برای ادامه پروژه، اضافه‌کردن endpoint عمومی فقط‌خواندنی برای دریافت Codeهای فعال یک گروه است.

الگوی پیشنهادی برای ثابت‌های فرانت:

```ts
export const TicketStatus = {
  waiting: { id: 49, label: "TicketStatus_Waiting" },
  answered: { id: 50, label: "TicketStatus_Answered" },
  closed: { id: 51, label: "TicketStatus_Close" },
} as const;
```

نمونه ارسال به API دامنه:

```json
{
  "ticketCategoryId": 56,
  "importanceId": 52
}
```

## استفاده در پنل ادمین

Base routeها:

- CodeGroup: `/api/Admin/CodeGroup`
- Code: `/api/Admin/Code`
- همه endpointها نیازمند Access Token هستند.

### جستجوی CodeGroup

```http
GET /api/Admin/CodeGroup?pageIndex=1&pageSize=100&q=وضعیت
Authorization: Bearer {adminAccessToken}
```

`q` روی `name` و به‌شکل Contains اعمال می‌شود.

نمونه پاسخ واقعی Search:

```json
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
```

### جستجوی Code

```http
GET /api/Admin/Code?codeGroupLabel=Ticket_Status&available=true&pageIndex=1&pageSize=100
Authorization: Bearer {adminAccessToken}
```

فیلترها:

| Query parameter | توضیح |
|---|---|
| `codeGroupLabel` | Label دقیق CodeGroup |
| `available` | فیلتر بر اساس `active` |
| `q` | تطبیق دقیق با `name`؛ Contains نیست |
| `pageIndex` | پیش‌فرض ۱ |
| `pageSize` | پیش‌فرض ۲۰؛ برای گرفتن کل یک گروه مقدار ۱۰۰ پیشنهاد می‌شود |

خروجی بر اساس `priority` مرتب می‌شود:

```json
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
```

### دریافت جزئیات

```http
GET /api/Admin/CodeGroup/{id}
GET /api/Admin/Code/{id}
```

پاسخ داخل `data` برمی‌گردد:

```json
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
```

### ایجاد و ویرایش CodeGroup

```http
POST /api/Admin/CodeGroup
PUT /api/Admin/CodeGroup
```

```json
{
  "id": 0,
  "name": "عنوان گروه",
  "label": "Stable_Group_Label"
}
```

در `POST` مقدار `id` صفر یا حذف‌شده است. در `PUT` ارسال `id` الزامی است.

### ایجاد و ویرایش Code

```http
POST /api/Admin/Code
PUT /api/Admin/Code
```

```json
{
  "id": 0,
  "name": "عنوان نمایشی",
  "label": "Stable_Code_Label",
  "value": "Stable_Code_Label",
  "codeGroupId": 13,
  "priority": 4,
  "active": true
}
```

### حذف

```http
DELETE /api/Admin/CodeGroup?id={id}
DELETE /api/Admin/Code?id={id}
```

حذف CodeGroup یا Codeهای Seedشده توصیه نمی‌شود، چون بخش‌های مختلف بک‌اند به Label و گاهی ID آن‌ها وابسته‌اند. برای خارج‌کردن یک Code از فرم‌ها، `active=false` امن‌تر است.

## فهرست CodeGroupها

| ID | Label | عنوان | تعداد Code |
|---:|---|---|---:|
| 1 | `Comment` | وضعیت دیدگاه | 3 |
| 2 | `ProductStatus` | وضعیت محصول | 5 |
| 3 | `ProductType` | نوع محصول | 2 |
| 4 | `Order_PaymentType` | روش پرداخت سفارش | 4 |
| 5 | `Order_Status` | وضعیت سفارش | 4 |
| 6 | `Order_State` | حالت سفارش | 3 |
| 7 | `PaymentType` | نوع پرداخت | 8 |
| 8 | `CompanionReserveState` | وضعیت رزرو همراه | 4 |
| 9 | `CompanionReserveOperatorState` | وضعیت اپراتور رزرو همراه | 3 |
| 10 | `CompanionAssistance_Type` | نوع خدمت همراه | 3 |
| 11 | `CompanionType` | نوع همراه | 6 |
| 12 | `PansionReserveStatus` | وضعیت رزرو پانسیون | 3 |
| 13 | `Ticket_Status` | وضعیت تیکت | 3 |
| 14 | `Ticket_Importance` | اهمیت تیکت | 3 |
| 15 | `Ticket_Category` | دسته‌بندی تیکت | 7 |
| 16 | `RebateType` | نوع کد تخفیف | 7 |
| 17 | `ScoreTransactionType` | نوع تراکنش امتیاز | 6 |
| 18 | `Trip_Status` | وضعیت سفر | 4 |
| 19 | `Driver_Status` | وضعیت راننده در سفر | 3 |
| 20 | `DriverRequestStatus` | وضعیت درخواست راننده | 3 |
| 21 | `CargoStatus` | وضعیت ارسال بار | 3 |
| 22 | `DeliveryType` | نوع ارسال | 4 |
| 23 | `Store_Type` | نوع فروشگاه | 3 |
| 24 | `Discount_Type` | نوع تخفیف محصول | 5 |
| 25 | `Feature_Type` | نوع ویژگی | 2 |
| 26 | `Detail_Type` | نوع جزئیات | 3 |
| 27 | `Map_type` | نوع نقشه | 1 |
| 28 | `PastilMatchGoal` | هدف پاستیل مچ | 4 |
| 29 | `PastilMatchRequestStatus` | وضعیت درخواست پاستیل مچ | 4 |
| 30 | `PastilMatchStatus` | وضعیت پاستیل مچ | 3 |
| 31 | `PastilMatchMessageType` | نوع پیام پاستیل مچ | 4 |
| 32 | `EnergyLevel` | سطح انرژی پت | 5 |
| 33 | `SocialLevel` | سطح اجتماعی پت | 5 |
| 34 | `PushMessageType` | مخاطب پیام پوش | 7 |

## فهرست کامل Codeها

### 1. وضعیت دیدگاه

- CodeGroup Label: `Comment`
- CodeGroupId: `1`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 1 | `Comment_NotChecked` | بررسی نشده | 1 |
| 2 | `Comment_Accept` | تأیید شده | 2 |
| 3 | `Comment_Reject` | رد شده | 3 |

### 2. وضعیت محصول

- CodeGroup Label: `ProductStatus`
- CodeGroupId: `2`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 4 | `ProductStatus_Draft` | پیش‌نویس | 1 |
| 5 | `ProductStatus_NotSell` | غیرقابل فروش | 2 |
| 6 | `ProductStatus_Soo` | به‌زودی | 3 |
| 7 | `ProductStatus_NotAvailable` | ناموجود | 4 |
| 8 | `ProductStatus_Available` | موجود | 5 |

### 3. نوع محصول

- CodeGroup Label: `ProductType`
- CodeGroupId: `3`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 9 | `ProductType_Product` | محصول | 1 |
| 10 | `ProductType_Media` | رسانه | 2 |

### 4. روش پرداخت سفارش

- CodeGroup Label: `Order_PaymentType`
- CodeGroupId: `4`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 11 | `OrderPaymentType_Online` | پرداخت آنلاین | 1 |
| 12 | `OrderPaymentType_Wallet` | پرداخت از کیف پول | 2 |
| 13 | `OrderPaymentType_Combinatorial` | پرداخت ترکیبی | 3 |
| 14 | `OrderPaymentType_Not` | پرداخت نشده | 4 |

### 5. وضعیت سفارش

- CodeGroup Label: `Order_Status`
- CodeGroupId: `5`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 15 | `ProductOrderStatus_Insert` | ثبت شده | 1 |
| 16 | `ProductOrderStatus_Proccess` | در حال پردازش | 2 |
| 17 | `ProductOrderStatus_Send` | ارسال شده | 3 |
| 18 | `ProductOrderStatus_Delivered` | تحویل داده شده | 4 |

### 6. حالت سفارش

- CodeGroup Label: `Order_State`
- CodeGroupId: `6`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 19 | `ProductOrderState_Normal` | عادی | 1 |
| 20 | `ProductOrderState_Edited` | ویرایش شده | 2 |
| 21 | `ProductOrderState_Canceled` | لغو شده | 3 |

### 7. نوع پرداخت

- CodeGroup Label: `PaymentType`
- CodeGroupId: `7`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 22 | `PaymentType_ProductOrder` | سفارش محصول | 1 |
| 23 | `PaymentType_CompanionReserve` | رزرو همراه | 2 |
| 24 | `PaymentType_Trip` | سفر | 3 |
| 25 | `PaymentType_Cargo` | ارسال بار | 4 |
| 26 | `PaymentType_Insurance` | بیمه | 5 |
| 27 | `PaymentType_PansionReserve` | رزرو پانسیون | 6 |
| 28 | `PaymentType_Wallet` | شارژ کیف پول | 7 |
| 29 | `PaymentType_PastilAI` | پاستیل هوش مصنوعی | 8 |

### 8. وضعیت رزرو همراه

- CodeGroup Label: `CompanionReserveState`
- CodeGroupId: `8`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 30 | `CompanianReserveState_Registered` | ثبت شده | 1 |
| 31 | `CompanianReserveState_PrePaid` | پیش‌پرداخت شده | 2 |
| 32 | `CompanianReserveState_Paid` | پرداخت شده | 3 |
| 33 | `CompanianReserveState_Complete` | تکمیل شده | 4 |

### 9. وضعیت اپراتور رزرو همراه

- CodeGroup Label: `CompanionReserveOperatorState`
- CodeGroupId: `9`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 34 | `OperatorState_InComplete` | تکمیل نشده | 1 |
| 35 | `OperatorState_Complete` | تکمیل شده | 2 |
| 36 | `OperatorState_Cancelled` | لغو شده | 3 |

### 10. نوع خدمت همراه

- CodeGroup Label: `CompanionAssistance_Type`
- CodeGroupId: `10`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 37 | `CompanionAssistanceType_Online` | آنلاین | 1 |
| 38 | `CompanionAssistanceType_InPerson` | در محل همراه | 2 |
| 39 | `CompanionAssistanceType_InPlace` | در محل کاربر | 3 |

### 11. نوع همراه

- CodeGroup Label: `CompanionType`
- CodeGroupId: `11`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 40 | `CompanionType_Clinic` | کلینیک | 1 |
| 41 | `CompanionType_DogWalker` | گردش حیوان | 2 |
| 42 | `CompanionType_Laboratory` | آزمایشگاه | 3 |
| 43 | `CompanionType_Barber` | آرایشگر | 4 |
| 44 | `CompanionType_Nurse` | پرستار | 5 |
| 45 | `CompanionType_Grooming` | خدمات گرومینگ | 6 |

### 12. وضعیت رزرو پانسیون

- CodeGroup Label: `PansionReserveStatus`
- CodeGroupId: `12`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 46 | `PansionReserveState_Registered` | ثبت شده | 1 |
| 47 | `PansionReserveState_Paid` | پرداخت شده | 2 |
| 48 | `PansionReserveState_Complete` | تکمیل شده | 3 |

### 13. وضعیت تیکت

- CodeGroup Label: `Ticket_Status`
- CodeGroupId: `13`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 49 | `TicketStatus_Waiting` | در انتظار پاسخ | 1 |
| 50 | `TicketStatus_Answered` | پاسخ داده شده | 2 |
| 51 | `TicketStatus_Close` | بسته شده | 3 |

### 14. اهمیت تیکت

- CodeGroup Label: `Ticket_Importance`
- CodeGroupId: `14`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 52 | `TicketImportance_Normal` | عادی | 1 |
| 53 | `TicketImportance_Important` | مهم | 2 |
| 54 | `TicketImportance_VeryImportant` | خیلی مهم | 3 |

### 15. دسته‌بندی تیکت

- CodeGroup Label: `Ticket_Category`
- CodeGroupId: `15`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 55 | `TicketCategory_General` | عمومی | 1 |
| 56 | `TicketCategory_TechnicalSupport` | پشتیبانی فنی | 2 |
| 57 | `TicketCategory_Financial` | مالی | 3 |
| 58 | `TicketCategory_Account` | حساب کاربری | 4 |
| 59 | `TicketCategory_Product` | محصول | 5 |
| 60 | `TicketCategory_Feedback` | پیشنهاد و انتقاد | 6 |
| 61 | `TicketCategory_Other` | سایر | 7 |

### 16. نوع کد تخفیف

- CodeGroup Label: `RebateType`
- CodeGroupId: `16`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 62 | `RebateType_Cart` | تخفیف سبد خرید | 1 |
| 63 | `RebateType_CompanionReserve` | تخفیف رزرو همراه | 2 |
| 64 | `RebateType_Cargo` | تخفیف ارسال بار | 3 |
| 65 | `RebateType_Trip` | تخفیف سفر | 4 |
| 66 | `RebateType_InsurancePackageSale` | تخفیف بسته بیمه | 5 |
| 67 | `RebateType_PansionReserve` | تخفیف رزرو پانسیون | 6 |
| 68 | `RebateType_PastilAI` | تخفیف پاستیل هوش مصنوعی | 7 |

### 17. نوع تراکنش امتیاز

- CodeGroup Label: `ScoreTransactionType`
- CodeGroupId: `17`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 69 | `ScoreTransactionType_ProductOrder` | امتیاز سفارش محصول | 1 |
| 70 | `ScoreTransactionType_CompanionReserve` | امتیاز رزرو همراه | 2 |
| 71 | `ScoreTransactionType_PansionReserve` | امتیاز رزرو پانسیون | 3 |
| 72 | `ScoreTransactionType_Spent` | مصرف امتیاز | 4 |
| 73 | `ScoreTransactionType_AdminCharge` | شارژ امتیاز توسط ادمین | 5 |
| 74 | `ScoreTransactionType_ReferralCode` | امتیاز کد معرف | 6 |

### 18. وضعیت سفر

- CodeGroup Label: `Trip_Status`
- CodeGroupId: `18`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 75 | `TripStatus_Requested` | درخواست شده | 1 |
| 76 | `TripStatus_Accepted` | پذیرفته شده | 2 |
| 77 | `TripStatus_Canceled` | لغو شده | 3 |
| 78 | `TripStatus_Compeleted` | تکمیل شده | 4 |

### 19. وضعیت راننده در سفر

- CodeGroup Label: `Driver_Status`
- CodeGroupId: `19`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 79 | `DriverStatus_Requested` | در انتظار راننده | 1 |
| 80 | `DriverStatus_Accepted` | پذیرفته شده | 2 |
| 81 | `DriverStatus_Rejected` | رد شده | 3 |

### 20. وضعیت درخواست راننده

- CodeGroup Label: `DriverRequestStatus`
- CodeGroupId: `20`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 82 | `DriverRequestStatus_Requested` | درخواست شده | 1 |
| 83 | `DriverRequestStatus_Accepted` | پذیرفته شده | 2 |
| 84 | `DriverRequestStatus_Rejected` | رد شده | 3 |

### 21. وضعیت ارسال بار

- CodeGroup Label: `CargoStatus`
- CodeGroupId: `21`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 85 | `CargoStatus_Requested` | درخواست شده | 1 |
| 86 | `CargoStatus_Accepted` | پذیرفته شده | 2 |
| 87 | `CargoStatus_Canceled` | لغو شده | 3 |

### 22. نوع ارسال

- CodeGroup Label: `DeliveryType`
- CodeGroupId: `22`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 88 | `DeliveryType_Courier` | پیک | 1 |
| 89 | `DeliveryType_Post` | پست | 2 |
| 90 | `DeliveryType_Tipax` | تیپاکس | 3 |
| 91 | `DeliveryType_InStore` | تحویل حضوری از فروشگاه | 4 |

### 23. نوع فروشگاه

- CodeGroup Label: `Store_Type`
- CodeGroupId: `23`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 92 | `StoreType_ClothingShop` | فروشگاه پوشاک | 1 |
| 93 | `StoreType_PetShop` | پت‌شاپ | 2 |
| 94 | `StoreType_PackageShop` | فروشگاه بسته‌ها | 3 |

### 24. نوع تخفیف محصول

- CodeGroup Label: `Discount_Type`
- CodeGroupId: `24`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 95 | `DiscountType_Store` | تخفیف فروشگاه | 1 |
| 96 | `DiscountType_Category` | تخفیف دسته‌بندی | 2 |
| 97 | `DiscountType_Brand` | تخفیف برند | 3 |
| 98 | `DiscountType_Product` | تخفیف محصول | 4 |
| 99 | `DiscountType_ProductItem` | تخفیف تنوع محصول | 5 |

### 25. نوع ویژگی

- CodeGroup Label: `Feature_Type`
- CodeGroupId: `25`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 100 | `FeatureType_SmallText` | متن کوتاه | 1 |
| 101 | `FeatureType_LargeText` | متن بلند | 2 |

### 26. نوع جزئیات

- CodeGroup Label: `Detail_Type`
- CodeGroupId: `26`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 102 | `Small_Text` | متن کوتاه | 1 |
| 103 | `Large_Text` | متن بلند | 2 |
| 104 | `Geographic_Text` | موقعیت جغرافیایی | 3 |

### 27. نوع نقشه

- CodeGroup Label: `Map_type`
- CodeGroupId: `27`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 105 | `MapType_MapIr` | مپ ایران | 1 |

### 28. هدف پاستیل مچ

- CodeGroup Label: `PastilMatchGoal`
- CodeGroupId: `28`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 106 | `PastilMatchGoal_Walking` | پیاده‌روی | 1 |
| 107 | `PastilMatchGoal_Playing` | بازی | 2 |
| 108 | `PastilMatchGoal_Friendship` | دوستی | 3 |
| 109 | `PastilMatchGoal_ParkMeetup` | قرار در پارک | 4 |

### 29. وضعیت درخواست پاستیل مچ

- CodeGroup Label: `PastilMatchRequestStatus`
- CodeGroupId: `29`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 110 | `PastilMatchRequestStatus_Pending` | در انتظار پاسخ | 1 |
| 111 | `PastilMatchRequestStatus_Accepted` | پذیرفته شده | 2 |
| 112 | `PastilMatchRequestStatus_Rejected` | رد شده | 3 |
| 113 | `PastilMatchRequestStatus_Cancelled` | لغو شده | 4 |

### 30. وضعیت پاستیل مچ

- CodeGroup Label: `PastilMatchStatus`
- CodeGroupId: `30`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 114 | `PastilMatchStatus_Active` | فعال | 1 |
| 115 | `PastilMatchStatus_Closed` | بسته شده | 2 |
| 116 | `PastilMatchStatus_Blocked` | مسدود شده | 3 |

### 31. نوع پیام پاستیل مچ

- CodeGroup Label: `PastilMatchMessageType`
- CodeGroupId: `31`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 117 | `PastilMatchMessageType_Text` | متن | 1 |
| 118 | `PastilMatchMessageType_Image` | تصویر | 2 |
| 119 | `PastilMatchMessageType_Voice` | صدا | 3 |
| 120 | `PastilMatchMessageType_System` | پیام سیستمی | 4 |

### 32. سطح انرژی پت

- CodeGroup Label: `EnergyLevel`
- CodeGroupId: `32`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 121 | `EnergyLevel_VeryLow` | خیلی کم | 1 |
| 122 | `EnergyLevel_Low` | کم | 2 |
| 123 | `EnergyLevel_Medium` | متوسط | 3 |
| 124 | `EnergyLevel_High` | زیاد | 4 |
| 125 | `EnergyLevel_VeryHigh` | خیلی زیاد | 5 |

### 33. سطح اجتماعی پت

- CodeGroup Label: `SocialLevel`
- CodeGroupId: `33`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 126 | `SocialLevel_VeryLow` | خیلی کم | 1 |
| 127 | `SocialLevel_Low` | کم | 2 |
| 128 | `SocialLevel_Medium` | متوسط | 3 |
| 129 | `SocialLevel_High` | زیاد | 4 |
| 130 | `SocialLevel_VeryHigh` | خیلی زیاد | 5 |

### 34. مخاطب پیام پوش

- CodeGroup Label: `PushMessageType`
- CodeGroupId: `34`

| Code ID | Label | عنوان نمایشی | Priority |
|---:|---|---|---:|
| 131 | `PushMessageType_Admin` | مدیران | 1 |
| 132 | `PushMessageType_Companion` | همراهان | 2 |
| 133 | `PushMessageType_Pansion` | پانسیون‌ها | 3 |
| 134 | `PushMessageType_EndUser` | کاربران | 4 |
| 135 | `PushMessageType_Store` | فروشگاه‌ها | 5 |
| 136 | `PushMessageType_Operator` | اپراتورها | 6 |
| 137 | `PushMessageType_All` | همه | 7 |

## چک‌لیست پیاده‌سازی فرانت

- [ ] Labelها به‌صورت ثابت و case-sensitive نگهداری شده‌اند.
- [ ] برای ارسال به بک‌اند از ID استفاده می‌شود.
- [ ] متن فارسی فقط برای نمایش استفاده می‌شود.
- [ ] Codeهای غیرفعال در فرم ایجاد نمایش داده نمی‌شوند.
- [ ] گزینه‌ها بر اساس Priority مرتب می‌شوند.
- [ ] سایت endpoint پنل ادمین را مصرف نمی‌کند.
- [ ] پنل برای مدیریت Codeها Access Token ارسال می‌کند.
