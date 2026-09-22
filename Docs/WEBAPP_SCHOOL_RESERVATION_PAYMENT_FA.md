# مستند وب‌اپ: مدرسه، رزرو، پرداخت و انجام دوره

این سند برای توسعه‌دهنده‌ی Nuxt وب‌اپ پاستیل است. هدف، حفظ قرارداد فعلی BFFها و رفتار درست کاربر از صفحه‌ی مدرسه تا پرداخت و نمایش وضعیت دوره است.

> این سند از APIهای `/app/server/api/school/*` استفاده می‌کند، نه تماس مستقیم از browser با backend. BFF مسئول خواندن cookie توکن، عبور امن خطا و حفظ `Idempotency-Key` پرداخت است.

---

## 1. مسیرها و فایل‌های مرجع فعلی

| وظیفه | Route وب‌اپ | فایل اصلی |
| --- | --- | --- |
| کشف مدرسه | `/school` | `webapp/app/pages/school/index.vue` |
| فهرست همه | `/school/total-all` | `webapp/app/pages/school/total-all.vue` |
| جزئیات مدرسه | `/school/school-{schoolId}` | `webapp/app/pages/school/school-[id].vue` |
| نهایی‌سازی دوره | `/school/finalize-reserve-{courseId}` | `webapp/app/pages/school/finalize-reserve-[id].vue` |
| store | — | `webapp/app/stores/useSchool.ts` |
| BFFها | — | `webapp/app/server/api/school/*` |

منطق business را در صفحه duplicate نکنید. برای تماس‌ها از `useSchoolStore` و برای پرداخت از `useCheckout()` استفاده کنید.

---

## 2. API surface وب‌اپ

| store method | BFF | backend مقصد | نیاز به login |
| --- | --- | --- | --- |
| `getSchools(query)` | `GET /api/school/publicList` | `GET /api/School` | خیر |
| `getPublicSchoolItem(id)` | `GET /api/school/publicItem` | `GET /api/School/{id}` | خیر |
| `getSchoolCourses(query)` | `GET /api/school/course` | `GET /api/SchoolCourse` | خیر |
| `getSchoolCourseItem(id)` | `GET /api/school/courseItem` | `GET /api/SchoolCourse/{id}` | خیر |
| `getCourseRemainingCapacity(id)` | `GET /api/school/courseRemainingCapacity` | `GET /api/SchoolCourse/remainingCapacity/{id}` | خیر |
| `postSchoolReserve(body)` | `POST /api/school/reserve` | `POST /api/EndUser/SchoolReserve` | بله |
| `getSchoolReserveItem(id)` | `GET /api/school/reserveItem` | `GET /api/EndUser/SchoolReserve/{id}` | بله |
| `getMySchoolReserves(query)` | `GET /api/school/myReserve` | `GET /api/EndUser/SchoolReserve` | بله |
| `postSchoolReserveSetRebate(body)` | `PUT /api/school/reserveSetRebate` | `PUT /api/EndUser/SchoolReserveSetRebate` | بله |
| `postSchoolReserveRemoveRebate(id)` | `PUT /api/school/reserveRemoveRebate` | `PUT /api/EndUser/SchoolReserveRemoveRebate` | بله |
| `postSchoolReserveSetWallet(body)` | `PUT /api/school/reserveSetWallet` | `PUT /api/EndUser/SchoolReserveSetWallet` | بله |
| `postSchoolPayment(body)` | `POST /api/school/payment` | `POST /api/EndUser/SchoolReservePayment` | بله |
| `postSchoolReserveCancel(body)` | `PUT /api/school/reserveCancel` | `PUT /api/EndUser/SchoolReserveCancel` | بله |

**قاعده‌ی خطا:** BFFهای authenticated با `withUpstreamStatus` خطا را به envelope امن تبدیل می‌کنند. در UI هم `isSuccess` و هم `_upstreamStatus` را با helper مشترک session/error بررسی کنید؛ object خام upstream را به کاربر log یا render نکنید.

