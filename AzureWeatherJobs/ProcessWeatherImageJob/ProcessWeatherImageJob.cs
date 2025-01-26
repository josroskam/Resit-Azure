using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using ProcessWeatherImageJob.Entities;
using ProcessWeatherImageJob.Services;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WeatherImageJob
{
    public class ProcessWeatherImageJob
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProcessWeatherImageJob> _logger;
        private readonly string _tableConnectionString = Environment.GetEnvironmentVariable("TableConnectionString");
        private readonly string _queueConnectionString = Environment.GetEnvironmentVariable("QueueConnectionString");
        private const string TableName = "WeatherImageGeneratorJobs";
        private const string GenerateImageQueueName = "generateimagequeue";
        private readonly WeatherStationService _weatherStationService;

        public ProcessWeatherImageJob(HttpClient httpClient, ILogger<ProcessWeatherImageJob> logger, WeatherStationService weatherStationService)
        {
            _httpClient = httpClient;
            _logger = logger ?? NullLogger<ProcessWeatherImageJob>.Instance;
            _weatherStationService = weatherStationService;
        }


        [Function("ProcessWeatherImageJob")]
        public async Task Run(
        [QueueTrigger("%IMAGE_PROCESSING_QUEUE%", Connection = "AZURE_STORAGE_CONNECTION_STRING")] string queueMessage)
        {
            string jobId = queueMessage;
            _logger.LogInformation("Processing weather image job with id: " + jobId);

            var tableClient = new TableClient(_tableConnectionString, TableName);

            // get the job status from Table Storage
            var jobStatus = await tableClient.GetEntityAsync<JobStatus>("WeatherJobs", jobId);
            jobStatus.Value.Status = "Stations retrieved";

            // get the weather stations from the Buienradar API
            var weatherStations = await _weatherStationService.GetWeatherStations();

            foreach (var station in weatherStations)
            {
                station.JobId = jobId;
            }

            // add the weather stations to the queue
            var queueClient = new QueueClient(_queueConnectionString, GenerateImageQueueName);
            jobStatus.Value.TotalImages = weatherStations.Count();
            await queueClient.CreateIfNotExistsAsync();

            foreach (var station in weatherStations)
            {
                var message = JsonSerializer.Serialize(station);
                message = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));
                await queueClient.SendMessageAsync(message);
            }

            // update the job entry in Table Storage
            jobStatus.Value.TotalImages = weatherStations.Count;
            jobStatus.Value.Status = "Images generated";
            await tableClient.UpdateEntityAsync(jobStatus.Value, jobStatus.Value.ETag);
        }
    }
}
