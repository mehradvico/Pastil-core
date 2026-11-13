# دیدگاه کالا فقط بعد از «تحویل گرفتم» — مستند وب‌اپ

> مخاطب: تیم وب‌اپ (Nuxt 4) · وضعیت بک: **پیاده و build شد، دیپلوی نشده** (هیچ migration/SQL لازم نیست).
> پاسخ‌های بک **همیشه HTTP 200** با `{ isSuccess, messages, data }` (به‌جز دو endpoint جدید پایین که خودِ داده را برمی‌گردانند). فیلدها camelCase. موفقیت را از `isSuccess` بخوانید.
> قاعده‌ی قبلی تحویل‌گیری: [PRODUCT_ORDER_USER_DELIVERY_FA.md](PRODUCT_ORDER_USER_DELIVERY_FA.md) و [PRODUCT_ORDER_USER_DELIVERY_WEBAPP_FA.md](PRODUCT_ORDER_USER_DELIVERY_WEBAPP_FA.md).

## ۱) هدف

- فرم ثبت دیدگاه برای کالا **از صفحه‌ی کالا برداشته شود**.
- مشتری فقط وقتی سفارشش را **«تحویل گرفتم»** زده (یا سیستم بعد از ۷ روز خودکار ثبت کرده)، در **صفحه‌ی جزئیات همان سفارش** زیر هر کالا دکمه‌ی «ثبت دیدگاه» ببیند و بتواند امتیاز + متن بگذارد.
- نمایش دیدگاه‌های تأییدشده‌ی دیگران در صفحه‌ی کالا **سر جایش می‌ماند** (فقط خواندن).

## ۲) تغییر بک (انجام شد)

| مورد | شرح |
| --- | --- |
| `POST /api/EndUser/ProductComment` | **سخت‌گیر شد.** قبلاً هر کاربر لاگین‌شده روی هر کالایی دیدگاه می‌گذاشت. حالا باید: (الف) سفارشِ **خودِ کاربر**، پرداخت‌شده، لغو‌نشده و با `userReceived == true` (یا وضعیت قدیمیِ «تحویل داده شد») حاوی آن کالا باشد؛ (ب) برای آن کالا قبلاً دیدگاه **ردنشده** نگذاشته باشد (یک دیدگاه برای هر کالا؛ اگر ادمین ردش کرده بود دوباره می‌تواند). دیدگاه جدید مثل قبل با وضعیت «در انتظار بررسی» ثبت می‌شود و بعد از تأیید ادمین در لیست عمومی دیده می‌شود. |
| امن‌سازی همان endpoint | `id` و `likeCount/disLikeCount` که کلاینت می‌فرستاد نادیده گرفته می‌شود (صفر). |
| `GET /api/EndUser/ProductComment/Eligibility?productId=` | جدید، **نیاز به توکن**. وضعیت یک کالا برای کاربر. |
| `GET /api/EndUser/ProductComment/OrderEligibility?orderId=` | جدید، **نیاز به توکن**. وضعیت همه‌ی کالاهای یک سفارش. |
| `GET /api/EndUser/ProductComment` (لیست) | بدون تغییر، عمومی. |

### پیام‌های خطای POST (در `messages[0].item1` به فارسی)

| وضعیت | پیام |
| --- | --- |
| کالا در سفارش تحویل‌گرفته‌شده‌ی کاربر نیست | «ثبت دیدگاه برای کالا فقط بعد از تحویل گرفتن سفارشی که شامل آن کالا است امکان‌پذیر است.» |
| قبلاً دیدگاه ثبت کرده | «شما قبلاً برای این کالا دیدگاه ثبت کرده‌اید.» |
| امتیاز خارج از ۱ تا ۵ | «بازه‌ی وارد شده صحیح نیست» (مثل قبل) |
| متن خالی | خطای فیلد `Text` (مثل قبل) |

