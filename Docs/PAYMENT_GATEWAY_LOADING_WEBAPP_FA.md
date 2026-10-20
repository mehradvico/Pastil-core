# وب‌اپ: صفحه‌ی «در حال رفتن به صفحه پرداخت» + پرداخت دوباره بعد از برگشت از درگاه

**مخاطب:** تیم وب‌اپ (app.pastil.pet). بک‌اند این تغییرات **انجام و build شده** (۲۰۲۶-۱۰-۱۲)؛ این سند فقط کارهای وب‌اپ است.
**طراحی بک‌اند:** [`PAYMENT_RETRY_AND_ALREADY_PAID_FA.md`](PAYMENT_RETRY_AND_ALREADY_PAID_FA.md)

---

## ۰. مشکل و نتیجه‌ی مطلوب

گزارش کاربر: «اصلاً به درگاه نرفتم» و بعد با زدن دوباره‌ی «پرداخت» این پیام را دید:
`خطا در ثبت مشاوره — پرداخت شروع شده و اطلاعات مالی قابل تغییر نیست.`

دو علت داشت:

1. **بک‌اند** بعد از هر تلاش درگاه (حتی اگر کاربر هیچ‌وقت به درگاه نرسیده بود) تا ۱۵ دقیقه خرید مشاوره را قفل می‌کرد، و در کلینیک/پانسیون/مدرسه/سفر هم تغییر کیف پول/کد تخفیف را **برای همیشه** قفل می‌کرد. ✅ در بک‌اند رفع شد.
2. **وب‌اپ** بین زدن «پرداخت» و باز شدن درگاه (زمان ساخت پرداخت + درخواست به زرین‌پال) تقریباً هیچ بازخوردی نشان نمی‌دهد؛ کاربر فکر می‌کند چیزی نشد، Back می‌زند یا دوباره می‌زند. ⬅️ کار این سند.

بعد از این تغییرات:

| وضعیت | رفتار |
|---|---|
| کاربر «پرداخت» را می‌زند | **فوراً** صفحه‌ی تمام‌صفحه‌ی «در حال رفتن به صفحه پرداخت» (سبد خرید در حال حرکت + نوار پیشرفت) تا باز شدن درگاه |
| از درگاه بدون پرداخت برگشت (یا اصلاً به درگاه نرسید) | دوباره «پرداخت» را می‌زند ⇒ درگاه تازه باز می‌شود. **هیچ‌وقت** «پرداخت در جریان است» / «قبلاً رزرو شده» نمی‌بیند |
| قبلاً واقعاً پرداخت کرده | پیام «این رزرو/سفارش/مشاوره قبلاً پرداخت شده است» + انتقال خودکار به جزئیات همان مورد |
| پرداخت قبلی همین چند ثانیه پیش در درگاه انجام شده و تأیید بانک در جریان است | پیام کوتاه «پرداخت قبلی شما همین الان در حال تأیید نهایی بانک است؛ چند ثانیه دیگر دوباره تلاش کنید.» |

---

## ۱. قرارداد تازه‌ی بک‌اند (چیزی که وب‌اپ به آن تکیه می‌کند)

همه‌ی مسیرهای پرداخت مثل قبل HTTP 200 با `{ isSuccess, code, messages, data }` برمی‌گردند. سه حالت تازه:

### ۱.۱ «قبلاً پرداخت/خریداری شده» — `code: 20`

```jsonc
{
  "isSuccess": false,
  "code": 20,
  "messages": [{ "item1": "این رزرو قبلاً پرداخت شده است.", "item2": "" }],
  "data": { "callBackTypeLabel": "CompanionReserve", "callBackId": "1234" }
}
```

`callBackTypeLabel` ⇒ مسیر جزئیات در وب‌اپ:

| callBackTypeLabel | مسیر |
|---|---|
| `CompanionReserve` | `/reserve/{id}` |
| `CompanionReserveBatch` | `/reserve` |
| `PansionReserve` | `/pansion/reserve-{id}` |
| `SchoolReserve` | `/school/reserve-{id}` |
| `ConsultationPurchase` | `/consultations/{id}` |
| `ProductOrder` | `/reserve/order-{id}` |
| `Trip` | `/trip/{id}` |
| `Insurance` / `Cargo` | (صفحه‌ی جزئیات ندارند) پیام را نشان دهید |

> **یک استثنا:** `POST /api/school/reserve` (ساخت ثبت‌نام مدرسه) در حالت پرداخت‌شده `code: 20` با `data` = **خودِ ثبت‌نام** (`SchoolReserveDto`، یعنی `data.id`) برمی‌گرداند، نه `callBackTypeLabel`. مسیر: `/school/reserve-{data.id}`.

پیام‌هایی که با `code: 20` می‌آیند (عیناً `messages[0].item1` را نشان دهید، متن ثابت ننویسید):
`این رزرو قبلاً پرداخت شده است.` · `این سفر قبلاً پرداخت شده است.` · `این درخواست کارگو قبلاً پرداخت شده است.` · `این بیمه قبلاً پرداخت شده است.` · `پرداخت این مورد قبلاً انجام شده است.` · `این مشاوره قبلاً خریداری شده است.` · `شما یک مشاوره‌ی آنلاین فعال دارید؛ تا پایان زمان آن نمی‌توانید مشاوره‌ی دیگری بخرید.` (این آخری هم حالا `code: 20` + شناسه‌ی همان مشاوره‌ی فعال دارد)

### ۱.۲ «تأیید بانک در جریان است» — بدون کد

`messages[0].item1 = "پرداخت قبلی شما همین الان در حال تأیید نهایی بانک است؛ چند ثانیه دیگر دوباره تلاش کنید."`
فقط چند ثانیه طول می‌کشد. با toast نارنجی نشان دهید؛ دکمه‌ی پرداخت را فعال بگذارید.

