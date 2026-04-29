using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EventEase.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
        {
            _logger = logger;

            try
            {
                var connectionString = configuration.GetConnectionString("AzureStorage");
                var containerName = configuration["AzureStorageConfig:ContainerName"];

                _logger.LogInformation($"Connection string: {connectionString}");
                _logger.LogInformation($"Container name: {containerName}");

                var blobServiceClient = new BlobServiceClient(connectionString);
                _containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                _containerClient.CreateIfNotExists(PublicAccessType.Blob);
                _logger.LogInformation("Blob storage initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize BlobStorageService");
                throw;
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string oldImageUrl = null)
        {
            _logger.LogInformation($"Uploading file: {file?.FileName}, Size: {file?.Length}");

            try
            {
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var blobClient = _containerClient.GetBlobClient(fileName);

                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
                }

                _logger.LogInformation($"File uploaded successfully. URL: {blobClient.Uri}");

                if (!string.IsNullOrEmpty(oldImageUrl) && (oldImageUrl.Contains("127.0.0.1") || oldImageUrl.Contains("localhost")))
                {
                    await DeleteImageAsync(oldImageUrl);
                }

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                throw;
            }
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl)) return;

                var uri = new Uri(imageUrl);
                var blobName = Path.GetFileName(uri.LocalPath);
                var blobClient = _containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image");
            }
        }
    }
}