> نکته: وب‌اپ همین‌الان `rate: 0` می‌فرستد اگر کاربر ستاره نزند و بک `0` را «خارج از بازه» رد می‌کند. در فرم جدید **ستاره را اجباری کنید** (دکمه تا `rate >= 1` غیرفعال بماند).

## ۳) قرارداد دو endpoint جدید

### الف) `GET /api/EndUser/ProductComment/OrderEligibility?orderId={order.id}`

`orderId` همان `order.id` (رشته‌ی guid) است، نه `orderCode`. پاسخ **آرایه‌ی ساده** است (نه `{isSuccess,data}`):

```json
[
  { "productId": 812, "received": true, "hasCommented": false, "canComment": true },
  { "productId": 977, "received": true, "hasCommented": true,  "canComment": false }
]
```

- سفارش مال کاربر دیگری / تحویل‌نگرفته / لغوشده / پرداخت‌نشده ⇒ `[]`.
- فقط کالاهای همان سفارش برمی‌گردند، هر `productId` یک‌بار (حتی اگر چند واریانت داشته باشد).
- `canComment == true` ⇒ فرم ثبت باز شود. `hasCommented == true` ⇒ برچسب «دیدگاه شما ثبت شد».

### ب) `GET /api/EndUser/ProductComment/Eligibility?productId={id}`

پاسخ یک شیء: `{ "productId": 812, "received": true, "hasCommented": false, "canComment": true }`. برای صفحه‌ی کالا (بخش ۶، اختیاری).

## ۴) BFF جدید در وب‌اپ

دو فایل کنار `app/server/api/comments/comment.post.js`:

**`app/server/api/comments/orderEligibility.get.js`**
```js
export default defineEventHandler(async (event) => {
  const { public: { baseUrl } } = useRuntimeConfig()
  const token = getCookie(event, 'token')
  try {
    return await $fetch(`${baseUrl}/api/EndUser/ProductComment/OrderEligibility`, {
      method: 'GET',
      headers: { Accept: 'application/json', Authorization: `Bearer ${token}` },
      query: getQuery(event), // { orderId }
    })
  } catch (error) {
    return [] // خطا/۴۰۱ = هیچ دکمه‌ای نشان نده
  }
})
```

**`app/server/api/comments/eligibility.get.js`** (فقط اگر بخش ۶ را می‌خواهید)
```js
export default defineEventHandler(async (event) => {
  const { public: { baseUrl } } = useRuntimeConfig()
  const token = getCookie(event, 'token')
  try {
    return await $fetch(`${baseUrl}/api/EndUser/ProductComment/Eligibility`, {
      method: 'GET',
      headers: { Accept: 'application/json', Authorization: `Bearer ${token}` },
      query: getQuery(event), // { productId }
    })
  } catch (error) {
    return { canComment: false, received: false, hasCommented: false }
  }
})
```

BFFِ `comment.post.js` **همان می‌ماند** (فقط `text`, `rate`, `productId` می‌فرستد).

## ۵) تغییر UI در وب‌اپ

### ۵-۱) صفحه‌ی جزئیات سفارش — `app/pages/reserve/order-[orderId].vue`

این همان صفحه‌ای است که `/orders/[orderId]` به آن ریدایرکت می‌شود. کالاها در حلقه‌ی `v-for="(item, itemIndex) in storeItems(storeOrder)"` (حدود خط ۲۰۱) رندر می‌شوند.

**۱. state و لود مجوزها** (در `<script setup>`، کنار `loadOrder`):