### ۱.۳ «لغو شده» — بدون کد

`این مورد لغو شده و دیگر قابل پرداخت نیست.` (قبلاً `اطلاعات وارد شده معتبر نیست` می‌آمد.)

### ۱.۴ رفتارهایی که دیگر خطا نمی‌دهند (لازم نیست وب‌اپ کاری بکند)

- خرید دوباره‌ی مشاوره (همان پکیج یا پکیج دیگر) بعد از برگشت از درگاه: خرید معلق قبلی خودکار لغو و ظرفیت ساعتش آزاد می‌شود.
- `PUT` کیف پول / کد تخفیف روی رزرو کلینیک، پانسیون، مدرسه، سفر پت‌رسان، کارگو، بیمه بعد از برگشت از درگاه: دیگر «پرداخت شروع شده…» نمی‌دهد. اگر پرداخت‌شده باشد `code: 20` می‌دهد.
- `POST /api/school/reserve` برای همان دوره + همان پت که ثبت‌نام **پرداخت‌نشده** دارد: دیگر «قبلا رزرو شده است» نمی‌دهد؛ **همان ثبت‌نام** با `isSuccess: true` برمی‌گردد (با قیمت فعلی دوره و بدون کیف پول/تخفیف قبلی). کد فعلی `ensureReservesCreated` بدون تغییر کار می‌کند.

---

## ۲. فایل‌های تازه

### ۲.۱ `app/components/payment/PaymentGatewayLoader.vue` (جدید)

انیمیشن: سبد خرید رو به چپ (جهت RTL) از راست وارد می‌شود، روی جاده حرکت می‌کند، چرخ‌ها می‌چرخند، جعبه‌ها تکان می‌خورند، خطوط سرعت پشتش است؛ زیرش نوار پیشرفت راست‌به‌چپ. **همه‌ی رنگ‌ها از توکن‌های `main.css`** ⇒ در `html.light` و تم تاریک خودکار درست است (در هر دو تم روی موبایل ۳۷۵px بررسی شد). `prefers-reduced-motion` هم رعایت شده.

```vue
<!-- app/components/payment/PaymentGatewayLoader.vue -->
<!-- لودینگ «در حال رفتن به صفحه پرداخت»: سبد خرید روی مسیر حرکت می‌کند + نوار پیشرفت.
     همه‌ی رنگ‌ها از توکن‌های تم (main.css) ⇒ در تم روشن (html.light) و تاریک خودکار درست است. -->
<template>
  <div class="pgl" role="status" aria-live="polite">
    <div class="pgl-stage" aria-hidden="true">
      <span class="pgl-track" />
      <div class="pgl-mover">
        <span class="pgl-speed"><i /><i /><i /></span>
        <svg class="pgl-cart" viewBox="0 0 72 58">
          <g class="pgl-cart-bob">
            <rect class="pgl-item pgl-item-a" x="22" y="5" width="13" height="14" rx="3" />
            <rect class="pgl-item pgl-item-b" x="37" y="1" width="11" height="18" rx="3" />
            <path class="pgl-basket-fill" d="M14 16H57.6L52.4 34H20Z" />
            <path class="pgl-basket" d="M57.6 16H14L20 34H52.4" />
            <path class="pgl-frame" d="M68 8H60L50 42H22" />
          </g>
          <g class="pgl-wheel"><circle cx="26" cy="49" r="5" /><path d="M26 46v6" /></g>
          <g class="pgl-wheel"><circle cx="46" cy="49" r="5" /><path d="M46 46v6" /></g>
        </svg>
      </div>
    </div>
    <div class="pgl-bar" role="progressbar" aria-valuemin="0" aria-valuemax="100" :aria-valuenow="Math.round(progress)">
      <span class="pgl-bar-fill" :style="{ width: `${progress}%` }" />
    </div>
    <p class="pgl-title">{{ title }}</p>
    <p v-if="subtitle" class="pgl-sub">{{ subtitle }}</p>
    <slot />
  </div>
</template>

<script setup>
defineProps({
  progress: { type: Number, default: 0 },
  title: { type: String, default: 'در حال رفتن به صفحه پرداخت' },
  subtitle: { type: String, default: 'چند لحظه صبر کنید؛ صفحه را نبندید.' },
})
</script>

<style scoped>
.pgl { width: 100%; max-width: 320px; display: flex; flex-direction: column; align-items: center; text-align: center; color: var(--color-text-primary); }
.pgl-stage { position: relative; width: 100%; height: 96px; overflow: hidden;
  -webkit-mask-image: linear-gradient(to left, transparent, #000 14%, #000 86%, transparent);
          mask-image: linear-gradient(to left, transparent, #000 14%, #000 86%, transparent); }
.pgl-track { position: absolute; inset-inline: 0; bottom: 14px; height: 2px; border-radius: 2px;
  background: repeating-linear-gradient(to left, color-mix(in srgb, var(--color-text-secondary) 45%, transparent) 0 14px, transparent 14px 26px); }
.pgl-mover { position: absolute; inset: 0; display: flex; align-items: flex-end; justify-content: center; padding-bottom: 12px;
  animation: pgl-drive 2.8s cubic-bezier(.45, .05, .55, .95) infinite; }
.pgl-cart { width: 84px; height: 68px; overflow: visible; }
.pgl-cart-bob { animation: pgl-bob .36s ease-in-out infinite alternate; }
.pgl-basket, .pgl-frame { stroke: var(--color-primary); stroke-width: 4; stroke-linecap: round; stroke-linejoin: round; fill: none; }
.pgl-basket-fill { fill: color-mix(in srgb, var(--color-primary) 16%, var(--color-bg)); }
.pgl-item { transform-box: fill-box; transform-origin: 50% 100%; }
.pgl-item-a { fill: var(--color-secondary); animation: pgl-jiggle .72s ease-in-out infinite alternate; }
.pgl-item-b { fill: color-mix(in srgb, var(--color-primary) 55%, var(--color-text-primary)); animation: pgl-jiggle .72s ease-in-out -.36s infinite alternate; }
.pgl-wheel { transform-box: fill-box; transform-origin: center; animation: pgl-spin .5s linear infinite; }
.pgl-wheel circle { fill: var(--color-bg); stroke: var(--color-primary); stroke-width: 3.5; }
.pgl-wheel path { stroke: var(--color-secondary); stroke-width: 2.5; stroke-linecap: round; }
.pgl-speed { position: absolute; bottom: 34px; display: flex; flex-direction: column; gap: 6px; transform: translateX(62px); }
.pgl-speed i { display: block; height: 3px; border-radius: 3px; background: color-mix(in srgb, var(--color-primary) 55%, transparent); animation: pgl-wind .5s ease-out infinite; }
.pgl-speed i:nth-child(1) { width: 18px; }
.pgl-speed i:nth-child(2) { width: 26px; animation-delay: .12s; }
.pgl-speed i:nth-child(3) { width: 14px; animation-delay: .24s; }
.pgl-bar { position: relative; width: 100%; height: 8px; margin-top: 6px; overflow: hidden; border-radius: 999px;
  background: color-mix(in srgb, var(--color-text-secondary) 18%, transparent); }
.pgl-bar-fill { position: absolute; inset-block: 0; right: 0; border-radius: inherit; transition: width .35s ease-out;
  background: linear-gradient(to left, var(--color-primary), #5cc8ff); }
.pgl-bar-fill::after { content: ""; position: absolute; inset: 0; border-radius: inherit;
  background: linear-gradient(to left, transparent, rgba(255, 255, 255, .45), transparent); background-size: 60px 100%; background-repeat: no-repeat;
  animation: pgl-shine 1.3s linear infinite; }
.pgl-title { margin: 18px 0 0; font-size: 15px; font-weight: 700; }
.pgl-sub { margin: 6px 0 0; font-size: 12px; line-height: 1.9; color: var(--color-text-secondary); }

@keyframes pgl-drive { 0% { transform: translateX(44%); } 100% { transform: translateX(-44%); } }
@keyframes pgl-bob { to { transform: translateY(-2.5px); } }
@keyframes pgl-jiggle { from { transform: rotate(-5deg); } to { transform: rotate(5deg); } }
@keyframes pgl-spin { to { transform: rotate(-360deg); } }
@keyframes pgl-wind { 0% { opacity: 0; transform: translateX(-8px); } 40% { opacity: 1; } 100% { opacity: 0; transform: translateX(10px); } }
@keyframes pgl-shine { from { background-position: 120% 0; } to { background-position: -60px 0; } }
@media (prefers-reduced-motion: reduce) {
  .pgl-mover, .pgl-cart-bob, .pgl-item, .pgl-wheel, .pgl-speed i, .pgl-bar-fill::after { animation: none; }
  .pgl-speed { display: none; }
}
</style>
```

