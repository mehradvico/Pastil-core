# «چیزی فراموش نکردید؟» هوشمند (پیشنهاد مکمل کل سبد) — مستند وب‌اپ

> مخاطب: تیم وب‌اپ (Nuxt 4) · وضعیت بک: **پیاده و build شد، دیپلوی نشده** (بدون SQL/migration). تست واحد: `Application.Tests/ProductRelations/*` (۳۲ تست سبز).
> مرتبط: [PRODUCT_SIMILAR_AND_BOUGHT_TOGETHER_WEBAPP_FA.md](PRODUCT_SIMILAR_AND_BOUGHT_TOGETHER_WEBAPP_FA.md) (همان موتور).

## ۱) وضع فعلی و مشکل

مودال `app/components/basket/CrossSellModal.vue` (وقتی مشتری در `/store/basket` روی «ادامه خرید از این فروشگاه» می‌زند باز می‌شود) الان در `collectRelatedProducts` فقط **همان دسته‌ی کالاهای سبد** در همان فروشگاه را از `/api/products/products` می‌خواند (جدیدترین‌ها). یعنی برای خاک گربه، خاک گربه‌ی دیگر! نه چیزی که واقعاً «فراموش شده».

## ۲) رفتار جدید بک

### `GET /api/EndUser/Cart/Suggestions?storeId={id}&take=6`
- مشتری لاگین: سبدش از روی **توکن** خوانده می‌شود. مهمان: `&uniqueId={cartUniqueId}` (از `cartStore.readSavedCartUniqueId()`/`useBasketId()`).
- `storeId` = فروشگاهی که مشتری همین الان از آن ادامه می‌دهد (`pendingStoreId`). فقط **اقلام همان فروشگاه** نامزدکننده‌اند و فقط **کالاهایی پیشنهاد می‌شوند که همان فروشگاه همین الان با موجودی می‌فروشد** — پس `addProduct` (که `productItemId` همان فروشگاه را پیدا می‌کند) همیشه موفق است.
- پاسخ مستقیم (نه `{isSuccess,data}`):

```json
{
  "list": [ { "id": 812, "name": "ظرف خاک مسقف میشا با بیلچه", "price": 450000, "picture": {...}, "...": "همان ProductVDto" } ],
  "totalCount": 6,
  "reasons": [
    { "productId": 812, "kind": "litter_box", "forProductId": 301, "forProductName": "خاک گربه میوکت ..." }
  ]
}
```
`reasons` هم‌ترتیب `list` است.

### قواعد (چرا بی‌ربط نمی‌آید)
- فقط **نوع‌های مکمل** (خاک گربه ← ظرف خاک، بیلچه، مت، بوگیر؛ جای خواب سگ ← پد، تشویقی، اسباب‌بازی؛ غذای خشک ← ظرف، تشویقی، مکمل، کنسرو؛ قلاده ← لیش/پلاک/هارنس/کیسه‌ی مدفوع؛ …).
- **حیوان همان کالا**: برای خاک گربه هیچ‌وقت پد سگ؛ پد توالت هرگز برای گربه/پرنده.
- **نوعی را که مشتری الان دارد پیشنهاد نمی‌دهد** (در سبد هر فروشگاه) و کالای موجود در سبد را تکرار نمی‌کند.
- **فقط موجود** با قیمت؛ قیمت پیشنهاد بیش از ۴ برابر گران‌ترین قلم سبد نباشد (پیشنهادِ «وسوسه‌ای» باید معقول باشد) و کالای گران‌تر امتیاز کمتر می‌گیرد.
- کالایی که چند قلم سبد به آن نیاز دارند (مثلاً ظرف غذا برای هم غذای خشک هم تشویقی) **بالاتر** می‌آید؛ خرید هم‌زمان واقعی مشتریان امتیاز می‌افزاید.
- هر نوع حداکثر ۲ کالا؛ اگر چیز مطمئنی نباشد **`list: []`** (وب‌اپ باید مودال را باز نکند و مستقیم برود صورتحساب).
- اقلام با نوع نامشخص فقط وقتی اثر دارند که ≥۴ سفارش واقعی هم‌زمان خریده باشند.

## ۳) BFF جدید

**`app/server/api/cart/suggestions.get.js`**
```js
export default defineEventHandler(async (event) => {
  const { public: { baseUrl } } = useRuntimeConfig()
  const token = getCookie(event, 'token')
  try {
    return await $fetch(`${baseUrl}/api/EndUser/Cart/Suggestions`, {
      method: 'GET',
      headers: { Accept: 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) },
      query: getQuery(event), // { storeId, uniqueId?, take? }
    })
  } catch (error) {
    return { list: [], totalCount: 0, reasons: [] } // خطا = مودال باز نشود، مستقیم صورتحساب
  }
})
```

## ۴) تغییر `app/components/basket/CrossSellModal.vue`

### ۴-۱) `<script setup>`

۱. state جدید کنار `products`:
```js
const reasons = ref({}) // productId(string) → { kind, forProductName }
const KIND_LABELS = {
  litter: 'خاک گربه', litter_box: 'ظرف خاک', litter_scoop: 'بیلچه خاک', litter_mat: 'مت ظرف خاک', litter_deodorizer: 'بوگیر خاک',
  pad: 'پد توالت', bowl: 'ظرف آب و غذا', fountain: 'آبخوری', dry_food: 'غذای خشک', wet_food: 'کنسرو و پوچ', food: 'غذا', treat: 'تشویقی',
  toy: 'اسباب‌بازی', bed: 'جای خواب', carrier: 'کیف و باکس حمل', scratcher: 'اسکرچر', collar: 'قلاده', leash: 'لیش', harness: 'هارنس',
  tag: 'پلاک', poop_bag: 'کیسه مدفوع', brush: 'برس', shampoo: 'شامپو', nail: 'ناخن‌گیر', dental: 'مراقبت دهان و دندان',
  wipes: 'دستمال مرطوب', supplement: 'مکمل', training: 'وسایل آموزش', nest: 'لانه', perch: 'نشیمنگاه', cage: 'قفس', hideout: 'مخفیگاه',
  playground: 'پارک بازی', bird_hanging: 'آویز قفس',
}
const kindLabel = (id) => KIND_LABELS[reasons.value[String(id)]?.kind] || ''
const forName = (id) => reasons.value[String(id)]?.forProductName || ''
```

