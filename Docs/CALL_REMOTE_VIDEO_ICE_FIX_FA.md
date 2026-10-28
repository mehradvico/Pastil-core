# رفع «هر طرف فقط دوربین خودش را می‌بیند» در تماس تصویری — تاریخ ۲۰۲۶-۱۰-۲۸

## علت
- سیگنالینگ سالم است: `Api/Hubs/CallHub.cs` در `JoinSessionCall` (خط ۱۴۶–۲۱۴) هر دو طرف را در گروه `call-{-sessionId}` می‌گذارد و `SendSignal` (خط ۴۷۰–۴۸۸) offer/answer/ice را به `OthersInGroup` رله می‌کند. ساخت جلسه‌ی مشاوره هم درست است (`ConsultationSessionService.cs:224-225`: Initiator = نماینده‌ی کلینیک/مربی، Target = مشتری) و به CompanionId وابسته نیست.
- مشکل در مسیر رسانه (ICE) است: وب‌اپ `iceServers` را از `runtimeConfig.public.turnServers` می‌خواند (`webapp/app/components/call/CallScreen.vue:430,457`) که پیش‌فرضش خالی است (`webapp/nuxt.config.ts:286`؛ در خروجی بیلد `.output/server/chunks/_/nitro.mjs` هم `"turnServers": ""`). یعنی اگر روی سرور `NUXT_PUBLIC_TURN_SERVERS` تنظیم نشده باشد، وب‌اپ فقط STUN گوگل دارد. پشت CGNAT موبایل ایران بدون TURN، offer/answer رد و بدل می‌شود ولی مسیر رسانه ساخته نمی‌شود ⇒ هر کس فقط پیش‌نمایش دوربین خودش را می‌بیند. TURN (`46.34.163.222:3478`) فقط در سند فلاتر آمده و تنظیم آن در هر کلاینت جداست.

## تغییر بک (انجام شد، commit نشده، بدون مایگریشن)
- `GET /api/EndUser/CallIceServers` (احراز هویت لازم، `Cache-Control: no-store`):
```json
{ "iceServers": [
  { "urls": ["stun:46.34.163.222:3478"], "username": null, "credential": null },
  { "urls": ["turn:...:3478?transport=udp", "turn:...:3478?transport=tcp"], "username": "...", "credential": "..." }
] }
```
- تنظیمات (appsettings/Environment، مقدار در گیت نگذارید):
  - `CallIce__StunUrls__0`، `CallIce__TurnUrls__0`، `CallIce__TurnUrls__1`
  - حالت فعلی coturn (رمز ثابت): `CallIce__TurnUsername`، `CallIce__TurnCredential`
  - حالت توصیه‌شده (credential کوتاه‌عمر): در coturn `use-auth-secret` + `static-auth-secret=...` و در بک `CallIce__TurnSharedSecret` (اولویت با این است)؛ `CallIce__CredentialTtlMinutes` پیش‌فرض ۱۸۰.
- اگر هیچ تنظیمی نباشد، آرایه‌ی خالی برمی‌گردد (کلاینت به STUN پیش‌فرض خودش برگردد).

## کار وب‌اپ (Arman)
در `CallScreen.vue` قبل از `createPeerConnection` همین endpoint را بگیرید و `iceServers` را از پاسخ بسازید (اگر خطا/خالی بود، `buildIceServers(turnServers)` فعلی به‌عنوان fallback). `ICE_SERVERS` را از ثابت سطح ماژول به مقدار گرفته‌شده در لحظه‌ی تماس تغییر دهید. تا آن موقع، راه سریع: روی سرور وب‌اپ `NUXT_PUBLIC_TURN_SERVERS` را با JSON آرایه‌ی RTCIceServer (همان TURN) تنظیم و ری‌استارت کنید.

## کار اپ فلاتر (AmirMohsen)
به‌جای رمز ثابت در `--dart-define`، `iceServers` را از همین endpoint بگیرید (قالب `urls/username/credential` همان است).

## تست روی دستگاه (هنوز انجام نشده)
1. دو گوشی روی دیتای دو اپراتور مختلف، تماس تصویری مشاوره‌ی کلینیک و مربی.
2. با `?callDebug` در وب‌اپ: `pair` باید `relay` یا `srflx` باشد و `video recv` افزایش یابد.
3. برای اطمینان از سلامت TURN، موقتاً `iceTransportPolicy: 'relay'`.