### ۲.۲ `app/composables/usePaymentRedirect.ts` (جدید)

state ماژولی است (نه `useState`) تا بعد از `await` در هندلرها و داخل storeها بدون context نوکست کار کند؛ فقط سمت کلاینت تغییر می‌کند.

```ts
// app/composables/usePaymentRedirect.ts
// انتقال به درگاه با لودینگ تمام‌صفحه (PaymentRedirectOverlay) از لحظه‌ی زدن «پرداخت» تا باز شدن درگاه.
// state ماژولی و فقط سمت کلاینت تغییر می‌کند؛ روی سرور همیشه مقدار اولیه می‌ماند.
export interface PaymentRedirectMeta { amount?: number | null; code?: string | null }
export type GatewayRedirectResult = 'redirecting' | 'invalid-url' | 'cancelled'

export const paymentRedirectState = reactive({
  visible: false,
  phase: 'preparing' as 'preparing' | 'redirecting',
  amount: null as number | null,
  code: '',
  url: '',
  startedAt: 0,
})

// هر begin یک بلیت تازه می‌دهد؛ انصراف کاربر یا برگشت از درگاه بلیت را باطل می‌کند تا پاسخ دیرِ سرور کاربر را به درگاه نبرد
let ticket = 0

export const beginPaymentRedirect = (meta: PaymentRedirectMeta = {}): number => {
  if (!import.meta.client) return 0
  ticket += 1
  Object.assign(paymentRedirectState, {
    visible: true, phase: 'preparing', url: '', startedAt: Date.now(),
    amount: meta.amount ?? null, code: meta.code ?? '',
  })
  return ticket
}

export const cancelPaymentRedirect = () => {
  ticket += 1
  Object.assign(paymentRedirectState, { visible: false, phase: 'preparing', url: '' })
}

export const startGatewayRedirect = async (
  rawUrl: unknown,
  options: PaymentRedirectMeta & { ticket?: number } = {},
): Promise<GatewayRedirectResult> => {
  if (!import.meta.client) return 'cancelled'
  if (options.ticket !== undefined && options.ticket !== ticket) return 'cancelled'
  if (!isSafeGatewayUrl(rawUrl)) {
    cancelPaymentRedirect()
    return 'invalid-url'
  }
  const current = paymentRedirectState.visible ? ticket : beginPaymentRedirect(options)
  Object.assign(paymentRedirectState, {
    phase: 'redirecting',
    url: rawUrl,
    amount: options.amount ?? paymentRedirectState.amount,
    code: options.code ?? paymentRedirectState.code,
  })
  await new Promise(resolve => setTimeout(resolve, 450)) // نوار تا ۱۰۰٪ پر شود
  if (current !== ticket) return 'cancelled'
  window.location.assign(rawUrl)
  return 'redirecting'
}

// صفحه‌ای که با Back از درگاه از bfcache برمی‌گردد، باید دکمه‌ی پرداختش دوباره فعال شود
export const onPaymentPageReturn = (callback: () => void) => {
  const handler = (event: PageTransitionEvent) => { if (event.persisted) callback() }
  onMounted(() => window.addEventListener('pageshow', handler))
  onBeforeUnmount(() => window.removeEventListener('pageshow', handler))
}
```

