# پرسش و پاسخ کالا به‌صورت «چت‌روم» — مستند وب‌اپ (و ادمین)

> مخاطب: تیم وب‌اپ (Nuxt 4) · وضعیت بک و پنل: **پیاده شد، دیپلوی نشده**. **اول SQL (`backend/scripts/DiscussionChatRoom.sql`) را کاربر اجرا کند، بعد Api، بعد وب‌اپ/پنل.** (۳ ستون جدید روی `DiscussionAnswers`؛ migration معادل `20261113090000_DiscussionChatRoom`.)
> پاسخ‌های بک **HTTP 200** با `{ isSuccess, messages, data }` هستند؛ جز `GET /api/DiscussionQuestion` (لیست `{list,totalCount}`) و `MyStatus` (شیء ساده). موفقیت را از `isSuccess` و متن خطا را از `messages[0].item1` (فارسی) بخوانید.

## ۱) رفتار مطلوب (همان خواسته)

1. صفحه‌ی کالا مثل یک چت‌روم است: **هر کاربر لاگین‌شده** (مشتری یا پرسنل فروشگاه) می‌تواند پرسش بگذارد، **به پرسش** یا **به هر پاسخِ تأییدشده‌ی دیگران** جواب بدهد (ریپلای).
2. هر پیام (پرسش/پاسخ) **تا تأیید ادمین برای عموم نمایش داده نمی‌شود**.
3. تا وقتی ادمین **پیام قبلیِ همان کاربر** را بررسی نکرده، او **نمی‌تواند پیام جدید بفرستد** (کل کالاها؛ `hasPending`).
4. بعد از ارسال: پیام «پیام شما ثبت شد؛ منتظر تأیید ادمین باشید» + **خودِ پیام برای خودِ کاربر با نشان «در انتظار تأیید» نمایش داده می‌شود**.
5. **قبل از ارسال** پیش‌نمایش و «آیا از ارسال این پیام مطمئن هستید؟» با دکمه‌های **ارسال / ویرایش / انصراف**. بعد از ارسال هم تا وقتی ادمین بررسی نکرده، کاربر می‌تواند **ویرایش یا حذف** کند.
6. پاسخِ پرسنل فروشگاهی که کالا را می‌فروشد با نشان **«پاسخ فروشگاه X»** نمایش داده می‌شود.

## ۲) قراردادهای API

### خواندن (عمومی) — `GET /api/DiscussionQuestion?productId=&pageSize=` (بدون تغییر مسیر)
فقط پرسش‌های تأییدشده و **فقط پاسخ‌های تأییدشده**. فیلدهای جدید هر پاسخ در `discussionAnswers[]`:

| فیلد | معنی |
| --- | --- |
| `replyToAnswerId` | `null` = پاسخ مستقیم به پرسش؛ وگرنه شناسه‌ی پاسخی که به آن ریپلای شده (در همان لیست هست) |
| `isStoreAnswer`, `storeId`, `storeName` | پاسخ فروشگاه |
| `user.fullName` | نام نویسنده (مثل قبل) |

### ثبت پرسش — `POST /api/EndUser/DiscussionQuestion` (لاگین)
بدنه: `{ "content": "…", "productId": 812 }` (۳ تا ۱۰۰۰ نویسه). `adminConfirm/active/answerCount` فرستاده شود هم **نادیده گرفته می‌شود**؛ همیشه «در انتظار» ثبت می‌شود. پاسخ موفق: `data.id`.

### ثبت پاسخ/ریپلای — `POST /api/EndUser/DiscussionAnswer` (لاگین)
بدنه: `{ "content": "…", "discussionQuestionId": 55, "replyToAnswerId": 901 }` (`replyToAnswerId` اختیاری؛ حذف = پاسخ به خود پرسش). پرسش باید تأییدشده و پاسخِ هدف تأییدشده و از همان پرسش باشد.

### خطاهای ثبت (`isSuccess=false`، `messages[0].item1`)
| وضعیت | پیام |
| --- | --- |
| پیام قبلی تأییدنشده دارد | «پیام قبلی شما هنوز در انتظار تأیید ادمین است؛ بعد از بررسی می‌توانید پیام جدید بفرستید.» |
| متن کوتاه/بلند | «متن پیام باید بین ۳ تا ۱۰۰۰ نویسه باشد.» |
| پرسش تأیید نشده/حذف‌شده | «این پرسش برای پاسخ‌دادن در دسترس نیست.» |
| هدفِ ریپلای نامعتبر | «پیامی که به آن پاسخ می‌دهید معتبر نیست.» |
| کالا نامعتبر | «کالا معتبر نیست.» |

