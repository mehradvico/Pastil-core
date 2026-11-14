# همه‌ی فروشگاه‌های دارای کالا + پیش‌فرضِ فروشگاهِ مبدأ + قیمتِ هر فروشگاه — مستند وب‌اپ (موبه‌مو)

> مخاطب: تیم وب‌اپ (Nuxt 4). وضعیت بک: **پیاده و تست شد، دیپلوی نشده** (بدون SQL/migration). تست: `Application.Tests/ProductStoreOffersTests.cs` (۳ تست روی SQL Server واقعی).
> پاسخ‌ها: `GET /api/Product` لیست `{ list, totalCount }`؛ `GET /api/ProductItem/{id}` به شکل `{ isSuccess, messages, data }`. فیلدها camelCase.

## ۱) ماجرا و علت‌ها

کاربر در صفحه‌ی «پت‌شاپ فرشته» کالایی را باز کرد؛ صفحه‌ی کالا فقط «پت‌شاپ هپت» را نشان داد و با «بازگشت» به فرشته برگشت. سه علت جدا داشت:

| # | علت | وضعیت |
| --- | --- | --- |
| ۱ | **داده‌ی کهنه:** فروشگاه فرشته (id=۱۴) بعد از تأیید ادمین، `SystemActive`ِ ۲۶ آیتمش ۰ مانده بود (Api بعد از «تأیید فروشگاه» procedure قیمت را اجرا نمی‌کرد). صفحه‌ی خودِ فروشگاه این آیتم‌ها را نشان می‌داد ولی صفحه‌ی کالا فقط آیتم‌های `SystemActive=1` را برمی‌گرداند ⇒ فقط هپت. | **کد اصلاح شد** (`StoreService.UpdateApprovalAsync` حالا procedure را اجرا می‌کند). داده‌ی فعلی را اسکریپت `backend/scripts/RecomputeStoreProductPrices.sql` درست می‌کند (کاربر اجرا کند). |
| ۲ | **قیمت کارت در صفحه‌ی فروشگاه:** `Price` هر محصول ارزان‌ترین پیشنهادِ بین همه‌ی فروشگاه‌هاست؛ در صفحه‌ی فرشته قیمتِ هپت دیده می‌شد. | **بک اصلاح شد** (پایین). |
| ۳ | **پیش‌فرض صفحه‌ی کالا:** فروشگاه مبدأ به صفحه‌ی کالا منتقل نمی‌شد؛ پس همیشه ارزان‌ترین (یا فروشگاهِ سبد) انتخاب می‌شد. | **بک پارامتر `storeId` گرفت؛ وب‌اپ باید بفرستد** (کار وب‌اپ). |

قاعده‌ی نهایی:
- صفحه‌ی کالا (عادی): همه‌ی فروشگاه‌های دارای کالا از **ارزان به گران**؛ پیش‌فرض **ارزان‌ترین**.
- ورود از صفحه‌ی یک فروشگاه: پیش‌فرض **همان فروشگاه با قیمت خودش**؛ بقیه‌ی فروشگاه‌ها (کمتر یا بیشتر) در لیست از ارزان به گران.
- صفحه‌ی فروشگاه: قیمت هر کارت = **قیمتِ همان فروشگاه**.

## ۲) قراردادهای API (بک)

### ۲-۱) `GET /api/Product?StoreId={id}&…` (صفحه‌ی فروشگاه)

وقتی `StoreId` داده شود (فقط endpoint عمومی):

| تغییر | شرح |
| --- | --- |
| `price`, `basePrice`, `discountPercent`, `discountGroupId`, `discountEndDate` | از **ارزان‌ترین آیتمِ موجودِ همان فروشگاه** (نه بین همه‌ی فروشگاه‌ها) |
| `storeItemAvailable` (**جدید**) | `true` = این فروشگاه الان کالا را موجود دارد و قیمت‌ها قیمتِ همین فروشگاه است. `false` = در این فروشگاه ناموجود است و **`price = 0`** (قیمتِ فروشگاه دیگر نشان داده نمی‌شود). بدون `StoreId` همیشه `null`. |
| مرتب‌سازی `sortBy` گران/ارزان | با قیمتِ همان فروشگاه |
| فیلتر | آیتمِ حذف‌شده یا غیرفعالِ فروشگاه، کالا را در صفحه‌ی آن فروشگاه نشان نمی‌دهد |

