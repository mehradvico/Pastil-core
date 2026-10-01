namespace Application.Common.Storage
{
    /// <summary>
    /// Liara Object Storage (S3-compatible) connection settings. Values are empty by
    /// default in appsettings and overridden from env vars via SecretConfiguration -
    /// same pattern as LiveKitOptions/ShippingOptions.
    /// </summary>
    public class ObjectStorageOptions
    {
        public const string SectionName = "ObjectStorage";

        public string Endpoint { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(Endpoint)
            && !string.IsNullOrWhiteSpace(AccessKey)
            && !string.IsNullOrWhiteSpace(SecretKey)
            && !string.IsNullOrWhiteSpace(BucketName);
    }
}