```js
// دیدگاه کالا: productId → { canComment, hasCommented }
const reviewState = ref({})
const reviewTarget = ref(null)          // { productId, name } کالایی که فرمش باز است
const reviewText = ref('')
const reviewRate = ref(0)
const reviewSending = ref(false)
const reviewModal = ref(null)

const itemProductId = (item) =>
  Number(item?.productItem?.productId ?? item?.productItem?.product?.id ?? 0) || 0

const loadReviewState = async () => {
  const id = orderInfo.value?.id ?? orderInfo.value?.Id
  // فقط وقتی کاربر «تحویل گرفتم» زده؛ وگرنه اصلاً درخواست نزن
  if (!id || orderInfo.value?.userReceived !== true) { reviewState.value = {}; return }
  try {
    const list = await $fetch('/api/comments/orderEligibility', { query: { orderId: id } })
    reviewState.value = Object.fromEntries((Array.isArray(list) ? list : []).map(x => [x.productId, x]))
  } catch { reviewState.value = {} }
}
```

`loadReviewState()` را **بعد از هر `loadOrder`** صدا بزنید: آخر تابع `loadOrder` (داخل `try` بعد از `stores.value = ...`) اضافه کنید `await loadReviewState()`. به‌این‌ترتیب بعد از «تحویل گرفتم» (`@updated="loadOrder(true)"` روی `StoreOrderDeliveryConfirm`) دکمه‌ها خودکار ظاهر می‌شوند. `refreshLive` وقتی `userReceived === true` متوقف می‌شود، پس بعد از ثبت دیدگاه خودمان state را دستی به‌روز می‌کنیم (پایین).

> شرط `userReceived !== true`: وضعیت‌های قدیمی «تحویل داده شد» بدون `userReceived` هم در بک مجازند، اما سفارش‌های جدید همه `userReceived=true` دارند. اگر می‌خواهید سفارش‌های قدیمی هم دکمه داشته باشند شرط را به `orderInfo.value?.userReceived === true || orderInfo.value?.productOrderStatus?.label === 'ProductOrderStatus_Delivered'` تغییر دهید.

**۲. دکمه زیر هر کالا** — داخل `<div class="min-w-0 flex-1">` آیتم، بعد از ردیف قیمت (`<div class="mt-2 flex items-center justify-between">…</div>`):

```vue
<template v-if="reviewState[itemProductId(item)]">
  <button
    v-if="reviewState[itemProductId(item)].canComment"
    type="button"
    class="mt-2 flex h-10 w-full items-center justify-center gap-1.5 rounded-lg border border-primary bg-primary/10 text-[11px] font-bold text-primary transition-all active:scale-95"
    @click="openReview(item)"
  >
    <Icon name="solar:chat-round-like-linear" size="15" />ثبت دیدگاه برای این کالا
  </button>
  <p
    v-else-if="reviewState[itemProductId(item)].hasCommented"
    class="mt-2 flex items-center gap-1 text-[11px] text-green-500"
  >
    <Icon name="solar:check-circle-linear" size="14" />دیدگاه شما ثبت شد و پس از تأیید نمایش داده می‌شود
  </p>
</template>
```

برای سفارشِ تحویل‌نگرفته `reviewState` خالی است ⇒ **هیچ چیزی نمایش داده نمی‌شود** (همان خواسته).

**۳. مودال فرم** (انتهای `<template>` صفحه، کنار بقیه‌ی `BottomModal`ها؛ همان کامپوننت و الگوی `OrderDeliveryConfirm.vue`):

