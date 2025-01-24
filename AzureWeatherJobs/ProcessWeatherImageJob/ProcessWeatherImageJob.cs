using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace WeatherImageJob
{
    public class ProcessWeatherImageJob
    {
        private readonly HttpClient _httpClient;

        public ProcessWeatherImageJob(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [FunctionName("ProcessWeatherImageJob")]
        public async Task Run(
        [QueueTrigger("%IMAGE_PROCESSING_QUEUE%", Connection = "AZURE_STORAGE_CONNECTION_STRING")] string queueMessage,

        {
            log.LogInformation("Processing weather image job.");

            // Parse job data
            var job = JsonSerializer.Deserialize<JobData>(jobData);
            var jobId = job?.JobId;

            // Fetch weather data from Buienradar API
            var response = await _httpClient.GetAsync("https://data.buienradar.nl/2.0/feed/json");
            var weatherDataJson = await response.Content.ReadAsStringAsync();
            var weatherData = JObject.Parse(weatherDataJson);

            // Parse weather data and queue tasks for each station
            var weatherStations = ParseWeatherStations(weatherData); 

            foreach (var station in weatherStations)
            {
                var taskData = new { jobId, station };
                await imageTasksQueue.AddAsync(JsonSerializer.Serialize(taskData));
            }

            log.LogInformation("Queued image generation tasks for all stations.");
        }

        private static List<(string Name, string Temperature)> ParseWeatherStations(JObject weatherData)
        {
            var stations = new List<(string Name, string Temperature)>();
            foreach (var station in weatherData["actual"]["stationmeasurements"])
            {
                stations.Add((station["stationname"].ToString(), station["temperature"].ToString()));
            }
            return stations;
        }
    }

    // Class to deserialize job data
    public class JobData
    {
        public string JobId { get; set; }
    }
}