۲. **`collectRelatedProducts` را کاملاً با این جایگزین کنید** (و `fetchProductList` حذف شود):
```js
const collectRelatedProducts = async () => {
  const storeId = props.preferredStoreId
  if (!storeId) return []
  const uniqueId = cartStore.readSavedCartUniqueId?.() || undefined
  const res = await $fetch('/api/cart/suggestions', {
    query: { storeId, take: Math.min(props.maxItems, 8), ...(uniqueId ? { uniqueId } : {}) },
  })
  const map = {}
  for (const r of res?.reasons || []) map[String(r.productId)] = r
  reasons.value = map
  return (res?.list || [])
    .filter((p) => Number(p?.price ?? p?.basePrice) > 0)
    .map((p) => ({ ...p, id: p?.id ?? p?.Id }))
}
```
(فیلترِ «اقلام سبد» و «دسته‌ها» دیگر لازم نیست؛ بک انجام می‌دهد. `prepareAndOpen`، `addProduct`، `resolveProductItemId` و بقیه **بدون تغییر**. اگر `products` خالی شد، `prepareAndOpen` همان `false` برمی‌گرداند و صفحه مستقیم به صورتحساب می‌رود — رفتار فعلی.)

### ۴-۲) `<template>`

۱. زیرعنوان هدر: به‌جای «… پیشنهاد از دسته‌بندی‌های انتخابی شما»:
```vue
<p class="mt-1 text-[11px] text-text-secondary">
  {{ products.length.toLocaleString('fa-IR') }} پیشنهاد مکمل برای سبد شما
</p>
```
۲. متن راهنمای آبی: «این گزینه‌ها از همان فروشگاه و دسته‌بندی کالاهای سبد شما انتخاب شده‌اند…» ← 
«لوازمی که معمولاً کنار کالاهای سبد شما لازم می‌شود؛ همین‌جا اضافه کنید تا با یک پرداخت تحویل بگیرید.»
۳. زیر نام هر کالا (داخل `<div class="min-w-0 flex-1 self-stretch py-0.5">` بعد از `<p class="line-clamp-2 …">{{ item.name }}</p>`) **دلیل پیشنهاد** را نشان دهید:
```vue
<p v-if="kindLabel(item.id)" class="mt-1 flex items-center gap-1 text-[11px] text-secondary">
  <Icon name="solar:link-round-angle-linear" size="12" />
  <span>{{ kindLabel(item.id) }}<template v-if="forName(item.id)"> · مکمل «{{ forName(item.id).length > 28 ? forName(item.id).slice(0, 28) + '…' : forName(item.id) }}»</template></span>
</p>
```
(این همان چیزی است که اعتماد می‌سازد: مشتری می‌بیند چرا این کالا پیشنهاد شده.)

### ۴-۳) `app/pages/store/basket/index.vue`
تغییری لازم نیست (`:preferred-store-id="pendingStoreId"` و `openCrossSellOrInvoice` همین‌طور می‌مانند).

## ۵) چک‌لیست تست دستی

1. سبد: «خاک گربه + غذای خشک گربه» از یک فروشگاه ← مودال: ظرف خاک، بیلچه، مت، بوگیر، ظرف غذا، تشویقی گربه…؛ **هیچ خاک گربه‌ی دیگری و هیچ کالای سگ/پرنده‌ای نه**.
2. سبد: «جای خواب سگ + تشویقی سگ» ← پد بهداشتی سگ، اسباب‌بازی، ظرف غذا؛ نه پد برای گربه.
3. سبد فقط «قفس پرنده» یا کالایی که نوعش ناشناخته است ← پاسخ خالی ← مودال باز نمی‌شود و مستقیم صورتحساب.
4. «ظرف خاک» را قبلاً در سبد دارد ← ظرف خاک دیگر پیشنهاد نمی‌شود (بیلچه/مت می‌شود).
5. افزودن از مودال با `+` موفق است (کالا همان فروشگاه را دارد) و شمارنده‌ی +/− کار می‌کند.
6. مهمان (بدون لاگین) تا قبل از ورود مودال باز نمی‌شود (`AuthModal` اول) — بعد از ورود با توکن کار می‌کند.

## ۶) دیپلوی

فقط **Api** (بدون SQL). ترتیب: اول Api، بعد وب‌اپ. اگر وب‌اپ زودتر بیاید BFF خطا می‌گیرد، لیست خالی برمی‌گردد و مشتری مستقیم به صورتحساب می‌رود (ایمن، فقط پیشنهاد نمی‌بیند).

## ۷) نکته‌ی داده

پیشنهادها فقط از کالاهایی است که **فروشگاهِ انتخاب‌شده همین الان با موجودی و قیمت دارد**؛ هرچه فروشگاه کاتالوگ کامل‌تر و موجودی به‌روزتر داشته باشد، سبد کامل‌تری پیشنهاد می‌شود. نام شفاف کالا («خاک گربه …»، «ظرف خاک …») دقت تشخیص نوع را بالا می‌برد.
