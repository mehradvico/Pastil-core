# ردیابی پیک میاره — مستند کامل وب‌اپ (مشتری + بک‌آفیس فروشنده‌ی داخل وب‌اپ)

> مخاطب: تیم وب‌اپ (Nuxt 4) — آرمان.
> وضعیت بک‌اند: **ساخته و تست‌شده، دیپلوی نشده.** `Api` جدید + اسکریپت `backend/scripts/AddShipmentCourierTracking.sql` باید اول اجرا شود؛ قبل از آن endpointهای این سند خطا می‌دهند.
> قرارداد مشترک و طراحی داخلی: [MIARE_COURIER_TRACKING_FA.md](MIARE_COURIER_TRACKING_FA.md). این سند **مستقل** است؛ همه‌ی چیزهای لازم برای وب‌اپ همین‌جا آمده.
> سند اپ نماینده (فلاتر) جداست: [MIARE_COURIER_TRACKING_NEMAYANDE_APP_FA.md](MIARE_COURIER_TRACKING_NEMAYANDE_APP_FA.md).

## ۰) قواعدی که قبل از هر کاری باید بدانید

1. **پاسخ همیشه HTTP 200** با `{ isSuccess, code, messages, data }` است. موفقیت را فقط از `isSuccess` بخوانید. پیام خطا: `messages[0].item1`.
2. فیلدها **camelCase** هستند. زمان‌ها **UTC با `Z`** هستند؛ نمایش به وقت تهران (`timeZone: 'Asia/Tehran'`).
3. **enumها در JSON عددی‌اند** (رشته نیستند): `shippingProvider`: `0` هیچ، `1` الوپیک، `2` تیپاکس، `3` اسنپ‌باکس، **`4` میاره**. `shipmentStatus`/`status` جدول بخش ۴ را ببینید.
4. فقط سفارش‌هایی که **با میاره** ارسال می‌شوند ردیابی دارند (`shippingProvider === 4` و `isPaid`). سفارش پستی/بدون مرسوله: بخش ردیابی **نمایش داده نشود**.
5. نام و تلفن و موقعیت پیک **حساس** است: در `localStorage`/Pinia persist/لاگ/آنالیتیکس ذخیره نشود و با خروج از صفحه پاک شود (بخش ۹).
6. هر درخواست ردیابی دو کوئری سبک است؛ ولی polling فقط وقتی صفحه دیده می‌شود، و **با `isFinal=true` متوقف می‌شود** (بخش ۵.۳).

## ۱) خلاصه‌ی کارها

| # | فایل | کار |
|---|---|---|
| ۱ | `app/server/api/shipmentTracking.get.js` | **جدید** — BFF ردیابی مشتری |
| ۲ | `app/server/api/seller/shipmentTracking.get.js` | **جدید** — BFF ردیابی فروشنده |
| ۳ | `app/pages/reserve/order-[orderId].vue` | بخش «پیگیری ارسال» + کارت «کد تحویل» (الان اصلاً نشان داده نمی‌شود) |
| ۴ | `app/components/global/Mapir.vue` | (اختیاری) نشانگر `courier` |
| ۵ | `app/pages/sellerProfile/orders/index.vue` | دکمه/مودال «ردیابی پیک» + اصلاح متن‌های کهنه‌ی «۱۵/۲۰ دقیقه» |

## ۲) BFFهای جدید (کامل)

### ۲.۱) `app/server/api/shipmentTracking.get.js` (مشتری)

```js
// ردیابی پیک سفارش مشتری. query: productOrderId (شناسه‌ی guid یا شماره‌ی سفارش)
export default defineEventHandler(async (event) => {
  const { public: { baseUrl } } = useRuntimeConfig()
  const token = getCookie(event, 'token')
  const query = getQuery(event)
  try {
    return await $fetch(`${baseUrl}/api/EndUser/ShipmentTracking`, {
      method: 'GET',
      headers: { Accept: 'application/json', Authorization: `Bearer ${token}` },
      query: { productOrderId: String(query.productOrderId || '') }
    })
  } catch (error) {
    return normalizeApiError(error, 'خطا در برقراری ارتباط با سرور')
  }
})
```

### ۲.۲) `app/server/api/seller/shipmentTracking.get.js` (فروشنده)

الگوی دقیقاً مثل `app/server/api/seller/shipmentAwaiting.get.js` است (هدر `StoreId` الزامی):

```js
// ردیابی پیک یک سفارش فروشگاه. query: storeId, productOrderStoreId
export default defineEventHandler(async (event) => {
  const { public: { baseUrl } } = useRuntimeConfig()
  const token = getCookie(event, 'token')
  const query = getQuery(event)
  try {
    return await $fetch(`${baseUrl}/api/Seller/Shipment/Tracking`, {
      method: 'GET',
      headers: { Accept: 'application/json', Authorization: `Bearer ${token}`, StoreId: String(query.storeId || '') },
      query: { productOrderStoreId: String(query.productOrderStoreId || '') }
    })
  } catch (error) {
    return normalizeApiError(error, 'خطا در برقراری ارتباط با سرور')
  }
})
```

## ۳) قرارداد API

### ۳.۱) مشتری — `GET /api/shipmentTracking?productOrderId={id|orderCode}`