### ۲.۳ `app/components/payment/PaymentRedirectOverlay.vue` (جدید)

نوار پیشرفت تا وقتی سرور جواب نداده آرام تا ۹۰٪ می‌رود و با رسیدن لینک درگاه ۱۰۰٪ می‌شود. اگر ۷ ثانیه بعد از شروع انتقال هنوز روی صفحه‌ایم (WebView بعضی گوشی‌ها) دکمه‌ی دستی «ورود به درگاه پرداخت» و «انصراف» ظاهر می‌شود؛ اگر خود سرور ۲۰ ثانیه جواب نداد، فقط «انصراف».

```vue
<!-- app/components/payment/PaymentRedirectOverlay.vue -->
<!-- لایه‌ی تمام‌صفحه‌ی «در حال رفتن به صفحه پرداخت»؛ یک‌بار در app.vue. کنترل با usePaymentRedirect.ts -->
<template>
  <Teleport to="body">
    <Transition name="pro-fade">
      <div v-if="state.visible" class="pro-root" dir="rtl">
        <PaymentGatewayLoader :progress="progress" :subtitle="subtitle">
          <p v-if="state.amount" class="pro-meta">
            مبلغ قابل پرداخت: <b>{{ Number(state.amount).toLocaleString('fa-IR') }}</b> تومان
            <span v-if="state.code" class="pro-code" dir="ltr">{{ state.code }}</span>
          </p>
          <div v-if="slow" class="pro-actions">
            <a v-if="state.url" :href="state.url" class="pro-btn pro-btn-primary">ورود به درگاه پرداخت</a>
            <button type="button" class="pro-btn" @click="cancelPaymentRedirect()">انصراف و بازگشت</button>
          </div>
        </PaymentGatewayLoader>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
const state = paymentRedirectState
const progress = ref(0)
const slow = ref(false)
let raf = 0
let slowTimer = 0

const subtitle = computed(() => (state.phase === 'redirecting'
  ? 'در حال باز کردن درگاه امن بانکی…'
  : 'چند لحظه صبر کنید؛ صفحه را نبندید.'))

const tick = () => {
  const elapsed = (Date.now() - state.startedAt) / 1000
  progress.value = state.phase === 'redirecting' ? 100 : Math.min(90, 90 * (1 - Math.exp(-elapsed / 1.6)))
  raf = requestAnimationFrame(tick)
}
const armSlow = (ms) => {
  clearTimeout(slowTimer)
  slow.value = false
  slowTimer = setTimeout(() => { slow.value = true }, ms)
}

watch(() => state.visible, (visible) => {
  cancelAnimationFrame(raf)
  clearTimeout(slowTimer)
  slow.value = false
  if (!visible) return
  progress.value = 0
  raf = requestAnimationFrame(tick)
  armSlow(20000)
})
watch(() => state.phase, (phase) => { if (state.visible && phase === 'redirecting') armSlow(7000) })

// Back از درگاه با bfcache: صفحه همان‌طور که رها شده بود برمی‌گردد ⇒ لایه باید بسته شود
const onPageShow = (event) => { if (event.persisted) cancelPaymentRedirect() }
onMounted(() => window.addEventListener('pageshow', onPageShow))
onBeforeUnmount(() => {
  window.removeEventListener('pageshow', onPageShow)
  cancelAnimationFrame(raf)
  clearTimeout(slowTimer)
})
</script>

<style scoped>
.pro-root { position: fixed; inset: 0; z-index: 10000; display: flex; align-items: center; justify-content: center; padding: 24px;
  background: color-mix(in srgb, var(--color-bg) 96%, transparent); backdrop-filter: blur(4px); }
.pro-meta { margin: 14px 0 0; font-size: 12px; color: var(--color-text-secondary); }
.pro-meta b { color: var(--color-text-primary); font-size: 14px; }
.pro-code { display: block; margin-top: 4px; font-size: 11px; opacity: .8; }
.pro-actions { margin-top: 18px; display: flex; flex-direction: column; gap: 10px; width: 100%; }
.pro-btn { display: flex; align-items: center; justify-content: center; height: 46px; border-radius: 12px; font-size: 13px; font-weight: 700;
  border: 1px solid var(--color-border); background: var(--color-surface); color: var(--color-text-primary); }
.pro-btn-primary { border-color: var(--color-primary); background: color-mix(in srgb, var(--color-primary) 12%, transparent); color: var(--color-primary); }
.pro-fade-enter-active, .pro-fade-leave-active { transition: opacity .2s ease; }
.pro-fade-enter-from, .pro-fade-leave-to { opacity: 0; }
</style>
```

### ۲.۴ `app/utils/paymentAlreadyPaid.ts` (جدید)

```ts
// app/utils/paymentAlreadyPaid.ts
// بک‌اند برای «این مورد قبلاً پرداخت/خریداری شده» code=20 و data={ callBackTypeLabel, callBackId } برمی‌گرداند
export const PAYMENT_ALREADY_PAID_CODE = 20

const DETAIL_ROUTES: Record<string, (id: string) => string> = {
  CompanionReserve: id => `/reserve/${id}`,
  CompanionReserveBatch: () => '/reserve',
  PansionReserve: id => `/pansion/reserve-${id}`,
  SchoolReserve: id => `/school/reserve-${id}`,
  ConsultationPurchase: id => `/consultations/${id}`,
  ProductOrder: id => `/reserve/order-${id}`,
  Trip: id => `/trip/${id}`,
}

export const isPaymentAlreadyPaid = (res: any) => Number(res?.code) === PAYMENT_ALREADY_PAID_CODE

export const paymentAlreadyPaidRoute = (res: any, fallback = '/reserve') => {
  const label = res?.data?.callBackTypeLabel
  const id = res?.data?.callBackId
  const build = typeof label === 'string' ? DETAIL_ROUTES[label] : undefined
  return build && id != null && id !== '' ? build(String(id)) : fallback
}
```

