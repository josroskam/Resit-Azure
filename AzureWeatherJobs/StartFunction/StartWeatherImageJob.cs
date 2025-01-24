using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using StartWeatherImageJob.Services;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace StartFunction
{
    public class StartWeatherImageJob
    {
        private readonly JobService _jobService;
        private readonly ILogger<JobService> _logger;

        public StartWeatherImageJob()
        {
            var tableConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var queueConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var tableName = "StartWeatherJobQueue";
            var queueName = "WeatherImageJobs";
            _jobService = new JobService(tableConnectionString, tableName, queueConnectionString, queueName, _logger);
        }

        [Function("StartWeatherImageJob")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)

        {
            string jobId = await _jobService.CreateJobAsync();
            return new OkObjectResult(jobId);
        }
    }
}