برمی‌گرداند: **لیست** (به‌ازای هر فروشگاهِ سفارش که با پیک می‌رود یکی). سفارش دیگران یا سفارشِ بدون پیک = `data: []` (خطا نیست).

```json
{
  "isSuccess": true, "code": 0, "messages": [],
  "data": [{
    "shipmentId": 123, "productOrderStoreId": 456, "storeId": 5, "storeName": "پی کی پت",
    "provider": "Miare", "status": 4, "statusKey": "PickedUp",
    "isExpress": true, "isFinal": false, "isLive": true,
    "currentStepKey": "picked_up",
    "headline": "پیک سفارش را از فروشگاه تحویل گرفت و در راه است.",
    "steps": [
      { "key": "preparing", "title": "فروشگاه در حال آماده‌سازی سفارش است.", "done": true, "current": false, "atUtc": "2026-10-25T10:05:00Z" },
      { "key": "picked_up", "title": "پیک سفارش را از فروشگاه تحویل گرفت و در راه است.", "done": false, "current": true, "atUtc": "2026-10-25T10:41:00Z" },
      { "key": "delivered", "title": "سفارش تحویل داده شد.", "done": false, "current": false, "atUtc": null }
    ],
    "courier": {
      "name": "علی", "phone": "09120000000", "canCall": true,
      "hasLocation": true, "latitude": 35.7101, "longitude": 51.4102,
      "locationUpdatedAtUtc": "2026-10-25T10:50:08Z", "locationAgeSeconds": 22, "locationStale": false,
      "distanceKm": 3.4, "distanceTarget": "destination"
    },
    "store": { "latitude": 35.70, "longitude": 51.39 },
    "destination": { "latitude": 35.73, "longitude": 51.42 },
    "plannedPickupUtc": "2026-10-25T10:50:00Z", "deliveredAtUtc": null,
    "lastUpdateUtc": "2026-10-25T10:50:08Z", "pollSeconds": 15
  }]
}
```

(در پاسخ واقعی `steps` همیشه **۵ مرحله** است: `preparing`، `courier_requested`، `courier_assigned`، `picked_up`، `delivered`؛ در لغو/ناموفق یک مرحله‌ی ششم `cancelled` / `failed` / `returning` اضافه می‌شود. مثال بالا برای کوتاهی خلاصه شده.)

#### فیلدها

| فیلد | معنی / رفتار UI |
|---|---|
| `productOrderStoreId` | کلید مرسوله؛ با `orderInfo.productOrderStores[].id` یکی است (برای پیدا کردن `shipmentDeliveryCode`) |
| `status` / `statusKey` | جدول بخش ۴ |
| `isFinal` | `true` برای ۵ تحویل‌شده، ۶ لغو، ۷ ناموفق ⇒ **polling را قطع کنید** |
| `isLive` | `true` فقط برای ۳ و ۴ ⇒ نقشه‌ی زنده معنی دارد |
| `currentStepKey` / `headline` | مرحله‌ی جاری و **متن فارسی آماده** (از سرور؛ ترجمه‌ی سمت کلاینت لازم نیست) |
| `steps[]` | `key`، `title` (فارسی)، `done`، `current`، `atUtc` (فقط برای مرحله‌ی انجام‌شده/جاری، وگرنه `null`) |
| `courier` | **`null` است** وقتی پیک هنوز تعیین نشده یا اطلاعات برای این وضعیت مجاز نیست (جدول بخش ۴). UI باید با `null` درست کار کند |
| `courier.name` | فقط **نام کوچک** (حریم خصوصی پیک) |
| `courier.phone` / `canCall` | فقط در وضعیت ۴ (کالا دست پیک است). دکمه‌ی تماس فقط اگر `canCall` |
| `courier.hasLocation`, `latitude`, `longitude` | موقعیت زنده در وضعیت ۳ و ۴ |
| `courier.locationStale` | موقعیت قدیمی‌تر از ۱۰ دقیقه ⇒ «آخرین موقعیت معلوم» نه «زنده» |
| `courier.locationAgeSeconds` | برای نمایش «n ثانیه/دقیقه پیش» (هر بار polling دوباره بخوانید، خودتان شمارنده نسازید) |
| `courier.distanceKm` / `distanceTarget` | فاصله‌ی **هوایی** (نه مسیر)؛ `store` = تا فروشگاه (وضعیت ۳)، `destination` = تا آدرس شما (وضعیت ۴). ETA نداریم |
| `store` / `destination` | مختصات فروشگاه و **آدرس خود مشتری** (برای نقشه)؛ ممکن است `null` باشد |
| `plannedPickupUtc` | ساعت تقریبیِ رسیدن پیک به فروشگاه |
| `pollSeconds` | فاصله‌ی polling (۱۵) |

> کد تحویل در این API **نیست**؛ از `orderInfo.productOrderStores[].shipmentDeliveryCode` می‌آید (بخش ۵.۵).

### ۳.۲) فروشنده — `GET /api/seller/shipmentTracking?storeId={..}&productOrderStoreId={..}`

