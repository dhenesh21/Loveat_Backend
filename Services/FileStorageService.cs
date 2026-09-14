using Amazon.S3;
using Amazon.S3.Model;

namespace LovEat.API.Services
{
    /// <summary>
    /// P1 hardening: real file storage via AWS S3. Previously there was no
    /// upload mechanism at all in the API — every ImageUrl/DocumentUrl field
    /// across the codebase (chef documents, portfolio photos, review images,
    /// FSSAI licenses, etc.) was set by callers to placeholder strings with
    /// nothing that could actually store a real uploaded file. This gives
    /// every one of those a genuine place to upload to.
    /// </summary>
    public interface IFileStorageService
    {
        Task<(bool Success, string? Url, string? ErrorMessage)> UploadAsync(Stream fileStream, string fileName, string contentType, string folder);
        Task<bool> DeleteAsync(string fileUrl);
        bool IsConfigured { get; }
    }

    public class S3FileStorageService : IFileStorageService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<S3FileStorageService> _logger;

        public S3FileStorageService(IConfiguration config, ILogger<S3FileStorageService> logger)
        {
            _config = config;
            _logger = logger;
        }

        private string? BucketName => _config["ExternalServices:Storage:BucketName"];
        private string? AccessKey => _config["ExternalServices:Storage:AccessKey"];
        private string? SecretKey => _config["ExternalServices:Storage:SecretKey"];
        private string Region => _config["ExternalServices:Storage:Region"] ?? "ap-south-1";
        private string? PublicBaseUrl => _config["ExternalServices:Storage:PublicBaseUrl"];

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(BucketName) && BucketName != "REPLACE_VIA_ENV_VAR" &&
            !string.IsNullOrWhiteSpace(AccessKey) && AccessKey != "REPLACE_VIA_ENV_VAR" &&
            !string.IsNullOrWhiteSpace(SecretKey) && SecretKey != "REPLACE_VIA_ENV_VAR";

        public async Task<(bool Success, string? Url, string? ErrorMessage)> UploadAsync(Stream fileStream, string fileName, string contentType, string folder)
        {
            if (!IsConfigured)
            {
                _logger.LogWarning("File upload skipped for '{FileName}' — ExternalServices:Storage:BucketName/AccessKey/SecretKey not configured.", fileName);
                return (false, null, "File storage is not configured on this server.");
            }

            // Randomized key prefix avoids collisions/overwrites and avoids
            // leaking the original filename (which may contain PII) in the
            // public URL.
            var safeExtension = Path.GetExtension(fileName);
            var key = $"{folder.Trim('/')}/{Guid.NewGuid():N}{safeExtension}";

            try
            {
                using var client = new AmazonS3Client(AccessKey, SecretKey, Amazon.RegionEndpoint.GetBySystemName(Region));
                var request = new PutObjectRequest
                {
                    BucketName = BucketName,
                    Key = key,
                    InputStream = fileStream,
                    ContentType = contentType,
                    CannedACL = S3CannedACL.PublicRead,
                };
                await client.PutObjectAsync(request);

                var url = !string.IsNullOrWhiteSpace(PublicBaseUrl)
                    ? $"{PublicBaseUrl.TrimEnd('/')}/{key}"
                    : $"https://{BucketName}.s3.{Region}.amazonaws.com/{key}";
                return (true, url, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "S3 upload failed for '{FileName}'", fileName);
                return (false, null, "Could not upload the file. Please try again.");
            }
        }

        public async Task<bool> DeleteAsync(string fileUrl)
        {
            if (!IsConfigured) return false;
            try
            {
                var uri = new Uri(fileUrl);
                var key = uri.AbsolutePath.TrimStart('/');
                // Strip the public-base-url's own path prefix if PublicBaseUrl included one.
                using var client = new AmazonS3Client(AccessKey, SecretKey, Amazon.RegionEndpoint.GetBySystemName(Region));
                await client.DeleteObjectAsync(new DeleteObjectRequest { BucketName = BucketName, Key = key });
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "S3 delete failed for '{FileUrl}'", fileUrl);
                return false;
            }
        }
    }
}
