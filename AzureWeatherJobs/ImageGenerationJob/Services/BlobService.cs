using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace ImageGenerationJob.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<BlobService> _logger;

        public BlobService(BlobServiceClient blobServiceClient, ILogger<BlobService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        public async Task UploadImageAsync(int jobId, int stationId, MemoryStream imageStream)
        {
            _logger.LogInformation($"Uploading image for JobId: {jobId}, StationId: {stationId}");
            var containerClient = _blobServiceClient.GetBlobContainerClient("weatherimagescontainer");
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient($"{jobId}/{stationId}.jpg");
            await blobClient.UploadAsync(imageStream, overwrite: true);
            _logger.LogInformation($"Image uploaded for JobId: {jobId}, StationId: {stationId}");
        }
    }
}