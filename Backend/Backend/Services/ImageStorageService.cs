using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Backend.Models;

namespace Backend.Services
{
    public interface IImageStorageService
    {
        Task<string> UploadAsync(Stream content, string contentType, string fileName, string category, CancellationToken cancellationToken = default);
    }
    public class YandexS3ImageStorageService : IImageStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly YandexS3Options _options;

        public YandexS3ImageStorageService(YandexS3Options options)
        {
            _options = options;
            var credentials = new BasicAWSCredentials(options.AccessKeyId, options.SecretAccessKey);
            var config = new AmazonS3Config
            {
                ServiceURL = options.ServiceUrl,
                ForcePathStyle = true,
                SignatureVersion = "v4"
            };
            _s3Client = new AmazonS3Client(credentials, config);
        }

        public async Task<string> UploadAsync(Stream content, string contentType, string fileName, string category, CancellationToken cancellationToken = default)
        {
            var safeFileName = $"{Guid.NewGuid()}_{fileName}";
            var keyPrefix = string.IsNullOrWhiteSpace(_options.FolderPrefix) ? string.Empty : _options.FolderPrefix.Trim('/').Trim() + "/";
            var key = $"{keyPrefix}{category}/{safeFileName}";

            var putRequest = new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                InputStream = content,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead
            };

            await _s3Client.PutObjectAsync(putRequest, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
            {
                var baseUrl = _options.PublicBaseUrl.TrimEnd('/');
                return $"{baseUrl}/{key}";
            }

            // Fallback to default Yandex endpoint
            var serviceUrl = _options.ServiceUrl.TrimEnd('/');
            return $"{serviceUrl}/{_options.BucketName}/{key}";
        }
    }
}


