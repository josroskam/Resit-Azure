using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace StartFunction
{
    public static class StartWeatherImageJob
    {
        [FunctionName("StartWeatherImageJob")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "start-job")] HttpRequest req,
            [Queue("weather-image-jobs", Connection = "AzureWebJobsStorage")] IAsyncCollector<string> jobQueue,
            ILogger log)
        {
            log.LogInformation("HTTP trigger to start weather image job processed a request.");

            // Generate a unique job ID
            string jobId = Guid.NewGuid().ToString();

            // Enqueue the job ID along with any necessary data 
            await jobQueue.AddAsync(jobId);

            // Respond with the job ID so the client can track it
            return new OkObjectResult(new { jobId, message = "Job started successfully. Use the job ID to track status." });
        }
    }
}