---

## 3. صفحات عمومی مدرسه

### فهرست و جزئیات

- در صفحه فهرست از `getSchools({ PageIndex, PageSize, StateId, SortBy })` استفاده کنید.
- صفحه‌ی مدرسه، `getPublicSchoolItem(schoolId)` و `getSchoolCourses({ SchoolId: schoolId, PageIndex, PageSize })` را هم‌زمان می‌خواند.
- روی card دوره `remainingCapacity` را نشان دهید. اگر صفر است CTA ثبت‌نام disabled باشد.
- پیش از CTA نهایی هم `getSchoolCourseItem(courseId)` را بخوانید؛ این پاسخ معتبرتر از مقدار cache شده در لیست است.
- route parameter ممکن است slug-id باشد؛ مانند الگوی فعلی، بخش پس از `-` را به ID عددی تبدیل کنید.

### داده‌های قابل نمایش دوره

```ts
type CourseCard = {
  id: number
  name: string
  price: number
  courseTypeId: 1 | 2 | 3
  sessionCount: number
  sessionDurationMinutes: number
  capacity: number
  remainingCapacity: number
  petId?: number | null
  petBreedId?: number | null
}
```

برچسب‌ها:

```ts
const courseTypeLabel = {
  1: 'ویدیویی',
  2: 'آنلاین زنده',
  3: 'حضوری',
}
```

برای مقدار ناشناخته، «نامشخص» نمایش دهید؛ fallback به نوع دیگر نداشته باشید.

---

## 4. صفحه‌ی نهایی‌سازی: state و انتخاب پت

### state حداقلی

```ts
const selectedPetIds = ref<number[]>([])
const reserveRecords = ref<Array<{
  petId: number
  record: { id: number; price: number; paymentPrice: number; statusId: number; isReserved: boolean }
}>>([])
const selectedMerchantId = ref<number | null>(null)
const rebateCodeInput = ref('')
const hasRebateApplied = ref(false)
const fromWallet = ref(false)
const submitting = ref(false)
const loadingFinancial = ref(false)
```

### eligibility پت

پت‌ها از `/api/pet/userPet` می‌آیند. filter باید دقیقاً این باشد:

```ts
const eligiblePets = computed(() => petsList.value.filter((pet) => {
  if (petIdFromCourse && pet.petId !== petIdFromCourse) return false
  if (petBreedIdFromCourse && pet.petBreedId !== petBreedIdFromCourse) return false
  return true
}))
```

- حداقل یک پت انتخاب شود.
- بیش از `remainingCapacity` انتخاب نشود.
- بعد از اینکه اولین reserve ایجاد شد، تغییر پت‌ها را lock کنید یا صریحاً یک flow reset/recovery بسازید؛ تغییر بدون reset باعث جا ماندن رزروهای قبلی می‌شود.

### نکته‌ی ظرفیت و ایجاد رزرو

هر POST رزرو برای یک پت ظرفیت را فوراً نگه می‌دارد، حتی قبل از پرداخت. `ensureReservesCreated` باید requestها را ترتیبی بفرستد:

```ts
for (const petId of missingPetIds) {
  const result = await schoolStore.postSchoolReserve({
    schoolCourseId: courseId,
    userPetId: petId,
  })

  if (result?.isSuccess) reserveRecords.value.push({ petId, record: result.data })
  else collectFailure(petId, result)
}
```

بعد از partial failure:

1. reserveهای موفق را نگه دارید؛
2. نام پت و پیام سرور را برای ناموفق‌ها نشان دهید؛
3. `await loadCourse()` اجرا کنید؛
4. انتخاب را به پت‌های موفق محدود کنید یا برای ادامه انتخاب صریح بگیرید.

ایجاد رزرو idempotency header ندارد. در timeout:

```text
POST را خودکار تکرار نکن
→ getMySchoolReserves({ SchoolCourseId: courseId })
→ reserve همان userPetId را پیدا کن
→ در reserveRecords قرار بده و flow را ادامه بده
```

---

## 5. تخفیف و کیف پول

این عملیات باید فقط بعد از ساخته‌شدن reserveها انجام شود. هر عملیات روی همه‌ی `reserveId`ها اجرا می‌شود، ولی نتیجه را per-reserve نگه دارید؛ batch اتمیک وجود ندارد.

```ts
await Promise.all(reserveIds.map((id) =>
  schoolStore.postSchoolReserveSetRebate({ id, rebateCode: rebateCodeInput.value }),
))

await Promise.all(reserveIds.map((id) =>
  schoolStore.postSchoolReserveSetWallet({ id, fromWallet: fromWallet.value }),
))

await refreshReserves()
```

پس از هر عملیات، `refreshReserves()` لازم است؛ قیمت فاکتور باید از `record.paymentPrice`، `rebatePrice` و `walletPrice` تازه‌شده بیاید، نه محاسبه‌ی صرفاً محلی.

بعد از آغاز payment، API تغییرات مالی را رد می‌کند. هنگامی که CTA پرداخت in-flight است، بخش تخفیف/کیف پول/انتخاب پت را disabled کنید.

---

## 6. پرداخت با useCheckout

`postSchoolPayment` باید تنها درگاه پرداخت صفحه باشد و از `useCheckout().requestCheckout` استفاده کند:

```ts
const response = await requestCheckout(
  '/api/school/payment',
  `school-reserve:${reserveId}`,
  {
    schoolReserveId: reserveId,
    merchantId: selectedMerchantId.value,
  },
)
```

`useCheckout` UUID را در sessionStorage و در scope همان reserve نگه می‌دارد و BFF آن را به `Idempotency-Key` backend منتقل می‌کند. بنابراین:

- برای یک reserve و retry آن، scope را تغییر ندهید؛
- برای reserve بعدی scope جداست؛
- CTA را تا پاسخ busy کنید؛
- `amount`، `callbackTypeLabel`، `paymentCode` و مبلغ نهایی را برای تصمیم backend ارسال یا بازنویسی نکنید.

بدنه‌ی canonical پرداخت فقط `schoolReserveId` و در صورت نیاز `merchantId` است. در نسخه‌ی فعلی [`finalize-reserve-[id].vue`](../../webapp/app/pages/school/finalize-reserve-[id].vue) چند فیلد اضافه مانند `amount` و `callBackTypeLabel` هم می‌فرستد؛ backend آن‌ها را overwrite می‌کند، اما برای جلوگیری از برداشت اشتباه و drift قرارداد باید در بازنویسی بعدی حذف شوند.

### واکنش به پاسخ

```ts
if (!response?.isSuccess) {
  showError(response?.messages?.[0]?.item1 ?? 'شروع پرداخت ناموفق بود')
  return
}

if (response.data?.paymentIsLink && response.data?.paymentUrl) {
  window.location.assign(response.data.paymentUrl)
  return
}

await refreshReserves()
showSuccess('پرداخت با کیف پول یا تخفیف نهایی شد')
```

- `paymentIsLink=false` یعنی پرداخت داخلی انجام شده است؛ redirect نکنید.
- callback URL را نسازید یا parse نکنید؛ فقط `paymentUrl` پاسخ را با full-page navigation باز کنید.
- بعد از بازگشت از درگاه، با `getMySchoolReserves` یا `getSchoolReserveItem` وضعیت واقعی را دوباره بگیرید. redirect شدن به معنی final payment نیست.

### چند پت: پرداخت ترتیبی

پرداخت گروهی مدرسه وجود ندارد. اگر پرداخت اولین reserve کاربر را به درگاه برد:

```text
پرداخت reserve جاری → redirect
بازگشت از درگاه → برو به /reserve?section=training یا صفحه ثبت‌نام‌های من
رزروهای Registered باقی‌مانده → CTA «پرداخت» جداگانه
```