### ۲.۵ `app/app.vue` — یک خط

بعد از `<ToastContainer />` داخل همان `<ClientOnly>`:

```vue
    <ClientOnly>
      <ToastContainer />
      <PaymentRedirectOverlay />
    </ClientOnly>
```

---

## ۳. الگوی مشترک برای هر دکمه‌ی «پرداخت»

```js
// فقط وقتی واقعاً درگاه لازم است لایه را از همان لحظه‌ی کلیک نشان بده (پرداخت کامل با کیف پول درگاه ندارد)
const ticket = needsGateway ? beginPaymentRedirect({ amount: payableAmount }) : undefined
try {
  const res = await requestCheckout(/* ... مثل قبل ... */)
  if (!res?.isSuccess) {
    cancelPaymentRedirect()
    if (isPaymentAlreadyPaid(res)) {
      toast.add({ title: res.messages?.[0]?.item1 || 'این مورد قبلاً پرداخت شده است.', color: 'green' })
      await navigateTo(paymentAlreadyPaidRoute(res, '/reserve'))
      return
    }
    toast.add({ title: 'پرداخت انجام نشد', description: res?.messages?.[0]?.item1, color: 'red' })
    return
  }
  if (res.data?.paymentIsLink === true) {
    const result = await startGatewayRedirect(res.data.paymentUrl, { ticket, code: res.data?.paymentCode })
    if (result === 'invalid-url') toast.add({ title: 'خطا', description: 'لینک معتبر درگاه پرداخت دریافت نشد.', color: 'red' })
    return
  }
  cancelPaymentRedirect()
  // ... همان شاخه‌ی «پرداخت با کیف پول/تخفیف تسویه شد» قبلی
} catch {
  cancelPaymentRedirect()
  toast.add({ title: 'خطا در ارتباط با سرور', description: 'لطفاً دوباره تلاش کنید.', color: 'red' })
}
```

قواعد:
- `toast «در حال انتقال به درگاه…»` و `setTimeout(400)` قبلی حذف شوند؛ لایه جایگزین آن‌هاست.
- `navigateTo(url, { external: true })` / `window.location.href = url` / `window.location.assign(url)` مستقیم به درگاه نماند؛ فقط `startGatewayRedirect`.
- حالت تست (`data.isTestMode`) مثل قبل به درگاه نمی‌رود: `cancelPaymentRedirect()` و همان شاخه‌ی تست.

---

## ۴. تغییر هر نقطه‌ی پرداخت (همه‌ی ۱۰ نقطه)

شماره‌خط‌ها مربوط به commit `6186101` است.

### ۴.۱ `utils/paymentHandoff.ts` → `goToPaymentPage` (کلینیک، سبد کلینیک، پانسیون، مدرسه را یک‌جا پوشش می‌دهد)

حالت `gateway` دیگر به `/payment` نمی‌رود؛ مستقیم همان لایه:

```ts
export const goToPaymentPage = async (handoff: PaymentHandoff) => {
  if (!import.meta.client) return
  const next = typeof handoff.next === 'string' && handoff.next.startsWith('/') && !handoff.next.startsWith('//') ? handoff.next : '/reserve'
  if (handoff.mode === 'gateway') {
    const result = await startGatewayRedirect(handoff.gatewayUrl, { amount: handoff.amount, code: handoff.code })
    if (result === 'invalid-url') await navigateTo(next)
    return
  }
  try {
    sessionStorage.setItem(PAYMENT_HANDOFF_KEY, JSON.stringify({ ...handoff, next }))
  } catch {
    await navigateTo(next)
    return
  }
  await navigateTo('/payment')
}
```

و در هر چهار صفحه، **قبل از** درخواست پرداخت `beginPaymentRedirect` و در هر شاخه‌ی خطا `cancelPaymentRedirect`:

| فایل | کجا |
|---|---|
| `pages/clinic/clinic-[id]/finalize-reserve-[assistanceId].vue` | قبل از `checkout.requestCheckout("/api/companionReserve/ReservePayment"…` (خط ~۶۲۶): `if (amountToPay > 0) beginPaymentRedirect({ amount: amountToPay })`. در `else` نتیجه‌ی ناموفق (خط ~۶۴۷) و در `catch` (خط ~۶۵۰): `cancelPaymentRedirect()`. اگر `isPaymentAlreadyPaid(payRes)` ⇒ toast سبز + `navigateTo(paymentAlreadyPaidRoute(payRes, '/reserve'))`. |
| `pages/clinic/clinic-[id]/finalize-reserve-batch.vue` | قبل از `requestCheckout("/api/companionReserve/BatchPayment"…` (خط ~۵۲۳) `beginPaymentRedirect({ amount: invoiceTotalPrice.value })`؛ `else` (خط ~۵۴۸) و `catch`: `cancelPaymentRedirect()` + بررسی `code 20` |
| `pages/pansion/finalize-reserve-[id].vue` | داخل حلقه، قبل از `companyStore.postPansionPayment` (خط ~۶۵۷) `beginPaymentRedirect({ amount: Number(record.paymentPrice ?? record.price) || 0 })`؛ در `if (!paymentRes?.isSuccess)` (خط ~۶۶۴) و `isSafeGatewayUrl` ناموفق (خط ~۶۶۹) `cancelPaymentRedirect()`؛ بعد از حلقه اگر چیزی به درگاه نرفت (`settled` یا همه ناموفق) هم `cancelPaymentRedirect()` |
| `pages/school/finalize-reserve-[id].vue` | داخل حلقه، قبل از `schoolStore.postSchoolPayment` (خط ~۴۱۶)، همان الگوی پانسیون |