همان ساختار، با این تفاوت‌ها:
- **`destination` هرگز نمی‌آید** (مختصات مشتری برای فروشنده ممنوع است).
- `courier.name` **نام کامل**؛ تلفن و موقعیت و فاصله‌تا‌فروشگاه **فقط وقتی `status = 3`** (پیک به‌سمت فروشگاه می‌آید). از وضعیت ۴ به بعد `courier = null`.
- سفارش فروشگاه دیگر ⇒ `isSuccess=false` با پیام «یافت نشد».
- `productOrderStoreId` را از سطر سفارش بگیرید (بخش ۶.۱).

## ۴) وضعیت‌ها و چه چیزی دیده می‌شود

| `status` | `statusKey` | معنی | نام پیک | تلفن | موقعیت/فاصله | polling |
|---|---|---|---|---|---|---|
| ۸، ۹ | AwaitingSellerConfirm / Preparing | فروشگاه در حال آماده‌سازی | — | — | — | بله |
| ۲ | Requested | پیک درخواست شد، در صف تخصیص | — | — | — | بله |
| ۳ | Accepted | پیک تعیین شد، به‌سمت فروشگاه | مشتری: نام کوچک · فروشنده: نام کامل | فقط فروشنده | هر دو؛ فاصله تا فروشگاه | بله |
| ۴ | PickedUp | کالا دست پیک، به‌سمت مشتری | مشتری: نام کوچک | **فقط مشتری** | مشتری؛ فاصله تا آدرس | بله |
| ۵ | Delivered | تحویل شد | — | — | — | **خیر** |
| ۶ | Cancelled | لغو | — | — | — | **خیر** |
| ۷ | Failed | ناموفق (تأخیر/برگشت) | — | — | — | **خیر** |

## ۵) صفحه‌ی سفارش مشتری — `app/pages/reserve/order-[orderId].vue`

(مسیرهای قدیمی `/orders` و `/orders/:id` به همین صفحه ریدایرکت می‌شوند؛ کاری لازم نیست.)

### ۵.۱) شرط نمایش

بخش «پیگیری ارسال» فقط وقتی نشان داده شود که: `orderInfo.isPaid` و `isCancelled` نباشد (مگر وضعیت لغوِ مرسوله)، و **حداقل یک** `orderInfo.productOrderStores[]` با `shippingProvider === 4` باشد، و `tracking.length > 0`. اگر `tracking` خالی است هیچ‌چیز رندر نکنید (نه خطا، نه اسکلتون دائمی).

### ۵.۲) state و توابع (داخل `<script setup>`، کنار `loadOrder`)

```js
// ---- ردیابی پیک (میاره) ----
const tracking = ref([])            // لیست مرسوله‌ها
const trackingLoaded = ref(false)
let trackTimer = null

const hasMiareStore = computed(() =>
  (orderInfo.value?.productOrderStores || []).some((s) => Number(s.shippingProvider) === 4)
)

const loadTracking = async (silent = true) => {
  if (!orderInfo.value?.isPaid || !hasMiareStore.value) { tracking.value = []; return }
  try {
    const res = await $fetch('/api/shipmentTracking', { query: { productOrderId: route.params.orderId } })
    if (res?.isSuccess) tracking.value = Array.isArray(res.data) ? res.data : []
    // isSuccess=false: لیست قبلی را نگه دارید (قطع لحظه‌ای)؛ چیزی به کاربر نشان ندهید
  } catch (e) {
    // خطای شبکه: بی‌صدا؛ پولینگ بعدی دوباره تلاش می‌کند
  } finally {
    trackingLoaded.value = true
  }
}
const trackingActive = computed(() => tracking.value.some((t) => !t.isFinal))
const trackingPollMs = computed(() =>
  Math.max(10, Math.min(60, Number(tracking.value[0]?.pollSeconds) || 15)) * 1000
)
const stopTracking = () => { if (trackTimer) { clearInterval(trackTimer); trackTimer = null } }
const startTracking = () => {
  stopTracking()
  if (!trackingActive.value) return
  trackTimer = setInterval(() => {
    if (document.visibilityState !== 'visible') return      // پس‌زمینه: درخواست نزنید
    loadTracking().then(() => { if (!trackingActive.value) stopTracking() })
  }, trackingPollMs.value)
}
```

### ۵.۳) اتصال به چرخه‌ی عمر صفحه (جایگزین `onMounted`/`onBeforeUnmount` فعلی)

در `onMounted` بعد از `await loadOrder()` اضافه کنید و در `visibilitychange` هم یک بار فوراً تازه کنید:

```js
onMounted(async () => {
  await loadOrder()
  await loadTracking()
  startTracking()
  void handleDeliveryDeepLink()
  liveTimer = setInterval(refreshLive, LIVE_MS)
  document.addEventListener('visibilitychange', refreshLive)
  document.addEventListener('visibilitychange', onVisibleTracking)
})
const onVisibleTracking = () => {
  if (document.visibilityState === 'visible' && trackingActive.value) { loadTracking(); startTracking() }
}
onBeforeUnmount(() => {
  if (liveTimer) clearInterval(liveTimer)
  stopTracking()
  tracking.value = []                       // اطلاعات پیک در حافظه نماند
  document.removeEventListener('visibilitychange', refreshLive)
  document.removeEventListener('visibilitychange', onVisibleTracking)
})
```

