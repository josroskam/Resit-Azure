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
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ImageGenerationJob
{
    public class ImageGenerationJob
    {
        private readonly HttpClient _httpClient;
        private readonly string _blobConnectionString;

        public ImageGenerationJob(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _blobConnectionString = Environment.GetEnvironmentVariable("BlobConnectionString"); // Blob Storage connection string
        }

        [FunctionName("GenerateWeatherImage")]
        public async Task Run(
            [QueueTrigger("weather-image-tasks", Connection = "AzureWebJobsStorage")] string taskData,
            [Blob("weather-images/{rand-guid}.jpg", FileAccess.Write, Connection = "AzureWebJobsStorage")] CloudBlockBlob blob,
            ILogger log)
        {
            log.LogInformation($"Processing image generation task: {taskData}");

            // Deserialize the task data
            var task = JsonSerializer.Deserialize<dynamic>(taskData);
            string jobId = task.jobId;
            var station = task.station;

            // Download the background image (e.g., from Unsplash API)
            var imageUrl = "https://source.unsplash.com/800x600/?nature,weather";  // Replace with actual API call
            var imageStream = await _httpClient.GetStreamAsync(imageUrl);

            // Create an image from the background
            var backgroundImage = Image.FromStream(imageStream);
            var graphics = Graphics.FromImage(backgroundImage);

            // Draw the weather data on the image
            var font = new Font("Arial", 16);
            var brush = new SolidBrush(Color.White);

            graphics.DrawString($"Station: {station.Name}", font, brush, 10, 10);
            graphics.DrawString($"Temperature: {station.Temperature}°C", font, brush, 10, 30);

            // Save the image to a memory stream
            var imageMemoryStream = new MemoryStream();
            backgroundImage.Save(imageMemoryStream, ImageFormat.Jpeg);
            imageMemoryStream.Position = 0;

            // Upload the image to Blob Storage
            await blob.UploadFromStreamAsync(imageMemoryStream);

            log.LogInformation($"Generated image for station {station.Name}.");

        }
    }
}
