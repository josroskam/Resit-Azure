using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using WeatherImageGenerator.ImageGenerationJob.Entities;

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

        public async Task UploadImageAsync(WeatherStation weatherStation, MemoryStream imageStream)
        {
            _logger.LogInformation($"Uploading image for JobId: {weatherStation.JobId}, StationId: {weatherStation.StationId}");
            var containerClient = _blobServiceClient.GetBlobContainerClient("weatherimagescontainer");
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient($"{weatherStation.JobId}/{weatherStation.StationId}.jpg");
            await blobClient.UploadAsync(imageStream, overwrite: true);
            _logger.LogInformation($"Image uploaded for JobId: {weatherStation.JobId}, StationId: {weatherStation.StationId}");
        }
    }
}