نکته: `refreshLive` فعلی هر ۷ ثانیه کل سفارش را می‌خواند. بعد از هر بار که `loadOrder(true)` تمام شد اگر وضعیت مرسوله عوض شده باشد ردیابی هم تازه می‌شود؛ ساده‌ترین راه: داخل `refreshLive` بعد از `loadOrder(true)` یک `loadTracking()` هم صدا بزنید **فقط اگر** `trackingActive.value` یا `tracking.value.length === 0`.

### ۵.۴) قالب (Template)

جای درج: **بعد از** `</section>` که «فروشگاه و کالاهای سفارش» را می‌بندد (قبل از `<section ... >اطلاعات پیگیری</section>`). از همان کلاس‌های صفحه استفاده شده (`border-border/60 bg-surface rounded-2xl`، متن‌های `text-[9px]`):

```vue
<section v-if="tracking.length" class="space-y-3">
  <h2 class="text-xs font-bold">پیگیری ارسال</h2>

  <article
    v-for="t in tracking"
    :key="t.shipmentId"
    class="overflow-hidden rounded-2xl border border-border/60 bg-surface p-4"
  >
    <!-- سربرگ: فروشگاه + اکسپرس -->
    <div class="flex items-center justify-between gap-2">
      <h3 class="min-w-0 truncate text-[11px] font-bold">{{ t.storeName }}</h3>
      <span v-if="t.isExpress" class="shrink-0 rounded-lg border border-amber-500/30 bg-amber-500/10 px-2 py-1 text-[8px] font-bold text-amber-400">
        <Icon name="solar:bolt-bold" size="10" class="ml-0.5 inline" />اکسپرس
      </span>
    </div>

    <!-- جمله‌ی وضعیت (از سرور) -->
    <p
      class="mt-3 rounded-lg px-3 py-2 text-[10px] font-bold leading-5"
      :class="trackTone(t)"
    >{{ t.headline }}</p>

    <!-- مراحل (عمودی؛ تعداد مرحله ۵ یا ۶ است) -->
    <ol class="mt-4 space-y-3">
      <li v-for="step in t.steps" :key="step.key" class="flex items-start gap-3">
        <span
          class="mt-0.5 flex h-4 w-4 shrink-0 items-center justify-center rounded-full border-2"
          :class="step.done ? 'border-primary bg-primary text-white'
            : step.current ? (isBadStep(step.key) ? 'border-red-500 bg-red-500 text-white' : 'border-primary bg-surface')
            : 'border-border bg-surface'"
        >
          <Icon v-if="step.done" name="solar:check-read-linear" size="9" />
        </span>
        <div class="min-w-0 flex-1">
          <p class="text-[9px] leading-5" :class="step.done || step.current ? 'font-bold text-text-primary' : 'text-text-secondary'">{{ step.title }}</p>
          <p v-if="step.atUtc" class="text-[8px] text-text-secondary">{{ timeText(step.atUtc) }}</p>
        </div>
      </li>
    </ol>

    <!-- کارت پیک (فقط وقتی courier != null) -->
    <div v-if="t.courier" class="mt-4 space-y-3 rounded-xl bg-bg p-3">
      <div class="flex items-center justify-between gap-3">
        <div class="flex min-w-0 items-center gap-2.5">
          <div class="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-primary/10 text-primary">
            <Icon name="solar:delivery-bold-duotone" size="20" />
          </div>
          <div class="min-w-0">
            <p class="text-[8px] text-text-secondary">پیک شما</p>
            <p class="truncate text-[11px] font-bold">{{ t.courier.name || 'پیک میاره' }}</p>
          </div>
        </div>
        <a
          v-if="t.courier.canCall && t.courier.phone"
          :href="`tel:${t.courier.phone}`"
          class="flex h-10 shrink-0 items-center gap-1.5 rounded-xl border border-green-500/30 bg-green-500/10 px-3 text-[10px] font-bold text-green-500 active:scale-95"
        >
          <Icon name="solar:phone-calling-bold" size="16" />تماس
        </a>
      </div>

      <div v-if="t.courier.hasLocation" class="flex flex-wrap items-center gap-2 text-[9px]">
        <span v-if="t.courier.distanceKm != null" class="rounded-lg bg-surface px-2.5 py-1.5 font-bold text-text-primary">
          {{ faNum(t.courier.distanceKm) }} کیلومتر تا {{ t.courier.distanceTarget === 'destination' ? 'شما' : 'فروشگاه' }}
        </span>
        <span class="rounded-lg px-2.5 py-1.5" :class="t.courier.locationStale ? 'bg-amber-500/10 text-amber-500' : 'bg-surface text-text-secondary'">
          {{ t.courier.locationStale ? 'آخرین موقعیت معلوم: ' : 'به‌روزرسانی: ' }}{{ ageText(t.courier.locationAgeSeconds) }}
        </span>
      </div>

      <!-- نقشه‌ی اختصاصی پاستیل -->
      <div v-if="t.isLive && t.courier.hasLocation" class="h-56 overflow-hidden rounded-xl border border-border/60">
        <Mapir
          :key="t.shipmentId"
          :markers="trackMarkers(t)"
          :center="[t.courier.longitude, t.courier.latitude]"
          :zoom="14"
          :interactive="true"
          @ready="(map) => fitTrack(map, t)"
        />
      </div>
    </div>

    <!-- کد تحویل (بخش ۵.۵) -->
    <div
      v-if="deliveryCodeFor(t)"
      class="mt-4 rounded-xl border border-primary/30 bg-primary/5 p-3 text-center"
    >
      <p class="text-[9px] text-text-secondary">کد تحویل شما</p>
      <p class="mt-1 text-2xl font-black tracking-[.4em] text-primary" dir="ltr">{{ deliveryCodeFor(t) }}</p>
      <p class="mt-2 text-[8px] leading-5 text-text-secondary">
        فقط بعد از دریافت و بررسی سالم بودن کالا، این کد را به پیک بگویید. گفتن کد به معنی تأیید دریافت است.
      </p>
    </div>
  </article>
</section>
```