> وقتی کاربر چند پت دارد و رزرو اول با کیف پول تسویه شد ولی دومی درگاه دارد، `begin` حلقه‌ی بعد لایه را تازه می‌کند؛ مشکلی نیست.

### ۴.۲ `pages/payment/index.vue`

> **باگ فوری (اسکرین‌شات ۲۰۲۶-۱۰-۱۲):** کاربر در درگاه Back زد/انصراف را زد و مرورگر همین صفحه را از bfcache با حالت «در حال انتقال...» (`leaving = true`) برگرداند و کاربر گیر کرد. حتی قبل از بقیه‌ی این سند، این چند خط را اضافه کنید:
>
> ```js
> // برگشت از درگاه با دکمه‌ی Back: صفحه همان‌طور که رها شده بود برمی‌گردد؛ کاربر را به جای پرداخت دوباره ببر
> const onPageShow = (event) => {
>   if (!event.persisted) return
>   leaving.value = false
>   navigateTo(handoff.value?.next || '/reserve', { replace: true })
> }
> onMounted(() => window.addEventListener('pageshow', onPageShow))
> onBeforeUnmount(() => window.removeEventListener('pageshow', onPageShow))
> ```
>
> مسیر عادی «انصراف» درگاه بعد از انتشار بک‌اند ۲۰۲۶-۱۰-۱۲ به صفحه‌ی نتیجه‌ی سرویس Payment می‌رود (پیام «پرداخت در درگاه انجام نشد یا لغو شد…» + دکمه‌ی برگشت به همان رزرو/سفارش)، نه این صفحه.

شاخه‌ی `gateway` دیگر از مسیرهای بالا نمی‌آید. برای handoffهای قدیمیِ مانده در sessionStorage، در `onMounted`:

```js
if (isGateway.value) {
  const url = handoff.value.gatewayUrl
  sessionStorage.removeItem(PAYMENT_HANDOFF_KEY)
  const result = await startGatewayRedirect(url, { amount: handoff.value.amount, code: handoff.value.code })
  if (result === 'invalid-url') navigateTo(handoff.value.next || '/reserve', { replace: true })
  return
}
```

و `<template v-if="isGateway">` (کارت + اسپینر + دکمه) را حذف کنید. حالت `done` بدون تغییر.

### ۴.۳ `pages/consultations/buy.vue` — همان صفحه‌ی اسکرین‌شات

تابع `submit` (خط ۳۹۱–۴۵۶):

1. قبل از `requestCheckout` (خط ۴۱۲): `const ticket = expectsGateway ? beginPaymentRedirect({ amount: payable }) : undefined` — `payable` همان مبلغی که صفحه بعد از تخفیف و کیف پول نشان می‌دهد؛ اگر صفر است `undefined`.
2. در `if (!res?.isSuccess)` (خط ۴۱۳) اولین خط `cancelPaymentRedirect()`؛ بعدش **قبل از** شاخه‌های ساعت رزرو:
   ```js
   if (isPaymentAlreadyPaid(res)) {
     toast.add({ title: message, color: 'green' })
     await navigateTo(paymentAlreadyPaidRoute(res, '/consultations'))
     return
   }
   if (/در حال تأیید نهایی بانک/.test(message)) {
     toast.add({ title: 'چند ثانیه صبر کنید', description: message, color: 'orange' })
     return
   }
   ```
3. خط ۴۳۶–۴۴۵ (`paymentIsLink`): toast و `setTimeout` و `navigateTo(... external)` حذف ⇒
   ```js
   if (res.data?.paymentIsLink === true) {
     const result = await startGatewayRedirect(res.data.paymentUrl, { ticket, code: res.data?.paymentCode })
     if (result === 'invalid-url') toast.add({ title: 'خطا', description: 'لینک معتبر درگاه پرداخت دریافت نشد.', color: 'red' })
     return
   }
   cancelPaymentRedirect()
   ```
4. `catch` (خط ۴۵۱): `cancelPaymentRedirect()`.
5. (اصلاح کوچک) خط ۴۴۸: `res.data?.id || res.data?.purchaseId` در پاسخ پرداخت وجود ندارد؛ شناسه‌ی خرید در `res.data?.callBackId` است: `const purchaseId = res.data?.callBackId || res.data?.id`.

### ۴.۴ `stores/useCart.ts` — سبد خرید فروشگاه

`getCartCount` (خط ۲۵۹ به بعد): قبل از `checkout.requestCheckout("/api/cart/cart"…` (خط ۲۷۲) فقط وقتی `isCheckout` و مبلغ قابل پرداخت با درگاه > ۰: `const ticket = beginPaymentRedirect({ amount: Number(cart.value?.data?.paymentPrice) || 0 })`.
خط ۳۳۳–۳۴۶: toast و `setTimeout(400)` و `navigateTo(gatewayUrl, { external: true })` ⇒ `await startGatewayRedirect(gatewayUrl, { ticket, code: paymentCode })`. (`resolveGatewayUrl` بماند؛ خروجی‌اش را بدهید.) در همه‌ی شاخه‌های دیگر (تست، ناموفق، catch) `cancelPaymentRedirect()`.

### ۴.۵ `components/basket/Wallet.vue` و `pages/wallet/index.vue` — شارژ کیف پول

- `Wallet.vue` خط ۱۷۱: قبلش `const ticket = beginPaymentRedirect({ amount: Number(cleanPrice) })`؛ خط ۱۷۵–۱۷۸ ⇒ `startGatewayRedirect(res.data.paymentUrl, { ticket, code: paymentCode })`؛ شاخه‌های دیگر و `catch`: `cancelPaymentRedirect()`.
- `wallet/index.vue` خط ۶۷۶: همان؛ خط ۶۸۳–۶۹۰ ⇒ `startGatewayRedirect`.

