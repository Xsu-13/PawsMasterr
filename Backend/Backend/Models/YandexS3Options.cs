namespace Backend.Models
{
    public class YandexS3Options
    {
        public string ServiceUrl { get; set; } = string.Empty; // e.g. https://storage.yandexcloud.net
        public string BucketName { get; set; } = string.Empty;
        public string AccessKeyId { get; set; } = string.Empty;
        public string SecretAccessKey { get; set; } = string.Empty;
        public string PublicBaseUrl { get; set; } = string.Empty; // e.g. https://storage.yandexcloud.net/{bucket}
        public string FolderPrefix { get; set; } = string.Empty; // optional, e.g. images/
    }
}