```vue
<BottomModal ref="reviewModal">
  <template #header>
    <div class="flex w-full items-center justify-between border-b border-border/60 pb-4">
      <span class="text-sm font-bold text-text-primary">دیدگاه شما درباره‌ی «{{ reviewTarget?.name }}»</span>
      <button type="button" aria-label="بستن" class="flex h-8 w-8 items-center justify-center rounded-full border border-border bg-surface text-text-secondary" @click="reviewModal?.closeModal()">
        <Icon name="solar:close-circle-bold" size="18" />
      </button>
    </div>
  </template>
  <template #body>
    <div class="space-y-4 py-4 text-right">
      <div class="flex flex-col gap-1">
        <span class="text-[11px] text-text-secondary">امتیاز شما به کالا</span>
        <Rate dir="rtl" id="order-product-rate" v-model="reviewRate" />
      </div>
      <div class="flex flex-col rounded-lg border border-border bg-bg px-4 py-2.5 transition-all focus-within:border-primary">
        <textarea v-model="reviewText" rows="4" maxlength="1000"
          placeholder="تجربه‌ی خود از این کالا را بنویسید (مثلاً کیفیت، بسته‌بندی، مناسب بودن برای حیوان‌تان)..."
          class="min-h-[100px] w-full resize-none bg-transparent text-xs text-text-primary outline-none"></textarea>
      </div>
      <button type="button" :disabled="reviewSending || !reviewText.trim() || reviewRate < 1"
        class="flex h-14 w-full items-center justify-center gap-2 rounded-xl border border-primary bg-primary/10 text-xs font-bold text-primary transition-all active:scale-95 disabled:opacity-35"
        @click="sendReview">
        <Icon v-if="reviewSending" name="svg-spinners:ring-resize" size="18" />ثبت دیدگاه
      </button>
    </div>
  </template>
</BottomModal>
```

**۴. توابع باز کردن و ارسال:**

```js
const openReview = (item) => {
  reviewTarget.value = { productId: itemProductId(item), name: itemName(item) }
  reviewText.value = ''
  reviewRate.value = 0
  nextTick(() => reviewModal.value?.openModal())
}

const sendReview = async () => {
  if (reviewSending.value || !reviewTarget.value) return
  const toast = useToast()
  reviewSending.value = true
  try {
    const res = await $fetch('/api/comments/comment', {
      method: 'POST',
      body: { text: reviewText.value.trim(), rate: reviewRate.value, productId: reviewTarget.value.productId },
    })
    if (res?.isSuccess) {
      toast.add({ title: 'دیدگاه شما ثبت شد و پس از بررسی نمایش داده می‌شود', color: 'green' })
      reviewModal.value?.closeModal()
      await loadReviewState()           // دکمه → «دیدگاه شما ثبت شد»
    } else {
      toast.add({ title: res?.messages?.[0]?.item1 || 'ثبت دیدگاه ناموفق بود', color: 'red' })
      await loadReviewState()           // اگر مجوز عوض شده بود (مثلاً قبلاً ثبت شده) دکمه درست شود
    }
  } catch {
    toast.add({ title: 'خطا در ارتباط با سرور', color: 'red' })
  } finally {
    reviewSending.value = false
  }
}
```

### ۵-۲) (اختیاری) لیست سفارش‌ها — `app/pages/reserve/index.vue`

در کارت سفارش‌های `userReceived === true` می‌توان یک نشان «ثبت دیدگاه کالا» گذاشت که به `/reserve/order-{id}` برود. نیازی به endpoint جدید نیست و فعلاً ضروری نیست.

### ۵-۳) صفحه‌ی کالا — `app/components/products/comments.vue`

1. **کل بلوک فرم** `<div class="mt-8 border-t border-border/30 pt-6 space-y-4">…</div>` (عنوان «ثبت دیدگاه جدید برای این کالا»، `textarea`، `Rate`، دکمه‌ی «ثبت دیدگاه») را حذف کنید.
2. به‌جایش فقط یک راهنمای کوتاه بگذارید:
   ```vue
   <p class="mt-6 flex items-start gap-2 rounded-xl border border-border/40 bg-bg p-3 text-[11px] leading-6 text-text-secondary">
     <Icon name="solar:info-circle-linear" size="14" class="mt-1 shrink-0" />
     دیدگاه فقط برای خریدارانی که سفارش را تحویل گرفته‌اند ثبت می‌شود؛ از بخش «سفارش‌های من» می‌توانید دیدگاه بگذارید.
   </p>
   ```