### وضعیت پیام‌های من — `GET /api/EndUser/DiscussionChat/MyStatus?productId=` (لاگین)
```json
{
  "hasPending": true,
  "items": [
    { "kind": 1, "id": 77, "content": "…", "createDate": "…", "status": 0, "discussionQuestionId": 77, "replyToAnswerId": null },
    { "kind": 2, "id": 903, "content": "…", "createDate": "…", "status": 2, "discussionQuestionId": 55, "replyToAnswerId": 901 }
  ]
}
```
`kind`: ۱ پرسش، ۲ پاسخ · `status`: ۰ در انتظار تأیید، ۲ رد شده (تا ۷ روز نشان بده). **`hasPending` سراسری است (همه‌ی کالاها)**؛ `items` فقط برای همین کالا.

### ویرایش / حذف پیامِ در انتظار (فقط صاحب پیام و فقط `status=0`)
- `PUT /api/EndUser/DiscussionChat` بدنه `{ "kind": 1|2, "id": 77, "content": "…" }`
- `DELETE /api/EndUser/DiscussionChat?kind=1&id=77`

خطا اگر دیگر در انتظار نباشد: «فقط پیامی که هنوز در انتظار تأیید ادمین است قابل ویرایش یا حذف است.» (حذف پیام در انتظار، قفل «پیام جدید» را برمی‌دارد.)

## ۳) BFFهای جدید (`app/server/api/DiscussionChat/`)

همه با الگوی BFFهای فعلی (`Authorization: Bearer <cookie token>`):

**`myStatus.get.js`**
```js
export default defineEventHandler(async (event) => {
  const { public: { baseUrl } } = useRuntimeConfig()
  const token = getCookie(event, 'token')
  try {
    return await $fetch(`${baseUrl}/api/EndUser/DiscussionChat/MyStatus`, {
      headers: { Accept: 'application/json', Authorization: `Bearer ${token}` }, query: getQuery(event) })
  } catch { return { hasPending: false, items: [] } }
})
```
**`edit.put.js`** — `PUT` به `/api/EndUser/DiscussionChat` با `body = await readBody(event)`؛ در خطا `return normalizeApiError(error, 'خطا در برقراری ارتباط با سرور')`.
**`remove.delete.js`** — `DELETE` به `/api/EndUser/DiscussionChat` با `query: getQuery(event)` (`kind`, `id`).

(BFFهای موجود `DiscussionQuestion.post.js` و `DiscussionAnswer.post.js` همان می‌مانند؛ فقط بدنه‌ی پاسخ می‌تواند `replyToAnswerId` داشته باشد.)

## ۴) تغییر `app/components/products/question.vue`

ساختار: همان کارت، ولی «لیست افقی کوتاه» + مودال «همه‌ی پرسش‌ها» حالا یک **ترد چت** است.

### ۴-۱) state و لود وضعیت من
```js
const my = ref({ hasPending: false, items: [] })
const loadMy = async () => {
  if (!authStore.userData) { my.value = { hasPending: false, items: [] }; return }
  try { my.value = await $fetch('/api/DiscussionChat/myStatus', { query: { productId: productIdValue() } }) } catch {}
}
onMounted(loadMy)
const myQuestions = computed(() => my.value.items.filter(i => i.kind === 1))
const myAnswersOf = (qid) => my.value.items.filter(i => i.kind === 2 && i.discussionQuestionId === qid)
const blocked = computed(() => my.value.hasPending)   // composer غیرفعال + اعلان
```

