# «کالاهای مشابه» و «در کنارش خریداری شده» هوشمند — مستند وب‌اپ

> مخاطب: تیم وب‌اپ (Nuxt 4) · وضعیت بک: **پیاده و build شد، دیپلوی نشده** (بدون SQL/migration). تست واحد بک: `Application.Tests/ProductRelations/*` (۲۲ تست سبز).
> دو endpoint جدید **عمومی (بدون لاگین)** و **ساده‌اند: پاسخ مستقیم `{ list, totalCount }`** (نه `{isSuccess,data}`)؛ هر عنصر `list` همان شکل `ProductVDto` جستجوی محصول است، پس `<ProductItem :item="el">` بدون تغییر کار می‌کند.

## ۱) مشکل قبلی

- «مشابه» = فقط `CategoryIds=[categoryId]` از `/api/products/products` (هر محصول دیگر همان دسته). دسته‌ی «بهداشت، مراقبت و نگهداری گربه» ۲۳۳ محصولِ ناهمگون دارد (خاک، ظرف، جای خواب، برس، اسکرچر…)، پس برای خاک گربه همه‌چیز می‌آمد.
- «در کنارش» = `/api/ProductRelate` = فقط لیست دست‌چین ادمین؛ معمولاً خالی.

## ۲) رفتار جدید بک (قرارداد)

### `GET /api/Product/Similar?productId={id}&take=12`
فقط **هم‌نوعِ دقیق** با **حیوان سازگار**: خاک گربه ← فقط خاک گربه؛ پد سگ ← فقط پد؛ هرگز پد سگ برای خاک گربه. موجودها اول؛ ناموجودِ هم‌نوع فقط وقتی جای خالی هست. اگر مطمئن نباشد **خالی** برمی‌گرداند (`list: []`)، نه کالای بی‌ربط.

### `GET /api/Product/BoughtTogether?productId={id}&take=8`
لوازمِ **مکمل** همان حیوان، فقط **موجود**، حداکثر ۲ کالا از هر نوع تا سبد «کامل» باشد:

| محصول مبدأ | پیشنهاد |
| --- | --- |
| خاک گربه | ظرف خاک، بیلچه، مت ظرف خاک، بوگیر خاک |
| جای خواب سگ | پد بهداشتی، تشویقی، اسباب‌بازی |
| جای خواب گربه | تشویقی، اسباب‌بازی (پد توالت برای گربه **نمی‌آید**) |
| غذای خشک | ظرف غذا، تشویقی، مکمل، کنسرو |
| تشویقی | اسباب‌بازی، وسایل آموزش، ظرف |
| قلاده | لیش، پلاک، هارنس، کیسه‌ی مدفوع |
| برس | شامپو، ناخن‌گیر، دستمال مرطوب |
| قفس پرنده | ظرف، نشیمنگاه، اسباب‌بازی، لانه |

ترتیب پاسخ: **دست‌چینِ ادمین** (`ProductRelates`، فقط فعال) ← بعد هوشمند (نوع‌های مکمل + اگر مشتریان واقعی هم‌زمان خریده‌اند امتیاز بیشتر). `take` حداکثر ۲۴.

نکته‌ی ایمنی: کالایی که نوعش مشخص نیست، ناموجود است یا حیوانش ناسازگار است **هرگز** نمی‌آید. اگر چیزی مناسب نباشد لیست خالی است.

## ۳) BFFهای جدید

**`app/server/api/products/similarProducts.get.js`**
```js
export default defineEventHandler(async (event) => {
  const { public: { baseUrl } } = useRuntimeConfig()
  try {
    return await $fetch(`${baseUrl}/api/Product/Similar`, {
      method: 'GET',
      headers: { Accept: 'application/json', 'Accept-Language': 'fa-IR' },
      query: getQuery(event), // { productId, take? }
    })
  } catch (error) {
    return { list: [], totalCount: 0 } // خطا = بخش را نشان نده (نه خطا به کاربر)
  }
})
```

**`app/server/api/products/boughtTogether.get.js`** — همان کد با آدرس `/api/Product/BoughtTogether`.

(BFF قدیمی `relatedProducts.get.js` را نگه دارید؛ دیگر در این کامپوننت استفاده نمی‌شود.)

## ۴) تغییر `app/components/products/similarProducts.vue`

### ۴-۱) `<script setup>`
جایگزین `data`/`related`/`onMounted`/computedها:

```js
const props = defineProps(["similarData", "src", "title"]);

const similar = ref([]);
const together = ref([]);

// ترتیب را بک تعیین کرده (موجودها اول)؛ sortProductsByAvailability فقط ناموجودها را آخر نگه می‌دارد
const sortedSimilarList = computed(() => sortProductsByAvailability(similar.value));
const sortedTogetherList = computed(() => sortProductsByAvailability(together.value));

// تب خالی اصلاً نمایش داده نشود (به‌جای NotFound)
const items = computed(() => [
  sortedSimilarList.value.length ? { key: "similar", label: "کالاهای مشابه", slot: "similar" } : null,
  sortedTogetherList.value.length ? { key: "together", label: "در کنارش خریداری شده", slot: "together" } : null,
].filter(Boolean));

const load = async () => {
  const productId = props?.similarData?.productId;
  if (!productId) { similar.value = []; together.value = []; return; }
  const [a, b] = await Promise.allSettled([
    $fetch("/api/products/similarProducts", { query: { productId, take: 12 } }),
    $fetch("/api/products/boughtTogether", { query: { productId, take: 8 } }),
  ]);
  similar.value = a.status === "fulfilled" ? (a.value?.list || []) : [];
  together.value = b.status === "fulfilled" ? (b.value?.list || []) : [];
};

onMounted(load);
// وقتی کاربر از صفحه‌ی یک کالا به کالای دیگر می‌رود (کامپوننت دوباره ساخته نمی‌شود)
watch(() => props?.similarData?.productId, load);
```

(`categoryId` در `similarData` دیگر لازم نیست؛ `index.vue` همان‌طور که هست بماند.)

### ۴-۲) `<template>`
- شرط ریشه: `v-if="items.length > 0"` (به‌جای شرط فعلی).
- slot اول همان `#similar` با `v-if="sortedSimilarList.length"`/`v-for="el in sortedSimilarList"`.
- slot دوم **`#related` را به `#together` تغییر دهید** و `v-for="el in sortedTogetherList"` بگذارید. (نام slot باید با `key` در `items` یکی باشد.)
- `<NotFound v-else />` را از هر دو تب بردارید (تب خالی دیگر ساخته نمی‌شود).
- متن‌ها بدون تغییر: «پیشنهادات و مکمل‌های خرید»، «کالاهای مشابه»، «در کنارش خریداری شده».

## ۵) چک‌لیست تست دستی

1. صفحه‌ی «خاک گربه …»: تب مشابه فقط خاک گربه‌های دیگر؛ تب «در کنارش» ظرف خاک/بیلچه/مت/بوگیر.
2. «پد بهداشتی سگ …»: مشابه فقط پد؛ «در کنارش» جای خواب سگ/تشویقی سگ/دستمال مرطوب.
3. «جای خواب گربه …»: «در کنارش» پد توالت ندارد.
4. «غذای خشک …»: ظرف غذا/تشویقی/کنسرو همان حیوان.
5. محصولی که فقط یک کالای هم‌نوع دارد یا نوعش ناشناخته است: تب مربوط پنهان می‌شود، هیچ کالای بی‌ربط نمی‌آید.
6. رفتن از یک محصول به محصول دیگر (بدون رفرش) لیست‌ها را عوض می‌کند (`watch`).
7. بک خطا بدهد یا Api هنوز دیپلوی نشده باشد ⇒ BFF لیست خالی می‌دهد و کل کارت نمایش داده نمی‌شود (ایمن).

## ۶) دیپلوی

فقط **Api**. ترتیب: اول Api، بعد وب‌اپ (اگر وب‌اپ زودتر بیاید BFFها ۴۰۴ می‌گیرند و کارت پنهان می‌ماند). پاسخ‌ها با `CustomOutputCache` کش می‌شوند (`Similar` هم‌گروه کش جستجوی محصول، `BoughtTogether` هم‌گروه کش «مرتبط»).

## ۷) نکته‌های داده (برای ادمین/فروشگاه)

- طبقه‌بندی از **نام کالا + نام دسته** است. نام کالا هرچه شفاف‌تر باشد («خاک گربه …»، «پد بهداشتی سگ …»، «جای خواب …») دقیق‌تر می‌شود. کالایی با نام مبهم (مثلاً «مدل X») نوعش «ناشناخته» حساب می‌شود و پیشنهاد نمی‌دهد/نمی‌گیرد.
- فقط کالاهای **موجود** (`status = موجود` و `price > 0`) به‌عنوان «در کنارش» می‌آیند؛ تا موجودی/قیمت کالاها در پنل کامل نشود، لیست‌ها کوتاه‌اند.
- دست‌چین ادمین (جدول `ProductRelates`) همیشه اول می‌ماند و روی هوشمند اولویت دارد.