### ۴.۶ `pages/ai/index.vue` — خرید پلن PastilAI (دو تابع)

خط ~۱۲۰۴ و ~۱۲۵۴ (`toast.add({ title: 'در حال اتصال به درگاه بانک...' })`): حذف و به‌جایش `const ticket = beginPaymentRedirect({ amount: plan.price })`. خط ۱۲۱۵–۱۲۱۸ و ۱۲۶۵–۱۲۶۸ ⇒ `startGatewayRedirect(res.data.paymentUrl, { ticket, code: res.data?.paymentCode })`. شاخه‌های دیگر و `catch`: `cancelPaymentRedirect()`. پیام «در حال تأیید نهایی بانک» را هم مثل §۴.۳ نارنجی نشان دهید.

### ۴.۷ `pages/transportation/index.vue` — پرداخت سفر پت‌رسان

تابع پرداخت سفر (خط ~۴۲۲۵–۴۲۸۸):
- قبل از `$fetch("/api/trip/payment"…` (خط ۴۲۴۳): `const ticket = activeTrip.value?.fromWallet ? undefined : beginPaymentRedirect({ amount: Number(activeTrip.value?.paymentPrice) || 0 })`.
- `if (!succeeded(response))` (خط ۴۲۴۸): قبل از throw، `cancelPaymentRedirect()`؛ و اگر `isPaymentAlreadyPaid(response)` ⇒ toast سبز + `refreshTripDetail()` (به‌جای خطا).
- خط ۴۲۵۱–۴۲۵۵ ⇒
  ```js
  if (continuation.type === "redirect") {
    await startGatewayRedirect(continuation.url, { ticket, code: continuation.payment?.paymentCode });
    return;
  }
  cancelPaymentRedirect();
  ```
  و `paymentRedirecting.value = true` حذف شود (در bfcache گیر می‌کرد و دکمه را برای همیشه غیرفعال نگه می‌داشت). اگر جای دیگری به `paymentRedirecting` تکیه دارد، با `onPaymentPageReturn(() => { paymentRedirecting.value = false; paymentLoading.value = false })` در setup آزادش کنید.
- `catch` (خط ~۴۲۸۲): `cancelPaymentRedirect()`.

> `requestCheckout("/api/petResanService/petResanService"…` (خط ۳۵۲۹) سرویس هفتگی می‌سازد و به درگاه نمی‌رود؛ تغییری لازم ندارد.

### ۴.۸ `$fetch`های کیف پول/تخفیف قبل از پرداخت (کلینیک، سبد کلینیک، پانسیون، مدرسه)

`PUT /api/companionReserve/wallet`، `/setRebate`، `pansionReserveSetWallet`، `school/reserveSetWallet` و … حالا ممکن است `code: 20` برگردانند (یعنی همین رزرو قبلاً پرداخت شده). کد فعلی نتیجه‌ی این‌ها را نادیده می‌گیرد و بعدش درخواست پرداخت هم همان `code: 20` را می‌دهد، پس با §۳ درست هندل می‌شود؛ تغییر اجباری نیست.

---

## ۵. برگشت از درگاه و دوباره پرداخت کردن (مهم‌ترین قسمت تجربه‌ی کاربر)

- **Back از زرین‌پال**: اگر صفحه از bfcache برگردد، `PaymentRedirectOverlay` با `pageshow.persisted` خودش بسته می‌شود. فلگ‌های محلی هر صفحه (`submitting`، `isProcessing`، `paymentLoading`، …) در `finally` آزاد می‌شوند چون `startGatewayRedirect` بعد از `location.assign` بی‌درنگ برمی‌گردد. فقط فلگ‌هایی که **عمداً** قبل از redirect روی `true` می‌مانند (مثل `paymentRedirecting` در پت‌رسان) باید با `onPaymentPageReturn` آزاد شوند.
- **دوباره زدن «پرداخت»**: هیچ کار خاصی لازم نیست؛ بک‌اند تلاش قبلی را کنار می‌گذارد و لینک تازه می‌دهد.
- **اگر کاربر درگاه قدیمی (تب/تاریخچه‌ی قبلی) را پرداخت کند**: بانک تأیید نمی‌کند و صفحه‌ی نتیجه‌ی پرداخت این پیام را نشان می‌دهد: «این پرداخت با یک تلاش پرداخت جدیدتر جایگزین شده و تأیید نمی‌شود. اگر مبلغی از حساب شما کم شده، بانک آن را خودکار به حسابتان برمی‌گرداند.» (بدون کار وب‌اپ.)

---

## ۶. موارد دیگر

### ۶.۱ مدرسه: پت‌رسانِ تکراری

وقتی کاربر به `finalize-reserve` مدرسه برمی‌گردد و ثبت‌نام پرداخت‌نشده‌ی قبلی **همان** ثبت‌نام برمی‌گردد (§۱.۴)، اگر در تلاش قبلی پت‌رسان را هم اضافه کرده بود، `ensurePetResanTripCreated` دوباره `POST /api/petresan/schoolReservation` می‌زند و بک‌اند می‌گوید «برای این رزرو قبلاً یک سفر پت‌رسان ثبت شده است.» و پرداخت متوقف می‌شود. پیشنهاد: در `ensurePetResanTripCreated` همین پیام را موفق حساب کنید:

```js
if (tripRes?.isSuccess || /قبلاً یک سفر پت‌رسان ثبت شده/.test(tripRes?.messages?.[0]?.item1 || '')) {
  petResanCreatedForReserveId.value = Number(reserveId)
  return true
}
```

### ۶.۲ درگاه سامان (فرم HTML)

