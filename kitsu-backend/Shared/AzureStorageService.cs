using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace kitsu_backend.Shared
{
    public static class AzureStorageService
    {
        static string? ConnectionString = string.Empty;
        static string BlobContainer = "images-test";

        public static void Initialize(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            ConnectionString = configuration.GetConnectionString("AzureStorage");
            BlobContainer = hostEnvironment.IsProduction() ? "images" : "images-test";
        }
        public static async Task<string?> PostImage(IFormFile image, string storagePath)
        {
            try
            {
                var imageId = Guid.NewGuid().ToString();
                var containerClient = new BlobContainerClient(ConnectionString, BlobContainer);

                var blobName = $"{storagePath}/{imageId + Path.GetExtension(image.FileName)}";
                var blobClient = containerClient.GetBlobClient(blobName);

                using var stream = image.OpenReadStream();
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = string.IsNullOrEmpty(image.ContentType) ? "image/jpeg" : image.ContentType }
                });

                return Path.GetFileName(blobName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading image: {ex.Message}");
                return null;
            }
        }

        public static async Task DeleteImage(string fileName, string storagePath)
        {
            var containerClient = new BlobContainerClient(ConnectionString, "images-test");

            var blobClient = containerClient.GetBlobClient($"{storagePath}/{fileName}.jpg");

            var result = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.None);
        }
    }
}