### ۵.۵) توابع کمکی قالب

```js
const faDigits = (s) => String(s).replace(/\d/g, (d) => '۰۱۲۳۴۵۶۷۸۹'[d])
const faNum = (n) => Number(n).toLocaleString('fa-IR', { maximumFractionDigits: 1 })
const timeText = (utc) => utc
  ? new Date(utc).toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit', timeZone: 'Asia/Tehran' })
  : ''
const ageText = (sec) => {
  if (sec == null) return 'نامشخص'
  if (sec < 60) return 'چند ثانیه پیش'
  const m = Math.floor(sec / 60)
  return m < 60 ? `${faNum(m)} دقیقه پیش` : `${faNum(Math.floor(m / 60))} ساعت پیش`
}
const isBadStep = (key) => key === 'cancelled' || key === 'failed' || key === 'returning'
const trackTone = (t) =>
  t.status === 7 || t.status === 6 ? 'bg-red-500/10 text-red-500'
  : t.status === 5 ? 'bg-green-500/10 text-green-500'
  : 'bg-primary/10 text-primary'

// کد تحویل از سفارش می‌آید (نه از ردیابی): فقط وقتی کالا دست پیک است (shipmentStatus >= 4) پر است و برای فروشنده/ادمین هیچ‌وقت.
const deliveryCodeFor = (t) => {
  const row = (orderInfo.value?.productOrderStores || []).find((s) => s.id === t.productOrderStoreId)
  return row?.shipmentDeliveryCode || ''
}
// نشانگرها: فروشگاه (مبدأ)، آدرس مشتری (مقصد)، پیک (driver)
const trackMarkers = (t) => [
  ...(t.store ? [{ lat: t.store.latitude, lng: t.store.longitude, type: 'origin', title: t.storeName }] : []),
  ...(t.destination ? [{ lat: t.destination.latitude, lng: t.destination.longitude, type: 'destination', title: 'آدرس شما' }] : []),
  ...(t.courier?.hasLocation ? [{ lat: t.courier.latitude, lng: t.courier.longitude, type: 'courier', title: 'پیک' }] : [])
]
// فقط بار اول همه‌ی نقاط را در کادر بیاورید؛ بعد از آن کاربر نقشه را جابه‌جا می‌کند و نباید هر ۱۵ ثانیه برگردد
const fittedShipments = new Set()
const fitTrack = (map, t) => {
  if (fittedShipments.has(t.shipmentId)) return
  const pts = trackMarkers(t).map((m) => [m.lng, m.lat])
  if (pts.length < 2) return
  fittedShipments.add(t.shipmentId)
  const lngs = pts.map((p) => p[0]), lats = pts.map((p) => p[1])
  map.fitBounds([[Math.min(...lngs), Math.min(...lats)], [Math.max(...lngs), Math.max(...lats)]],
    { padding: 50, maxZoom: 16, duration: 0 })
}
```

- `shipmentDeliveryCode` را **فقط** به خود مشتری نشان دهید. سرور بیرون از وضعیت ۴ آن را `null` می‌کند؛ شما هیچ شرط اضافه‌ای لازم ندارید.
- **این بخش تازه است:** صفحه‌ی فعلی کد تحویل را اصلاً نشان نمی‌دهد (فقط پیامک دارد). مشتری‌ای که پیامک را گم کرده باید کد را اینجا ببیند.
- نشانگر نقشه‌ی پیک: `Mapir.vue` نوع `driver` (آواتار گرد با 🚕) دارد. برای نوع `courier` تغییر ۴ خطی در `updateMarkers` (بخش ۵.۶).

### ۵.۶) `Mapir.vue` — نشانگر `courier` (اختیاری ولی توصیه می‌شود)

در `updateMarkers` شرط `if (m.type === 'driver')` را به `if (m.type === 'driver' || m.type === 'courier')` تغییر دهید و ایموجی را بسته به نوع بگذارید:

```js
const isMover = m.type === 'driver' || m.type === 'courier'
const moverEmoji = m.type === 'courier' ? '🛵' : '🚕'
if (isMover) {
  // ... همان کد فعلی؛ فقط همه‌ی '🚕' را با moverEmoji عوض کنید
  // و alt/title پیش‌فرض را: m.type === 'courier' ? 'پیک' : 'راننده پت‌رسان'
}
const marker = new maplibregl.Marker({ element: el, anchor: isMover ? 'center' : 'bottom' })
```

