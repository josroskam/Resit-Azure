using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using WeatherImageGenerator.ImageGenerationJob.Entities;
using ImageGenerationJob.Services;
using Azure.Storage.Blobs;

namespace ImageGenerationJob
{
    public class ImageGenerationJob
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ImageGenerationJob> _logger;
        private readonly ImageGenerationService _imageGenerationService;
        private readonly BlobService _blobService;
        private readonly TableService _tableService;

        public ImageGenerationJob(ILogger<ImageGenerationJob> logger, ILogger<ImageGenerationService> imageServiceLogger, ILogger<BlobService> blobServiceLogger, ILogger<TableService> tableServiceLogger)
        {
            _httpClient = new HttpClient();
            _logger = logger;

            _imageGenerationService = new ImageGenerationService(_httpClient, imageServiceLogger);

            var blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            _blobService = new BlobService(blobServiceClient, blobServiceLogger);

            var tableConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var tableName = "WeatherImageGeneratorJobs";
            _tableService = new TableService(tableConnectionString, tableName, tableServiceLogger);
        }


       [Function("GenerateWeatherImage")]
        public async Task Run(
        [QueueTrigger("%JOB_START_QUEUE%", Connection = "AZURE_STORAGE_CONNECTION_STRING")] WeatherStation weatherStation)
        {
            _logger.LogInformation($"Processing image generation for JobId: {weatherStation.JobId}, StationId: {weatherStation.StationId}");
            var blobStream = await _imageGenerationService.GenerateImageAsync(weatherStation);

            if (blobStream != null)
            {
                await _blobService.UploadImageAsync(weatherStation, blobStream);
                await _tableService.UpdateJobStatusAsync(weatherStation);
            }
            else
            {
                _logger.LogError($"Failed to generate image for JobId: {weatherStation.JobId}, StationId: {weatherStation.StationId}");
            }

        }
    }
}