نکته‌ها: `priceFrom/priceTo` هنوز روی «ارزان‌ترین بین همه‌ی فروشگاه‌ها» اعمال می‌شود. پنل فروشنده و ادمین (که این endpoint را نمی‌زنند) تغییری ندارند.

### ۲-۲) `GET /api/ProductItem/{productId}?storeId={id}` (صفحه‌ی کالا)

`storeId` **اختیاری** است. پاسخ `data`:

```json
{
  "variety1": { ... }, "variety2": null,
  "productItems": [
    { "id": 15773, "storeId": 15, "price": 395000, "basePrice": 395000, "discountPercent": 0, "quantity": 2, "varietyItem": {...}, "store": { "id": 15, "name": "پت شاپ هپت", ... } },
    { "id": 16502, "storeId": 14, "price": 649000, ... }
  ],
  "offers": [
    { "storeId": 15, "store": { "storeId": 15, "name": "پت شاپ هپت", "rateAvg": 4.2, "picture": {...}, "expressShippingEnabled": true },
      "productItemId": 15773, "price": 395000, "basePrice": 395000, "discountPercent": 0, "quantity": 2, "isPreferred": false },
    { "storeId": 14, "store": { "name": "پت شاپ فرشته", ... }, "productItemId": 16502, "price": 649000, "basePrice": 649000, "discountPercent": 0, "quantity": 6, "isPreferred": true }
  ],
  "preferredStoreId": 14
}
```

- `productItems`: **همه‌ی** آیتم‌های قابل‌فروش از **همه‌ی** فروشگاه‌های فعال، **از کم به زیاد** (`price`، بعد `id`). (قبلاً ترتیب نداشت و فرانت مرتب می‌کرد؛ آیتمِ فروشگاه غیرفعال/حذف‌شده هرگز نمی‌آید.)
- `offers`: هر فروشگاه **یک‌بار** با ارزان‌ترین آیتمِ موجودش (همه‌ی تنوع‌ها)، از کم به زیاد. برای لیستِ «فروشندگان این کالا» کافی است؛ برای کالای **دارای تنوع** قیمتِ هر فروشگاه به‌ازای تنوعِ انتخاب‌شده را از `productItems` بسازید (همان `getStoreProductItem` فعلی).
- `preferredStoreId`: اگر `storeId` فرستاده شده **و** آن فروشگاه همین الان این کالا را موجود دارد ⇒ همان؛ وگرنه `null` (یعنی «پیش‌فرض = ارزان‌ترین»). `offers[].isPreferred` هم همین را نشان می‌دهد.
- کالای ناموجود در همه‌ی فروشگاه‌ها: `isSuccess:true` با `productItems: []`. محصولِ نامعتبر: `isSuccess:false`.

### ۲-۳) `GET /api/ProductItem2/{id}?varietyItem1Id=&varietyItem2Id=&storeId=`
(فعلاً وب‌اپ از آن استفاده نمی‌کند) همین `offers` و `preferredStoreId` را **برای تنوعِ انتخاب‌شده** برمی‌گرداند.

## ۳) تغییرات وب‌اپ — فایل‌به‌فایل

### ۳-۱) BFF — `app/server/api/products/productItem.get.js`
`storeId` را به بک بفرستید:

```js
query: {
  varietyItem1Id: getQuery(event)?.varietyItem1Id,
  varietyItem2Id: getQuery(event)?.varietyItem2Id,
  storeId: getQuery(event)?.storeId,          // ← جدید
}
```
(BFF محصولات `products.get.js` همه‌ی query را رد می‌کند؛ تغییری لازم نیست.)

### ۳-۲) لینک کارت محصول با فروشگاه مبدأ