اگر نمی‌خواهید `Mapir` را دست بزنید، در `trackMarkers` به‌جای `type: 'courier'` بگذارید `type: 'driver'` (🚕 نشان داده می‌شود؛ کار می‌کند).

### ۵.۷) هر وضعیت چه چیزی را به مشتری نشان می‌دهد

| وضعیت | `headline` (از سرور) | عناصر UI |
|---|---|---|
| ۸/۹ | فروشگاه در حال آماده‌سازی سفارش است. | فقط مراحل |
| ۲ | پیک درخواست شد و در انتظار تخصیص است. | مراحل |
| ۳ | پیک تعیین شد و در مسیر فروشگاه است. | مراحل + کارت پیک (نام کوچک، موقعیت، فاصله تا فروشگاه)، **بدون تلفن** |
| ۴ | پیک سفارش را از فروشگاه تحویل گرفت و در راه است. | مراحل + کارت پیک + **تلفن/تماس** + نقشه‌ی سه‌نقطه‌ای + **کد تحویل** |
| ۵ | سفارش تحویل داده شد. | مراحل (همه سبز)؛ کارت پیک و نقشه **نیست**؛ پرسش «آیا تحویل گرفتید؟» همان رفتار فعلی صفحه |
| ۶ | ارسال لغو شد. | مراحل + مرحله‌ی قرمز |
| ۷ | ارسال با مشکل روبه‌رو شد / کالا در حال بازگشت به فروشگاه است. | مراحل + مرحله‌ی قرمز؛ دکمه‌ی «تماس با پشتیبانی» (از قبل در صفحه هست) |

### ۵.۸) حالت‌های خاص

- **لودینگ:** تا اولین پاسخ، بخش را نشان ندهید (اسکلتون صفحه‌ی اصلی کافی است). بعد از آن polling بی‌صداست؛ هیچ اسپینری نباید هر ۱۵ ثانیه چشمک بزند.
- **`courier == null` در وضعیت ۲/۳:** خطا نیست؛ فقط مراحل.
- **نبود `store`/`destination`:** نقشه را فقط با نقاطی که هست بکشید؛ اگر فقط پیک هست با `center` خودش و zoom ۱۴.
- **`locationStale`:** نشانگر خاکستری/نیمه‌شفاف و متن «آخرین موقعیت معلوم».
- **چند فروشگاه:** هر مرسوله کارت جداست (هر فروشگاه پیک خودش را دارد)؛ کد تحویل هم برای هر کدام جدا.
- **قطع اینترنت:** آخرین داده نمایش داده می‌شود؛ پولینگ بعدی خودش جبران می‌کند؛ پیام خطا لازم نیست.
- **دسترسی:** همه‌ی دکمه‌ها `min-h-10`؛ جهت `rtl`؛ ارقام فارسی (`toLocaleString('fa-IR')`)؛ ساعت‌ها به وقت تهران.

## ۶) بک‌آفیس فروشنده‌ی داخل وب‌اپ — `app/pages/sellerProfile/orders/index.vue`

### ۶.۱) داده

لیست سفارش‌ها از `/api/seller/sellerOrders` می‌آید و هر ردیف (`item`) یک سفارش است که `item.productOrderStores?.[0]` همان سفارش فروشگاه جاری است (همین الگو در مودال `currentOrder.productOrderStores?.[0]` استفاده شده).

- **شناسه‌ی لازم:** `const pos = item.productOrderStores?.[0]` ← `pos.id` همان `productOrderStoreId` است.
- ⚠️ لیست فروشنده مرسوله (`Shipment`) را **لود نمی‌کند**؛ پس `pos.shipmentStatus` در لیست `null` است. دکمه را با `Number(pos?.shippingProvider) === 4 && item.isPaid` نشان دهید و وضعیت را از خود API ردیابی بگیرید.

### ۶.۲) دکمه روی کارت سفارش

در کارت سفارش (زیر بلوک `deliveryBadge`، داخل همان `v-for="item in orderList"`):

```vue
<button
  v-if="item.isPaid && Number(item.productOrderStores?.[0]?.shippingProvider) === 4"
  type="button"
  class="col-span-2 flex h-11 items-center justify-center gap-2 rounded-xl border border-primary/30 bg-primary/10 text-[11px] font-bold text-primary active:scale-95"
  @click="openCourierTracking(item)"
>
  <Icon name="solar:delivery-bold-duotone" size="18" />ردیابی پیک
</button>
```

### ۶.۳) مودال و منطق

از همان الگوی مودال‌های فعلی صفحه (`orderModal`, `statusModal` …) پیروی کنید؛ این‌ها فقط منطق و محتوا:

