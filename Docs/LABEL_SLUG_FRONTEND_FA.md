# قرارداد Slug برای پنل و سایت

## محدوده

فیلد `Slug` فقط برای بخش‌هایی ساخته می‌شود که در محتوا، فروشگاه یا مسیرهای سایت
کاربرد دارند:

| بخش | مبنای ساخت Slug |
|---|---|
| Role | `label` |
| Banner | `label` |
| Brand | `secondName` |
| Category | `label` |
| Feature | `label` |
| Gallery | `label` |
| PetBreed | `label` |
| Pet | `label` |
| Product | `productLabel` |

سایر فیلدهای `Label` پروژه هیچ ارتباطی با Slug ندارند و تغییری نکرده‌اند.

## رفتار بک‌اند

فرانت فقط فیلد مبنا را ارسال می‌کند. مقدار `slug` توسط بک‌اند تولید می‌شود و
مقداری که فرانت برای `slug` بفرستد، هنگام ذخیره بازنویسی خواهد شد.

```text
ProductLabel: SDFSD dsf fsdf
Slug:         sdfsd-dsf-fsdf
```

قواعد:

- حروف انگلیسی به حروف کوچک تبدیل می‌شوند.
- فاصله، `_`، `-` و جداکننده‌های متوالی به یک `-` تبدیل می‌شوند.
- علائم اضافی مانند `!@#$%^&*()` حذف می‌شوند.
- حروف فارسی و سایر حروف غیر ASCII در فیلد مبنا پذیرفته نمی‌شوند.
- اگر بعد از پاک‌سازی هیچ حرف انگلیسی یا عددی باقی نماند، درخواست ناموفق است.
- Slug در هر بخش تکراری نیست؛ برای مثال دو Product نمی‌توانند Slug یکسان داشته
  باشند.

## جلوگیری هنگام تایپ در پنل و سایت

این محدودیت را فقط روی فیلد مبنای بخش‌های جدول بالا اعمال کنید:

```ts
const SLUG_SOURCE_PATTERN = /^[A-Za-z0-9 _-]*$/

export const sanitizeSlugSource = (value: string) =>
  value.replace(/[^A-Za-z0-9 _-]/g, '')

export const previewSlug = (value: string) =>
  value
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9\s_-]+/g, '')
    .replace(/[\s_-]+/g, '-')
    .replace(/^-+|-+$/g, '')
```

نمونه Vue:

```vue
<input
  :value="form.productLabel"
  dir="ltr"
  autocomplete="off"
  @input="onProductLabelInput"
/>

<small v-if="form.productLabel">
  Slug: {{ previewSlug(form.productLabel) }}
</small>
```

```ts
const onProductLabelInput = (event: Event) => {
  const input = event.target as HTMLInputElement
  input.value = sanitizeSlugSource(input.value)
  form.productLabel = input.value
}
```

## درخواست و پاسخ Product

درخواست:

```json
{
  "productLabel": "SDFSD dsf fsdf"
}
```

پاسخ:

```json
{
  "productLabel": "SDFSD dsf fsdf",
  "slug": "sdfsd-dsf-fsdf"
}
```

برای Brand همین رفتار بر اساس `secondName` و برای سایر بخش‌ها بر اساس `label`
انجام می‌شود.