۱. `app/utils/productPath.ts` — پارامتر دوم اختیاری:
```ts
export const productPath = (
  item: { id?: number | string | null, name?: string | null } | null | undefined,
  options?: { storeId?: number | string | null },
): string | null => {
  const id = Number(item?.id)
  if (!Number.isInteger(id) || id <= 0) return null
  const slug = String(item?.name || '').trim().replace(/[\s/\?#%]+/g, '-')
  const path = slug ? `/store/products/${id}-${slug}` : `/store/products/${id}`
  const storeId = Number(options?.storeId)
  return Number.isInteger(storeId) && storeId > 0 ? `${path}?storeId=${storeId}` : path
}
```
۲. `app/components/product/item.vue` و `app/components/product/HorizontalItem.vue`: prop جدید `storeId` و در `NuxtLink`:
```vue
:to="productPath(item, { storeId }) || '/store/products'"
```
(`const props = defineProps(["item", "isrounded", "storeId"])`؛ در `HorizontalItem.vue` هم همین prop.)
۳. **فقط صفحه‌های فروشگاه** `storeId` را بدهند (همه‌ی جاهای دیگر — صفحه‌ی اصلی، جست‌وجو، دسته‌ها — بدون `storeId`، یعنی ارزان‌ترین):
- `app/pages/store/petshop/[id].vue` (جایی که `<ProductHorizontalItem …>` رندر می‌شود، حدود خط ۲۲۰): `:store-id="storeIdNumber"`
- `app/pages/store/clothing-store/[id].vue`: همان.
- `const storeIdNumber = computed(() => Number(String(route.params.id).split('-')[0]) || null)`.
(شناسه‌ی عددی فروشگاه؛ نه `slug`.)

### ۳-۳) صفحه‌ی کالا — `app/pages/store/products/[id]/index.vue`

۱. شناسه‌ی فروشگاهِ مبدأ را از URL بخوانید:
```js
const requestedStoreId = computed(() => {
  const value = Number(route.query.storeId)
  return Number.isInteger(value) && value > 0 ? value : null
})
```
۲. `getProductItem()` را اصلاح کنید: `storeId` را بفرستید و اولویتِ انتخاب را **مبدأ ← فروشگاهِ سبد ← ارزان‌ترین** کنید:
```js
async function getProductItem() {
  const res = await $fetch("/api/products/productItem", {
    method: "GET",
    query: {
      id: route.params.id.split("-")[0],
      ...(requestedStoreId.value ? { storeId: requestedStoreId.value } : {}),
    },
  });
  if (!res.isSuccess) { varietyList.value = {}; return; }

  varietyList.value = res.data;
  const items = varietyList.value.productItems || [];           // بک از قبل از ارزان به گران مرتب کرده
  if (items.length === 0) return;

  // ۱) فروشگاهی که کاربر از صفحه‌ی آن آمده (فقط اگر بک تأیید کرده الان موجود دارد)
  let storeId = varietyList.value.preferredStoreId || null;
  // ۲) فروشگاهِ فعالِ سبد خرید
  if (!storeId) {
    const activeCartStore = cartStore.cart?.data?.cartStores?.find((s) => s?.active || s?.Active);
    const cartStoreId = activeCartStore?.storeId || activeCartStore?.StoreId;
    if (cartStoreId && items.some((i) => i?.storeId === cartStoreId || i?.store?.id === cartStoreId)) storeId = cartStoreId;
  }
  // ۳) ارزان‌ترین
  const chosen = storeId
    ? items.find((i) => i?.storeId === storeId || i?.store?.id === storeId)
    : [...items].sort((a, b) => getEffectivePrice(a) - getEffectivePrice(b))[0];

  selectedVarity.value = chosen;
  isfilterStore.value = chosen?.storeId || chosen?.store?.id;
  selectedVarity2.value = selectedVarity.value;

  // کاربر از صفحه‌ی یک فروشگاه آمده ولی آن فروشگاه این کالا را الان ندارد
  if (requestedStoreId.value && !varietyList.value.preferredStoreId) {
    useToast().add({ title: "این کالا در فروشگاه انتخابی موجود نیست", description: "از ارزان‌ترین فروشنده نمایش داده شد.", color: "orange" });
  }
}
```
(نکته: اولویتِ `storeId` مبدأ **بالاتر از سبد** است؛ چون کاربر صریحاً از آن فروشگاه آمده.)