```js
const courierModal = ref(null)          // ref مودال (مثل بقیه)
const courierTrack = ref(null)
const courierLoading = ref(false)
let courierTimer = null

const stopCourierPoll = () => { if (courierTimer) { clearInterval(courierTimer); courierTimer = null } }
const fetchCourierTrack = async (pos) => {
  const storeId = sellerStore.currentStore?.id || sellerStore.currentStore?.storeId
  try {
    const res = await $fetch('/api/seller/shipmentTracking', { query: { storeId, productOrderStoreId: pos.id } })
    courierTrack.value = res?.isSuccess ? res.data : null
    if (!res?.isSuccess) toast.add({ title: res?.messages?.[0]?.item1 || 'ردیابی در دسترس نیست', color: 'orange' })
    if (courierTrack.value?.isFinal) stopCourierPoll()
  } catch (e) { /* قطع لحظه‌ای */ }
}
const openCourierTracking = async (item) => {
  const pos = item.productOrderStores?.[0]
  if (!pos) return
  courierTrack.value = null
  courierLoading.value = true
  courierModal.value?.open?.()          // یا همان روش باز کردن مودال‌های دیگر این صفحه
  await fetchCourierTrack(pos)
  courierLoading.value = false
  stopCourierPoll()
  if (courierTrack.value && !courierTrack.value.isFinal) {
    const ms = Math.max(10, Number(courierTrack.value.pollSeconds) || 15) * 1000
    courierTimer = setInterval(() => { if (document.visibilityState === 'visible') fetchCourierTrack(pos) }, ms)
  }
}
const closeCourierTracking = () => { stopCourierPoll(); courierTrack.value = null }
onBeforeUnmount(stopCourierPoll)        // کنار پاک‌سازی‌های فعلی
```

محتوای مودال (فقط خواندنی):
- `headline` بالا؛ لیست `steps` مثل بخش ۵.۴.
- اگر `courierTrack.courier`: نام **کامل**، دکمه‌ی **تماس** (فقط اگر `canCall`)، «`distanceKm` کیلومتر تا فروشگاه»، و نقشه‌ی `Mapir` با دو نشانگر `store` (origin) و پیک. **`destination` هرگز نمی‌آید.**
- وقتی `status = 4` به بعد، `courier = null` است؛ به‌جای کارت پیک بنویسید «پیک سفارش را تحویل گرفت».
- بستن مودال ⇒ `closeCourierTracking()`.

### ۶.۴) بعد از «درخواست پیک» (اکسپرس)

در `requestCourier` بعد از موفقیت و `await loadAwaiting()` کاربر را به ردیابی ببرید: با `a.productOrderStoreId` (همان که در `awaiting` هست) `fetchCourierTrack({ id: a.productOrderStoreId })` را صدا بزنید و مودال را باز کنید. (اختیاری ولی تجربه را خیلی بهتر می‌کند.)

### ۶.۵) متن‌های کهنه‌ی کارت «در انتظار درخواست پیک» — باید اصلاح شود

میاره `pickup.deadline` کمتر از **۳۰ دقیقه** را رد می‌کند (`403 deadline_too_soon`)؛ سرور حالا حداقل ۳۰ دقیقه می‌گذارد و در `plannedPickupUtc` برمی‌گرداند. در همین فایل:

| محل | متن فعلی | جایگزین |
|---|---|---|
| کامنت بالای کارت (خط ≈ ۲۱) | «اکسپرس با مهلت ۲۰ دقیقه» | «اکسپرس با مهلت درخواست پیک» |
| `<p v-if="a.isExpress">` (خط ≈ ۳۴–۳۶) | «پیک حدود ۱۵ دقیقه بعد می‌رسد. تحویل به مشتری تا {{ hhmm(a.slotEndUtc) }}» | «پیک حدود {{ hhmm(a.plannedPickupUtc) }} می‌رسد.» (فیلد `plannedPickupUtc` در پاسخ `Awaiting` هست) |
| `confirm(...)` در `requestCourier` (خط ≈ ۷۴۳) | «پیک حدود ۱۵ دقیقه‌ی دیگر می‌رسد.» | «کالا بسته‌بندی شده و آماده‌ی تحویل به پیک است؟» (بدون عدد) |

قواعد لیست «در انتظار» (جایگزینی کامل لیست، تازه‌سازی بعد از Ready، حذف کارت بعد از موفقیت و …) در [MIARE_SELLER_APP_AWAITING_LIST_FA.md](MIARE_SELLER_APP_AWAITING_LIST_FA.md) آمده و وب‌اپ از قبل بیشترشان را دارد؛ فقط **فیلتر `.filter((x) => x.isExpress)`** در `loadAwaiting` سفارش‌های غیراکسپرس را پنهان می‌کند — اگر عمداً نیست، بپرسید.

## ۷) داده‌ی آزمایشی برای توسعه (بدون میاره‌ی واقعی)

تا وقتی میاره‌ی Production وصل نیست داده‌ی واقعی نمی‌آید. برای ساخت UI، در کنسول مرورگر (روی صفحه‌ی سفارش) پاسخ BFF را جعل کنید:

```js
const realFetch = window.$fetch
// نمونه: وضعیت ۴ با پیک، موقعیت و فاصله
window.__mockTrack = { isSuccess: true, code: 0, messages: [], data: [{ shipmentId: 1, productOrderStoreId: 456, storeId: 5, storeName: 'پی کی پت', provider: 'Miare', status: 4, statusKey: 'PickedUp', isExpress: true, isFinal: false, isLive: true, currentStepKey: 'picked_up', headline: 'پیک سفارش را از فروشگاه تحویل گرفت و در راه است.', steps: [], courier: { name: 'علی', phone: '09120000000', canCall: true, hasLocation: true, latitude: 35.7101, longitude: 51.4102, locationUpdatedAtUtc: new Date().toISOString(), locationAgeSeconds: 20, locationStale: false, distanceKm: 3.4, distanceTarget: 'destination' }, store: { latitude: 35.70, longitude: 51.39 }, destination: { latitude: 35.73, longitude: 51.42 }, plannedPickupUtc: new Date().toISOString(), deliveredAtUtc: null, lastUpdateUtc: new Date().toISOString(), pollSeconds: 15 }] }
```

سپس موقتاً در `loadTracking` به‌جای `$fetch` از `window.__mockTrack` استفاده کنید (و قبل از commit برگردانید). یا در BFF یک `if (process.env.NUXT_MOCK_TRACKING)` موقت بگذارید. **mock را commit نکنید.**
برای هر وضعیت ۲، ۳، ۴، ۵، ۶، ۷ و حالت `courier: null` یک پاسخ بسازید؛ جدول بخش ۴ تعیین می‌کند هر فیلد کی `null` باشد.

## ۸) چک‌لیست تست (بعد از دیپلوی بک و اتصال میاره‌ی Production)

مشتری:
1. سفارش اکسپرس پرداخت‌شده ← صفحه‌ی `/reserve/order-{id}`: بخش «پیگیری ارسال» با مرحله‌ی «فروشگاه در حال آماده‌سازی».
2. فروشنده «درخواست پیک» بزند ← حداکثر ۱۵ ثانیه بعد مرحله‌ی «پیک درخواست شد».
3. پیک تعیین شد ← کارت پیک با **نام کوچک**، **بدون تلفن**، نقشه با فروشگاه + پیک، فاصله تا فروشگاه.
4. پیک کالا را گرفت ← تلفن و دکمه‌ی تماس ظاهر شود، نقشه با سه نقطه، «n کیلومتر تا شما»، **کارت کد تحویل**.
5. تحویل ← مراحل کامل، کارت پیک و نقشه ناپدید، polling متوقف (در تب Network درخواست جدید نیاید).
6. تب را پس‌زمینه ببرید ← درخواستی ارسال نشود؛ برگردید ← فوراً تازه شود.
7. سفارش یک کاربر دیگر را با لینک باز کنید ← بخش ردیابی نباید ظاهر شود و هیچ داده‌ای نشت نکند.
8. سفارش پستی ← بخش ردیابی نباشد.
9. لغو سفارش/ناموفق ← مرحله‌ی قرمز، بدون نقشه.
10. سفارش دو فروشگاهی ← دو کارت جدا، دو کد تحویل جدا.

فروشنده:
11. کارت سفارش میاره ← دکمه‌ی «ردیابی پیک»؛ سفارش پستی ← دکمه نباشد.
12. وضعیت ۳: نام کامل، تلفن، فاصله تا فروشگاه، نقشه با دو نقطه؛ **هیچ‌جا آدرس/مختصات مشتری نباشد**.
13. وضعیت ۴ به بعد: «پیک تحویل گرفت»؛ بدون تلفن/موقعیت.
14. بستن مودال ← polling قطع شود.
15. سفارش فروشگاه دیگر (دستکاری شناسه) ← پیام «یافت نشد».
16. متن‌های «۱۵/۲۰ دقیقه» از کارت «درخواست پیک» رفته باشد و ساعت `plannedPickupUtc` نشان داده شود.

## ۹) حریم خصوصی و امنیت (الزامی)

- اطلاعات پیک (نام، تلفن، مختصات) را در Pinia persist/`localStorage`/cookie/`sessionStorage`/لاگ کنسول/Sentry/آنالیتیکس **نگذارید**.
- هیچ‌وقت شماره‌ی پیک را به فروشنده نشان ندهید جز وضعیت ۳ و به مشتری جز وضعیت ۴ — **سرور همین را اعمال می‌کند**؛ UI فقط آنچه می‌آید را نشان دهد و هیچ‌چیز را از جای دیگر (مثلاً از سفارش) حدس نزند.
- کد تحویل فقط روی صفحه‌ی خود مشتری و بعد از وضعیت ۴؛ در URL/پوش/اشتراک‌گذاری نباشد.
- `tel:` فقط وقتی `canCall`.
- درخواست‌ها فقط از طریق BFF (توکن cookie سمت سرور)، نه مستقیم از مرورگر به `api.pastil.pet`.

## ۱۰) بعد از دیپلوی: چک سریع

- `GET /api/shipmentTracking?productOrderId=…` با توکن مشتری همان سفارش داده بدهد؛ با توکن مشتری دیگر `data: []`.
- اگر `courier` همیشه `null` است ولی وضعیت ۳/۴ شده: مشکل از سمت بک (نام فیلدهای وب‌هوک میاره) است، نه UI؛ به بک‌اند خبر دهید (متن خام وب‌هوک در پنل ادمین، صفحه‌ی سفارش، «مراحل ارسال» دیده می‌شود).
- اگر `distanceKm` و `store`/`destination` نیست: مختصات فروشگاه یا آدرس مشتری ثبت نشده.