3. از `<script setup>` کدهای بی‌مصرف را پاک کنید: `rate`, `commentText`, `loading2`, `allowToSend`, تابع `sendComment`.
4. لیست دیدگاه‌ها، «مشاهده همه»، لایک/دیسلایک (`likeComent`) **دست نخورد**.
5. بخش «دیدگاه‌ها» الان فقط وقتی `comments?.list?.length > 0` است نمایش داده می‌شود؛ پس برای کالای بدون دیدگاه فقط همان راهنما می‌ماند (یا اگر نمی‌خواهید، کل کامپوننت را با `v-if="comments?.list?.length"` در `pages/store/products/[id]/index.vue` خط ~۳۴۲ بپیچید).

### ۵-۴) صفحه‌ی کالا — `pages/store/products/[id]/index.vue`

تغییری لازم نیست (`getComments` فقط لیست عمومی را می‌خواند).

## ۶) (اختیاری) تعبیه‌ی دکمه‌ی «ثبت دیدگاه» در صفحه‌ی کالا برای خریدارِ تحویل‌گرفته

اگر خواستید خریدار از خودِ صفحه‌ی کالا هم بتواند: در `comments.vue` بعد از لاگین `GET /api/comments/eligibility?productId=…` را بزنید و **فقط اگر `canComment === true`** همان فرم قدیمی را نشان دهید (کد فرم را نگه دارید و با `v-if="eligibility?.canComment"` بپیچید؛ بعد از ثبت موفق دوباره eligibility را بگیرید). این بخش خواسته‌ی اصلی نبود؛ اگر فقط «از بخش سفارش‌ها» مدنظر است حذف کامل فرم (بخش ۵-۳) کافی است.

## ۷) وب‌سایت عمومی (`website/`)

`website/app/components/products/detail/ProductReviews.vue` هم مستقیم به `POST /api/EndUser/ProductComment` می‌زند. بعد از دیپلوی بک، برای غیرخریدار با همان پیام خطای بالا رد می‌شود (بدون کرش). توصیه: فرم همان‌جا هم حذف یا با پیام «دیدگاه فقط برای خریداران تحویل‌گرفته» جایگزین شود.

## ۸) چک‌لیست تست

1. کاربر بدون سفارش → در صفحه‌ی کالا فرمی نیست؛ POST مستقیم ⇒ `isSuccess=false` با پیام «فقط بعد از تحویل گرفتن…».
2. سفارش «ارسال‌شده» و تحویل‌نگرفته → در صفحه‌ی سفارش هیچ دکمه‌ی دیدگاهی نیست؛ `OrderEligibility` ⇒ `[]`.
3. «تحویل گرفتم» بزنید → بدون رفرش، زیر هر کالا «ثبت دیدگاه برای این کالا» می‌آید.
4. ثبت با امتیاز ۴ و متن ⇒ toast موفق؛ دکمه تبدیل به «دیدگاه شما ثبت شد…»؛ در پنل ادمین، دیدگاه «در انتظار بررسی» است و پس از تأیید در صفحه‌ی کالا می‌آید.
5. ثبت دوباره برای همان کالا (حتی از سفارش دیگر) ⇒ «شما قبلاً برای این کالا دیدگاه ثبت کرده‌اید.» اگر ادمین رد کرده باشد دوباره مجاز است.
6. سفارشِ لغوشده یا پرداخت‌نشده ⇒ هیچ دکمه‌ای.
7. کاربر B با `orderId` کاربر A ⇒ `[]`.
8. سفارش چند کالا: برای هر کالا جدا دکمه/وضعیت.

## ۹) دیپلوی

فقط **Api** (بدون SQL/migration). تا دیپلوی Api، دو endpoint جدید ۴۰۴ می‌دهند؛ BFF بالا در این حالت `[]` برمی‌گرداند و هیچ دکمه‌ای نمایش داده نمی‌شود (ایمن). **ترتیب پیشنهادی: اول Api، بعد وب‌اپ.** اگر وب‌اپ قبل از Api بیاید فرم صفحه‌ی کالا برداشته شده ولی دکمه‌ی سفارش هنوز کار نمی‌کند.