۳. وقتی کاربر در لیست فروشندگان فروشگاه دیگری را انتخاب می‌کند، URL را هم عوض کنید تا «بازگشت/رفرش/اشتراک» همان فروشگاه را نگه دارد:
```js
const updateFilterStore = (storeId) => {
  isfilterStore.value = storeId;
  navigateTo({ path: route.path, query: { ...route.query, storeId } }, { replace: true });   // صفحه را دوباره لود نکند
};
```
(`canonical` فقط مسیر است و `?storeId` را ندارد؛ پس سئو دست‌نخورده است.)

۴. `watch(() => route.query.storeId, getProductItem)` اضافه نکنید (انتخاب دستیِ فروشگاه با `isfilterStore` مدیریت می‌شود)؛ فقط اگر مسیرِ کالا عوض شد (کالای دیگر) صفحه همان‌طور که الان هست دوباره ساخته می‌شود.

### ۳-۴) لیست فروشندگان — `app/components/products/seller.vue`

الان فقط «فروشنده‌ی فعال» + دکمه‌ی «مشاهده N فروشنده» (مودال) دارد. خواسته: **همه‌ی فروشگاه‌ها مستقیم در صفحه، از ارزان به گران**.

۱. ترتیب بر اساس قیمتِ **همان تنوعِ انتخاب‌شده** (قیمتِ فروشگاه برای هر تنوع فرق می‌کند)؛ ناموجودها آخر:
```js
const storePrice = (storeId) => {
  const item = getStoreProductItem(storeId);
  if (!item || isProductUnavailable(item)) return Infinity;
  return item.price || item.basePrice || Infinity;
};
const sortedStores = computed(() =>
  [...(props.stores || [])].sort((a, b) => storePrice(a.storeId) - storePrice(b.storeId)),
);
const cheapestStoreId = computed(() => {
  const first = sortedStores.value[0];
  return first && storePrice(first.storeId) !== Infinity ? first.storeId : null;
});
const requestedStoreId = computed(() => Number(useRoute().query.storeId) || null);
```
۲. زیر کارت «فروشنده‌ی فعال» یک **لیست ثابت** (نه مودال) از `sortedStores` بگذارید (حداکثر همه؛ اگر بیشتر از ۵ بود ۵ تا + «مشاهده‌ی همه» که همان مودال فعلی را باز می‌کند). هر ردیف:
   - لوگو و نام فروشگاه، امتیاز، نشان «تحویل ۳ ساعته» اگر `expressShippingEnabled`
   - **قیمتِ همان فروشگاه** (یا «ناموجود» قرمز اگر `storePrice` بی‌نهایت)، قیمت خط‌خورده اگر `discountPercent > 0`
   - نشان‌ها: **«ارزان‌ترین»** (برای `cheapestStoreId`)، **«فروشگاهی که از آن آمدید»** (برای `requestedStoreId`)، «انتخاب‌شده» (برای `selected2.storeId`)
   - اگر قیمتش از فروشگاهِ انتخاب‌شده کمتر/بیشتر است، یک خط کوچک: «X تومان ارزان‌تر» / «X تومان گران‌تر» (تفاوتِ `storePrice` با قیمتِ `selected2`)
   - کلیک: `selectStore(storeId)` (همان emit فعلی `filterByStore`)؛ ردیف‌های ناموجود غیرفعال (`pointer-events-none opacity-50`).
۳. دکمه‌ی «مشاهده N فروشنده» حالا برای جزئیات بیشتر (گارانتی/امتیاز) می‌ماند؛ متنش را «مقایسه‌ی N فروشگاه» کنید و مودال هم `v-for="item in sortedStores"` بشود (به‌جای `stores`).
۴. اگر فقط یک فروشگاه هست (`sortedStores.length === 1`) لیست و دکمه نمایش داده نشود (همان رفتار فعلی).
۵. سبک: همان کلاس‌های فعلی کارت‌ها (`bg-surface border border-border rounded-2xl`, تایپوگرافی `text-body-1/text-body-2`، رنگ قیمت `text-green-500`) — رنگ/اندازه‌ی جدید معرفی نکنید.