### ۴-۲) پیش‌نمایش «آیا مطمئن هستید؟» (قبل از ارسال) — **یک مودال مشترک** برای پرسش و پاسخ
```js
const draft = ref(null)          // { type: 'question'|'answer', content, questionId?, replyToAnswerId?, replyToText? }
const confirmModal = ref(null)   // <BottomModal ref="confirmModal">
const askConfirm = (d) => { draft.value = { ...d }; confirmModal.value?.openModal() }
const sending = ref(false)
const confirmSend = async () => {
  if (sending.value || !draft.value) return
  sending.value = true
  const d = draft.value
  try {
    const res = d.type === 'question'
      ? await $fetch('/api/DiscussionQuestion/DiscussionQuestion', { method: 'POST', body: { content: d.content.trim(), productId: productIdValue() } })
      : await $fetch('/api/DiscussionAnswer/DiscussionAnswer', { method: 'POST', body: { content: d.content.trim(), discussionQuestionId: d.questionId, replyToAnswerId: d.replyToAnswerId || undefined } })
    if (res?.isSuccess) {
      toast.add({ title: 'پیام شما ثبت شد', description: 'منتظر تأیید ادمین باشید؛ پس از تأیید برای همه نمایش داده می‌شود.', color: 'green' })
      confirmModal.value?.closeModal(); resetComposers(); draft.value = null
      await loadMy(); emit('sent')
    } else {
      toast.add({ title: res?.messages?.[0]?.item1 || 'ثبت پیام ناموفق بود', color: 'red' })
      await loadMy()   // اگر مثلاً قفل «پیام قبلی» برقرار است نشانش بده
    }
  } catch { toast.add({ title: 'ثبت پیام ناموفق بود', color: 'red' }) }
  finally { sending.value = false }
}
```
محتوای مودال: عنوان «آیا از ارسال این پیام مطمئن هستید؟»؛ متن پیام داخل کادر (و اگر ریپلای است، خلاصه‌ی پیامی که به آن جواب می‌دهد)؛ یادآوری «پیام پس از تأیید ادمین نمایش داده می‌شود»؛ سه دکمه: **«بله، ارسال»** (`confirmSend`)، **«ویرایش»** (فقط مودال را ببندد؛ متن در فرم می‌ماند)، **«انصراف»** (مودال را ببندد و `resetComposers()`).

دکمه‌های «ثبت و انتشار پرسش» و «ارسال پاسخ من» دیگر مستقیم POST نمی‌کنند؛ فقط `askConfirm({...})` را صدا می‌زنند (و اگر `blocked` است غیرفعال‌اند).

### ۴-۳) اعلان قفل
وقتی `blocked`: بالای فرم‌ها (پرسش و پاسخ) یک نوار زرد:
«پیام قبلی شما در انتظار تأیید ادمین است. بعد از بررسی می‌توانید پیام جدید بفرستید.»
و `textarea` و دکمه‌ها `:disabled="blocked"`.

### ۴-۴) نمایش ترد (در مودال «همه‌ی پرسش‌ها»)
برای هر `item` از `Allcomments.list`:
1. خود پرسش (نام، تاریخ، متن) + دکمه‌ی **«پاسخ»** (باز کردن composer با `{type:'answer', questionId:item.id, replyToAnswerId:null}`).
2. پاسخ‌ها به ترتیب `id` صعودی (`[...item.discussionAnswers].sort((a,b)=>a.id-b.id)`). هر پاسخ:
   - نام نویسنده؛ اگر `isStoreAnswer`: نشان سبز «پاسخ فروشگاه {{ storeName }}».
   - اگر `replyToAnswerId`: نقل‌قول کوچک بالای متن: «در پاسخ به {{ نام نویسنده‌ی هدف }}: {{ ۶۰ نویسه‌ی اول متن هدف }}» (هدف را با `find` در همان `discussionAnswers` پیدا کنید).
   - دکمه‌ی **«پاسخ»** → composer با `replyToAnswerId = answer.id` و `replyToText` برای پیش‌نمایش.
3. **پیام‌های در انتظارِ خودِ کاربر** همین‌جا، رنگ زرد کم‌رنگ، نشان «در انتظار تأیید ادمین» (و برای `status=2` نشان قرمز «رد شد»)، با دکمه‌های **ویرایش** و **حذف** (فقط `status=0`):
   - `myAnswersOf(item.id)` زیر پاسخ‌های همان پرسش.
   - `myQuestions` به‌صورت کارت جدا بالای لیست («پرسش شما (در انتظار تأیید)»). چون پرسش تأییدنشده در `list` عمومی نیست.
4. ویرایش: همان `textarea` را با متن فعلی باز کنید و `PUT /api/DiscussionChat/edit` ( `{kind,id,content}` ) بزنید؛ حذف: confirm سبک سپس `DELETE /api/DiscussionChat/remove?kind=&id=`؛ در هر دو بعد `await loadMy()`.
5. composer پاسخ **یکی** است (نه یکی برای هر پرسش): `answerText`, `answerTarget = {questionId, replyToAnswerId, replyToText}`؛ هنگام باز شدن، بالای آن «در حال پاسخ به: …» با ✕ برای برگشت به «پاسخ به خود پرسش».