هرگز remaining reserveها را قبل از برگشت کاربر از درگاه در loop ادامه ندهید.

---

## 7. نمایش رزرو و انجام دوره

در صفحه‌ی «رزروهای من» این وضعیت‌ها را با `statusId` و `isReserved` نمایش دهید:

| وضعیت | condition | CTA کاربر |
| --- | --- | --- |
| در انتظار پرداخت | `statusId === 1 && !isReserved` | ادامه پرداخت |
| پرداخت‌شده | `statusId === 2 && isReserved` | مشاهده دوره و جلسات |
| کامل‌شده | `statusId === 3` | مشاهده نتیجه/تاریخچه |
| لغوشده | `statusId === 4 || isCancel` | بدون CTA پرداخت |

جلسات دوره از `record.schoolCourse.schoolCourseSessions` قابل نمایش‌اند. برای Live/حضوری، تاریخ، بازه‌ی زمان و (اگر وجود دارد) `meetingUrl` را نشان دهید. push شروع کلاس برای جلسه‌های غیر ویدیویی حداکثر تا ۱۰ دقیقه مانده به شروع و توسط job پنج‌دقیقه‌ای ارسال می‌شود؛ جدول زمان‌بندی باید بدون اتکا به push موجود باشد.

**تکمیل دوره:** end-user endpoint برای تغییر به Complete یا ثبت حضور ندارد. فقط مدیریت پاستیل/ارائه‌دهنده status را تغییر می‌دهد؛ وب‌اپ صرفاً آن را در refresh می‌خواند. دکمه‌ی «اتمام دوره» یا «تأیید حضور» سمت کاربر نسازید.

### لغو

```ts
await schoolStore.postSchoolReserveCancel({
  id: reserveId,
  isCancel: true,
  cancelDetail: reason.trim(),
})
```

API فقط رزرو پرداخت‌شده را لغو می‌کند و علت غیرخالی لازم دارد. رزرو پرداخت‌نشده را قابل لغو نمایش ندهید. این endpoint refund خودکار انجام نمی‌دهد؛ بدون قرارداد جداگانه متن «بازگشت وجه» نشان ندهید.

### محدودیت فعلی محتوا

API عمومی course اکنون session و videoها را برمی‌گرداند. بنابراین قفل واقعی محتوای ویدئویی/لینک کلاس بر مبنای پرداخت در backend موجود نیست. وب‌اپ نباید محتوای عمومی را به‌عنوان «فقط پس از خرید قابل مشاهده» معرفی کند تا زمانی که endpoint احراز هویت‌شده و authorization server-side ساخته شود.

---

## 8. چک‌لیست review وب‌اپ

- [ ] همه‌ی صفحات course عدد `remainingCapacity` را refresh می‌کنند؛ صفر CTA ندارد.
- [ ] هر pet یک POST جدا و ترتیبی می‌گیرد.
- [ ] timeout ساخت reserve با query رزروهای من recover می‌شود، نه POST کور.
- [ ] قیمت فاکتور پس از rebate/wallet فقط از reserve refresh‌شده می‌آید.
- [ ] هر پرداخت از `useCheckout` و scope `school-reserve:${reserveId}` می‌گذرد.
- [ ] `paymentIsLink=false` redirect ندارد؛ بعدش state refresh می‌شود.
- [ ] redirect خارجی با همان `paymentUrl` پاسخ است.
- [ ] وضعیت pending/paid/complete/cancelled در صفحه رزروها جدا و قابل فهم است.
- [ ] CTA لغو فقط برای reserve پرداخت‌شده و با علت غیرخالی نمایش داده می‌شود.
- [ ] جلسه و اعلان شروع کلاس جایگزین هم نیستند؛ جدول جلسات در دسترس است.

**وضعیت سند:** منطبق با backend و webapp فعلی پاستیل در ۲۱ سپتامبر ۲۰۲۶.
