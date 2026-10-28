namespace Application.Services.CommonSrv.CallIceSrv
{
    // مقادیر از appsettings/Environment (CallIce__TurnUrls__0، CallIce__TurnSharedSecret و ...) می‌آیند - هیچ مقداری اینجا هاردکد نمی‌شود.
    public class CallIceOptions
    {
        public const string SectionName = "CallIce";

        // مثلاً stun:host:3478
        public string[] StunUrls { get; set; }
        // مثلاً turn:host:3478?transport=udp و turn:host:3478?transport=tcp
        public string[] TurnUrls { get; set; }
        // اگر coturn با use-auth-secret/static-auth-secret اجرا شود: credential کوتاه‌عمر (REST API کوترن) ساخته می‌شود
        public string TurnSharedSecret { get; set; }
        // در غیر این صورت: long-term credential ثابت (lt-cred-mech)
        public string TurnUsername { get; set; }
        public string TurnCredential { get; set; }
        public int CredentialTtlMinutes { get; set; } = 180;
    }
}
