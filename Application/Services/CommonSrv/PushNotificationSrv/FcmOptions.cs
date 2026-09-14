namespace Application.Services.CommonSrv.PushNotificationSrv
{
    public class FcmOptions
    {
        public const string SectionName = "Fcm";

        // کل محتوای فایل JSON اکانت سرویس Firebase (Project Settings > Service accounts > Generate new
        // private key)، به‌صورت یک رشته‌ی خام. هرگز در appsettings واقعی/Git قرار نگیرد — از طریق
        // متغیر محیطی PASTIL_FCM_SERVICE_ACCOUNT_JSON تزریق می‌شود (نگاه کنید به SecretConfiguration.cs).
        public string ServiceAccountJson { get; set; }
    }
}
