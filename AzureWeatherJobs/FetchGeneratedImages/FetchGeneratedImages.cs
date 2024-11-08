using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace FetchGeneratedImages
{
    public class FetchGeneratedImages
    {
        private readonly ILogger _logger;

        public FetchGeneratedImages(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FetchGeneratedImages>();
        }

        [Function("FetchGeneratedImages")]
        public static async Task<IActionResult> Run(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "generated-images/{jobId}")] HttpRequest req,
         string jobId,
         ILogger log)
        {
            log.LogInformation($"Checking status of job {jobId}.");

            // Retrieve the connection string manually
            string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            // Create the CloudTableClient manually
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var jobStatusTable = tableClient.GetTableReference("JobStatus");

            // Retrieve job status from Table Storage
            var retrieveOperation = TableOperation.Retrieve<JobStatusEntity>("Job", jobId);
            var result = await jobStatusTable.ExecuteAsync(retrieveOperation);

            if (result.Result is JobStatusEntity jobStatus && jobStatus.IsComplete)
            {
                return new OkObjectResult(new { jobId, images = jobStatus.ImageUrls });
            }
            else if (result.Result == null)
            {
                return new NotFoundObjectResult(new { message = $"Job ID {jobId} not found." });
            }
            else
            {
                return new OkObjectResult(new { jobId, message = "Job is still in progress." });
            }
        }

    }
}
