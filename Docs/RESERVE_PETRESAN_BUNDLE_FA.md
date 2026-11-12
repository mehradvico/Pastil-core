# پت‌رسانِ همراه رزرو مراکز (کلینیک، آرایشگاه، مربی، پانسیون) — ۲۰۲۶-۱۱-۱۱

## خلاصه
- سفر پت‌رسان دقیقاً برای **زمان رزرو** ساخته می‌شود: زمان حرکت راننده = زمان تحویل پت به مرکز منهای **۶۰ یا ۱۲۰ دقیقه** (`scheduledLeadMinutes`).
- اگر کاربر سفر را **قبل از پرداخت رزرو** ثبت کند، سفر «همراه رزرو» است (`paidWithReserve = true`) و مشتری **یک پرداخت** می‌کند: مبلغ رزرو + مبلغ پت‌رسان.
- تا رزرو پرداخت نشده، سفر به هیچ راننده‌ای نمایش داده نمی‌شود، ادمین نمی‌تواند راننده انتخاب کند و jobهای لغو خودکار/هشدار سراغش نمی‌روند. بعد از پرداخت موفق، سفر «پرداخت‌شده» می‌شود و همان لحظه به راننده‌ها اعلام می‌شود.
- اگر رزرو **قبلاً پرداخت شده** باشد، رفتار قبلی می‌ماند: سفر فوراً اعلام می‌شود و پرداختش جدا (بعد از قبول راننده) است.
- **مرکز** (مالک، ارائه‌دهنده، پرسنل) فقط مبلغ رزرو خودش را می‌بیند؛ مبلغ پت‌رسان برایش صفر و `amountsHidden = true` است.
- **ادمین** هر دو مبلغ را جدا می‌بیند: `paymentPrice` (رزرو)، `petResanTrip.paymentPrice` (راننده) و `totalPrice` (جمع پرداخت مشتری).

## وب‌اپ (پیاده‌سازی شد ۲۰۲۶-۱۱-۱۲)
- `pages/{clinic,coach}/finalize-center-reserve-[id].vue`، `pages/groomer/finalize-reserve-[id].vue`: قبل از `pay` سفر با `api.me.createTrip` ثبت می‌شود؛ اگر تیک برداشته شده و سفرِ منتظر پرداخت هست، `api.me.discardTrip`. مبلغ داک/صورتحساب = نوبت + پت‌رسان (`payableTotal`).
- `pages/pansion/finalize-reserve-[id].vue`: سفر روی رزرو اول، با امضای بدنه (تغییر مبدأ/مقصد = ثبت دوباره)، حذف با `DELETE /api/petresan/pansionReservation/{id}`.
- `components/reserve/{BottomDock,InvoiceReceipt,PetResanCostCard}.vue`: حالت «شامل همین پرداخت» با پراپ‌های `extra-included` / `pet-resan-included` (پیش‌فرض‌ها رفتار قبلی را حفظ می‌کنند؛ مدرسه/نماینده تغییری ندارند).
- پروکسی مربی حالا فقط `TripCoachReservation` را از مسیرهای Trip مجاز می‌کند.

## ترتیب پیشنهادی در اپ
1. ساخت رزرو (مثل قبل).
2. اگر کاربر پت‌رسان خواست: ثبت سفر متصل (پایین) — **قبل از** رفتن به درگاه.
3. دریافت جزئیات رزرو: `totalPrice` را به‌عنوان «مبلغ قابل پرداخت» نشان دهید (رزرو + پت‌رسان، هر دو ردیف جدا).
4. شروع پرداخت رزرو با همان endpointهای قبلی (`*ReservePayment`) — مبلغ درگاه خودش شامل پت‌رسان است. کیف پول هم روی کل مبلغ حساب می‌شود.
5. بعد از پرداخت، `GET` سفر متصل وضعیت راننده را می‌دهد.

اگر کاربر بعد از ثبت سفر، دوباره سفر ثبت کند (نشانی/فاصله‌ی دیگر)، درخواستِ قبلیِ منتظر پرداخت خودکار جایگزین می‌شود.

## Endpointها (EndUser)
| مرکز | ثبت سفر | دیدن سفر |
|---|---|---|
| کلینیک (فقط حضوری) | `POST api/EndUser/TripClinicReservation` | `GET api/EndUser/TripClinicReservation/{clinicReserveId}` |
| آرایشگاه | `POST api/EndUser/TripGroomerReservation` | `GET api/EndUser/TripGroomerReservation/{groomerReserveId}` |
| مربی (فقط پانسیون آموزشی در مرکز، mode 38) — **جدید** | `POST api/EndUser/TripCoachReservation` | `GET api/EndUser/TripCoachReservation/{coachReserveId}` |
| پانسیون | `POST api/EndUser/TripPansionReservation` (مسیر قبلی) | مسیر قبلی |

برداشتن پت‌رسانِ منتظر پرداخت (قبل از پرداخت رزرو): `DELETE api/EndUser/Trip{Clinic|Groomer|Coach|Pansion}Reservation/{reserveId}`.

بدنه‌ی مربی: `{ coachReserveId, origin, destination, fromAddress, toAddress, scheduledLeadMinutes: 60|120, ownerRidesAlong, tripOptionIds, dropOffTime? }` — `dropOffTime` ("HH:mm") فقط وقتی لازم است که رزرو ساعت ندارد.

## فیلدهای جدید
- `TripVDto/TripDto`: `coachReserveId`, `paidWithReserve`.
- `petResanTrip` روی رزرو کلینیک/آرایشگاه/مربی/پانسیون (جزئیات و لیست‌ها): `paidWithReserve`, `amountsHidden`, `scheduledDepartureAt`, `scheduledLeadMinutes` (به‌علاوه‌ی فیلدهای قبلی).
- `totalPrice` روی رزرو کلینیک/آرایشگاه/مربی (پانسیون از قبل داشت).

## لغو، جابه‌جایی، برگشت پول
- لغو رزرو ⇐ سفر متصل هم لغو می‌شود (آرایشگاه و مربی هم اضافه شدند). سفرِ پرداخت‌نشده بی‌صدا بسته می‌شود؛ سفرِ همراه رزروِ پرداخت‌شده کل مبلغ سفر را به کیف پول برمی‌گرداند (مبلغ رزرو طبق قوانین لغو خود مرکز).
- جابه‌جایی زمان رزرو (کلینیک، آرایشگاه، مربی) ⇐ زمان حرکت سفر هم با همان فاصله‌ی ۶۰/۱۲۰ جابه‌جا می‌شود (تا قبل از «پت سوار شد»).
- اگر پرداخت رزرو رها شود، سفر منتظر در لحظه‌ی موعد حرکت خودکار بسته می‌شود.

## استقرار
1. اجرای `backend/scripts/ReservePetResanBundle.sql` (ستون‌های `Trips.CoachReserveId`, `Trips.PaidWithReserve`؛ در صورت نبودن `Trips.GroomerReserveId` را هم می‌سازد).
2. دیپلوی **Api و Payment با هم** (منطق پرداخت در هر دو است).

## موارد باز
- پرداخت دستی ادمین برای رزرو حالا مبلغ پت‌رسانِ منتظر را هم شامل می‌شود.
- لغو دیرهنگامِ بدون برگشت پولِ رزرو: مبلغ سفر کامل به کیف پول برمی‌گردد (سفر انجام نشده) — تصمیم تأییدشده ۲۰۲۶-۱۱-۱۲.
- رزروهای خدمت نماینده (Companion) و مدرسه تغییری نکردند (پرداخت جدا).