### ۳-۵) کارت محصول با `storeItemAvailable`

وقتی لیست با `StoreId` گرفته می‌شود و `item.storeItemAvailable === false`، کارت باید «ناموجود» باشد (قیمت ۰ است):
- `app/utils/product.ts` → `isProductUnavailable(item)`: اگر `item?.storeItemAvailable === false` ⇒ `true`.
- `app/utils/stockFirstProducts.js` (BFFِ «ناموجود آخر لیست») و `sortProductsByAvailability`: همین شرط را در تشخیص «ناموجود» اضافه کنید تا کالاهای ناموجودِ آن فروشگاه ته لیست بروند.

### ۳-۶) چیزهایی که تغییر **نمی‌کنند**
- افزودن به سبد: همان `productItemId` آیتمِ انتخاب‌شده؛ سبد چندفروشگاهی مثل قبل.
- `currentVariety.stores` و `productItemsFilter` (انتخاب بر اساس تنوع + فروشگاه) همان‌اند؛ فقط ورودی بهتر شد (مرتب و `preferredStoreId`).
- SEO: `canonical` بدون query؛ `?storeId` فقط برای تجربه‌ی کاربر.

## ۴) سناریوهای دقیق (برای تست)

| سناریو | انتظار |
| --- | --- |
| باز کردن کالا از صفحه‌ی اصلی/جست‌وجو (بدون `storeId`) | ارزان‌ترین فروشگاه انتخاب؛ لیست فروشندگان از کم به زیاد؛ «ارزان‌ترین» روی ردیف اول |
| صفحه‌ی فرشته ← کارت کالا | قیمتِ **فرشته** روی کارت؛ لینک `…?storeId=14`؛ صفحه‌ی کالا با فرشته و **قیمتِ فرشته** باز شود؛ هپت (ارزان‌تر) در لیست بالاتر و با «۲۵۴٬۰۰۰ تومان ارزان‌تر» |
| در صفحه‌ی کالا فروشگاه دیگر را انتخاب | قیمت/دکمه‌ی خرید عوض شود؛ URL بدون ری‌لود `?storeId=` جدید بگیرد؛ «بازگشت» به صفحه‌ی فروشگاه برود (نه به انتخاب‌های قبلی) |
| کالا در فرشته ناموجود، در هپت موجود | کارت در صفحه‌ی فرشته «ناموجود» (قیمت ۰)؛ با کلیک، صفحه‌ی کالا با هپت و پیام «در فروشگاه انتخابی موجود نیست» |
| کالای دارای تنوع (سایز/رنگ) | با تغییر تنوع، لیست فروشندگان دوباره بر اساس قیمتِ **همان تنوع** مرتب شود؛ فروشگاهِ فاقد آن تنوع «ناموجود» و آخر لیست |
| `storeId` نامعتبر/فروشگاه غیرفعال در URL | نادیده گرفته می‌شود (`preferredStoreId=null`)؛ ارزان‌ترین |
| سبدِ فعالِ فروشگاه X + ورود از صفحه‌ی فروشگاه Y | Y (مبدأ) اولویت دارد |
| فقط یک فروشگاه کالا را دارد | لیست/دکمه‌ی مقایسه نمایش داده نشود |

## ۵) ترتیب اجرا

1. (کاربر) `backend/scripts/RecomputeStoreProductPrices.sql` — کالاهای کهنه‌ی فرشته (۲۶ آیتم) را درست می‌کند. **این اسکریپت جداگانه و مستقل از دیپلوی است**؛ بدون آن، فرشته در کالاهایی که هنوز آیتمش `SystemActive=0` است در لیست دیده نمی‌شود.
2. دیپلوی **Api** (بدون SQL/migration).
3. وب‌اپ. اگر وب‌اپ زودتر بیاید: `storeItemAvailable` و `offers` و `preferredStoreId` وجود ندارند (`undefined`)؛ کد باید تحمل کند: `preferredStoreId ?? null`، `offers ?? []` و `storeItemAvailable === false` فقط وقتی `false` صریح است.
