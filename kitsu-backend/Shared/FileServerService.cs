using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace kitsu_backend.Shared
{
    public static class FileServerService
    {
        static string ConnectionString = string.Empty;

        public static void Initialize(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("AzureStorage");
        }
        /*Uploads images in frontend project folder (local only). For prod, images should be uploaded to Azure Storage*/
        public static async Task PostAnimeImage(string fileName, IFormFile image, string mediaPath)
        {
            var containerClient = new BlobContainerClient(ConnectionString, "images-test");

            //We can save image with file extension or without, but file URL in frontend needs to match chosen pattern
            var blobClient = containerClient.GetBlobClient($"{mediaPath}/{fileName}.jpg");

            var headers = new BlobHttpHeaders
            {
                ContentType = image.ContentType
            };

            using var stream = image.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = headers
            });
        }

        public static async Task DeleteImage(string fileName, string mediaPath)
        {
            var containerClient = new BlobContainerClient(ConnectionString, "images-test");

            var blobClient = containerClient.GetBlobClient($"{mediaPath}/{fileName}.jpg");

            var result = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.None);
        }
    }
}
