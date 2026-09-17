using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EventEase.Services
{
    /// <summary>
    /// Service for managing Azure Blob Storage operations for images.
    /// Handles uploading, deleting, and retrieving image URLs from Azure Blob Storage.
    /// </summary>
    public class AzureStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<AzureStorageService> _logger;

        public AzureStorageService(BlobContainerClient containerClient, ILogger<AzureStorageService> logger)
        {
            _containerClient = containerClient;
            _logger = logger;
        }

        /// <summary>
        /// Uploads an image file to Azure Blob Storage.
        /// </summary>
        /// <param name="file">The image file to upload</param>
        /// <param name="categoryName">Folder name in blob storage (e.g., 'events', 'venues')</param>
        /// <returns>The URI of the uploaded blob, or empty string if upload failed</returns>
        public async Task<string> UploadImageAsync(IFormFile file, string categoryName)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return string.Empty;

                // Generate unique filename
                string fileName = $"{categoryName}/{Guid.NewGuid()}_{file.FileName}";

                // Get blob client
                BlobClient blobClient = _containerClient.GetBlobClient(fileName);

                // Upload file
                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                _logger.LogInformation($"Image uploaded successfully: {fileName}");
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading image to Azure Storage: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Deletes an image from Azure Blob Storage.
        /// </summary>
        /// <param name="blobUri">The URI of the blob to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        public async Task<bool> DeleteImageAsync(string blobUri)
        {
            try
            {
                if (string.IsNullOrEmpty(blobUri))
                    return false;

                // Extract blob name from URI
                var uri = new Uri(blobUri);
                string blobName = uri.AbsolutePath.Trim('/');
                if (blobName.StartsWith(_containerClient.Name + "/"))
                {
                    blobName = blobName.Substring(_containerClient.Name.Length + 1);
                }

                // Get blob client and delete
                BlobClient blobClient = _containerClient.GetBlobClient(blobName);
                await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);

                _logger.LogInformation($"Image deleted successfully: {blobName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting image from Azure Storage: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a blob exists in Azure Blob Storage.
        /// </summary>
        public async Task<bool> BlobExistsAsync(string blobUri)
        {
            try
            {
                if (string.IsNullOrEmpty(blobUri))
                    return false;

                var uri = new Uri(blobUri);
                string blobName = uri.AbsolutePath.Trim('/');
                if (blobName.StartsWith(_containerClient.Name + "/"))
                {
                    blobName = blobName.Substring(_containerClient.Name.Length + 1);
                }

                BlobClient blobClient = _containerClient.GetBlobClient(blobName);
                return await blobClient.ExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking blob existence: {ex.Message}");
                return false;
            }
        }
    }
}