سامان به‌جای لینک، فرم HTML برمی‌گرداند: `paymentIsLink: false` و `paymentUrl` = متنی که با `<form` شروع می‌شود. امروز `resolvePaymentContinuation` و شاخه‌های `else` صفحه‌ها این را «تسویه شد» حساب می‌کنند و پیام موفقیت می‌دهند بدون این‌که پولی پرداخت شده باشد. تا وقتی این حالت در وب‌اپ پشتیبانی نشده، مرچنت سامان در پنل فعال نشود. اگر خواستید پشتیبانی کنید: وقتی `paymentIsLink === false` و `typeof paymentUrl === 'string' && paymentUrl.trimStart().startsWith('<form')`، فرم را در یک `div` مخفی بگذارید و `submit()` کنید (همان لایه‌ی §۲.۳ را قبلش نشان دهید).

### ۶.۳ پیام تازه‌ی صفحه‌ی نتیجه‌ی پرداخت

اگر بانک پول را گرفت ولی ثبت رزرو/سفارش ممکن نشد، بک‌اند حالا کل مبلغ را به کیف پول برمی‌گرداند و صفحه‌ی نتیجه (سرویس Payment) این را نشان می‌دهد: «پرداخت شما از بانک انجام شد ولی ثبت این مورد ممکن نشد؛ کل مبلغ به کیف پول پاستیل شما برگشت داده شد.» کاری در وب‌اپ لازم نیست؛ کاربر می‌تواند دوباره با کیف پول پرداخت کند.

---

## ۷. چک‌لیست تست (روی گوشی، هر دو تم)

| # | سناریو | انتظار |
|---|---|---|
| ۱ | هر ۱۰ نقطه: «پرداخت» با درگاه | فوراً لایه‌ی سبد خرید + نوار؛ بعد درگاه زرین‌پال |
| ۲ | همان در تم روشن و تاریک (پروفایل ⇒ تم) | رنگ‌ها درست، متن خوانا |
| ۳ | اینترنت کند (DevTools: Slow 3G) | نوار تا ~۹۰٪ آرام می‌رود؛ بعد از ۲۰ ثانیه دکمه‌ی «انصراف» |
| ۴ | در درگاه Back یا «انصراف» زرین‌پال ⇒ برگشت به اپ | لایه بسته، دکمه‌ی پرداخت فعال |
| ۵ | بلافاصله دوباره «پرداخت» (مشاوره، کلینیک، پانسیون، مدرسه، سفر، AI، کیف پول، سبد) | درگاه تازه، **بدون** «پرداخت شروع شده…» یا «قبلا رزرو شده» |
| ۶ | بعد از برگشت، کیف پول را خاموش/روشن یا کد تخفیف بزنید، بعد پرداخت | بدون خطای قفل |
| ۷ | مدرسه: ثبت‌نام بدون پرداخت، بستن صفحه، ورود دوباره به همان دوره با همان پت | ثبت‌نام قبلی ادامه پیدا می‌کند و به درگاه می‌رود |
| ۸ | پرداخت موفق یک رزرو، بعد دوباره «پرداخت» همان رزرو (مثلاً از فهرست رزروها) | پیام سبز «این رزرو قبلاً پرداخت شده است.» + انتقال به جزئیات |
| ۹ | مشاوره‌ی ساعت‌دار: پرداخت موفق، دوباره خرید همان پکیج و همان ساعت | «این مشاوره قبلاً خریداری شده است.» + `/consultations/{id}` |
| ۱۰ | مشاوره‌ی ساعت‌دار: رفتن به درگاه و برگشت بدون پرداخت، دوباره همان ساعت | ساعت آزاد است (رزرو معلق قبلی خودش لغو شد) |
| ۱۱ | پرداخت کامل با کیف پول | لایه نشان داده **نمی‌شود** (یا فقط لحظه‌ای)؛ همان پیام موفقیت قبلی |
| ۱۲ | در تنظیمات دسترسی «کاهش حرکت» روشن | سبد ساکن، نوار پر می‌شود |

---

## ۸. خلاصه‌ی فایل‌ها

| فایل | نوع |
|---|---|
| `app/components/payment/PaymentGatewayLoader.vue` | جدید (§۲.۱) |
| `app/components/payment/PaymentRedirectOverlay.vue` | جدید (§۲.۳) |
| `app/composables/usePaymentRedirect.ts` | جدید (§۲.۲) |
| `app/utils/paymentAlreadyPaid.ts` | جدید (§۲.۴) |
| `app/app.vue` | یک خط (§۲.۵) |
| `app/utils/paymentHandoff.ts` | `goToPaymentPage` (§۴.۱) |
| `app/pages/payment/index.vue` | حذف شاخه‌ی gateway (§۴.۲) |
| `app/pages/consultations/buy.vue` | §۴.۳ |
| `app/stores/useCart.ts` | §۴.۴ |
| `app/components/basket/Wallet.vue`، `app/pages/wallet/index.vue` | §۴.۵ |
| `app/pages/ai/index.vue` | §۴.۶ |
| `app/pages/transportation/index.vue` | §۴.۷ |
| `finalize-reserve-[assistanceId].vue`، `finalize-reserve-batch.vue`، `pansion/finalize-reserve-[id].vue`، `school/finalize-reserve-[id].vue` | `begin`/`cancel` + `code 20` (§۴.۱) |
| `school/finalize-reserve-[id].vue` | پت‌رسان تکراری (§۶) |

اپ فلاتر (اگر مسیر پرداخت بومی دارد): همان قرارداد §۱ را رعایت کند — `code: 20` ⇒ جزئیات همان مورد؛ پیام «در حال تأیید نهایی بانک» ⇒ صبر چند ثانیه‌ای؛ هیچ قفل محلی برای «پرداخت در جریان» نگه ندارد.
