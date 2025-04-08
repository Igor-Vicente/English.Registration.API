using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Languages.Registration.API.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadAsync(IFormFile file, string blobName, string contentType);
        Task<bool> DeleteAsync(string fileUrl);
    }
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;
        public BlobStorageService(BlobContainerClient containerClient)
        {
            _containerClient = containerClient;
        }

        public async Task<string> UploadAsync(IFormFile file, string blobName, string contentType)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
            }

            return blobClient.Uri.AbsoluteUri;
        }

        public async Task<bool> DeleteAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return true;

            var blobName = GetBlobNameFromUrl(fileUrl);

            var blobClient = _containerClient.GetBlobClient(blobName);

            return await blobClient.DeleteIfExistsAsync();
        }

        private string GetBlobNameFromUrl(string fileUrl)
        {
            var uri = new Uri(fileUrl);
            var fileName = Path.GetFileName(uri.AbsolutePath);
            return fileName;
        }
    }
}