### ۴-۵) تغییرات کوچک
- از بدنه‌ی POST پرسش `adminConfirm: false` را بردارید (بک نادیده می‌گیرد).
- Swiper کوتاه روی صفحه: فقط نمایش؛ نشان «پاسخ فروشگاه» روی اولین پاسخ اگر `isStoreAnswer`.
- کاربر لاگین نیست: دکمه‌ها به `/loginregister` (مثل الان).

## ۵) پنل ادمین (انجام شد در `panel/`)

- **لیست پرسش‌ها:** فیلتر جدید «پرسش در انتظار تأیید» و «دارای پاسخ در انتظار تأیید»؛ نشان زرد «N در انتظار تأیید» کنار شمارنده‌ی پاسخ‌ها.
- **جزئیات پرسش:** وضعیت هر پاسخ (در انتظار / فعال / رد شده)، نشان «پاسخ فروشگاه: X»، «در پاسخ به #id»، و دکمه‌ی **«رد پاسخ»** کنار «تأیید و نمایش».
- تأیید/رد پاسخ همان `PUT /api/Admin/DiscussionAnswerActive` است: `active=true` ⇒ تأیید (`AdminConfirm=true`)، `active=false` ⇒ رد (`AdminConfirm=false`، **قفل ارسال برای نویسنده برداشته می‌شود**). پرسش مثل قبل با «تأیید و نمایش / رد پرسش».
- **نیاز به build/deploy مجدد پنل** (`panel/output.zip`) و Permission Sync لازم نیست (endpoint جدید ادمین اضافه نشد).

## ۶) نکته‌های امنیتی که در بک اصلاح شد

- قبلاً کاربر می‌توانست در بدنه‌ی پرسش `adminConfirm: true, active: true` بفرستد و **خودش را تأیید کند**؛ حالا نادیده گرفته می‌شود.
- `GET /api/EndUser/DiscussionQuestion` (لیست/شناسه) قبلاً **پرسش‌های تأییدنشده** را هم برمی‌گرداند؛ حالا فقط تأییدشده.
- `GET /api/EndUser/DiscussionAnswer` برای کاربر لاگین فقط پاسخ‌های خودش را می‌داد؛ حالا لیست عمومی تأییدشده‌هاست.
- پاسخ حذف‌شده‌ی نرم دیگر نمایش داده نمی‌شود.

## ۷) چک‌لیست تست

1. کاربر A پرسش می‌نویسد ← پیش‌نمایش «مطمئن هستید؟» ← «ویرایش» متن را نگه می‌دارد ← «بله، ارسال» ← toast «منتظر تأیید ادمین» ← پرسش با نشان «در انتظار تأیید» فقط برای A دیده می‌شود.
2. A بلافاصله پرسش دوم یا پاسخ می‌نویسد ← فرم قفل با نوار زرد (و اگر دور زد، سرور پیام «پیام قبلی شما…» می‌دهد).
3. A پیام در انتظارش را ویرایش/حذف می‌کند؛ بعد از حذف قفل برداشته می‌شود.
4. ادمین پرسش را تأیید می‌کند ← برای همه نمایش؛ قفل A برداشته؛ ویرایش/حذف دیگر ممکن نیست.
5. کاربر B (مشتری) به پرسش جواب می‌دهد ← در انتظار؛ ادمین تأیید ← دیده می‌شود؛ A روی پاسخ B «پاسخ» می‌زند ← در انتظار ← پس از تأیید با نقل‌قول «در پاسخ به B» دیده می‌شود.
6. پرسنل فروشگاه (که کالا را می‌فروشد) پاسخ می‌دهد ← پس از تأیید با «پاسخ فروشگاه X».
7. ادمین پیام را «رد» می‌کند ← نویسنده ۷ روز نشان «رد شد» می‌بیند و می‌تواند پیام جدید بفرستد.
8. پاسخ به پاسخِ تأییدنشده/حذف‌شده یا پاسخ به پرسش تأییدنشده ← خطای سرور.
9. پیام کمتر از ۳ یا بیشتر از ۱۰۰۰ نویسه ← خطا.
