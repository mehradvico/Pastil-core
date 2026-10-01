namespace Application.Services.SchoolSrvs.SchoolCourseLiveSrv
{
    // مقادیر از appsettings/Environment (LiveKit__ApiKey و ...) می‌آیند - هیچ مقداری اینجا هاردکد نمی‌شود.
    public class LiveKitOptions
    {
        public const string SectionName = "LiveKit";

        // مثلاً https://your-project.livekit.cloud
        public string Host { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        // برای اعتبارسنجی امضای وب‌هوک Egress (هدر Authorization که LiveKit می‌فرستد)
        public string WebhookApiKey { get; set; }
        public string WebhookApiSecret { get; set; }
        // مقصد ذخیره‌ی خروجی ضبط (S3-compatible) - Egress مستقیم همین‌جا آپلود می‌کند
        public string RecordingBucket { get; set; }
        public string RecordingRegion { get; set; }
        public string RecordingAccessKey { get; set; }
        public string RecordingSecret { get; set; }
        public string RecordingPublicBaseUrl { get; set; }
    }
}
