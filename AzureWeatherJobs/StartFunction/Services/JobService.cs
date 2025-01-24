using Azure.Data.Tables;
using Azure.Storage.Queues;
using Grpc.Core.Logging;
using Microsoft.Extensions.Logging;
using StartWeatherImageJob.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartWeatherImageJob.Services
{
    public class JobService
    {
        private readonly ILogger<JobService> _logger;
        private readonly TableClient _tableClient;
        private readonly QueueClient _queueClient;
        private string queueName;

        public JobService(string tableConnectionString, string tableName, string queueConnectionString, string queueName, ILogger<JobService> logger)
        {
            _logger = logger;
            _tableClient = new TableClient(tableConnectionString, "WeatherJobs");
            _queueClient = new QueueClient(queueConnectionString, "weather-image-jobs");
            this.queueName = queueName;
        }

        // Add a method to create a new job
        public async Task<string> CreateJobAsync()
        {
            _logger.LogInformation("Creating new job. Sending message to queue and adding entity to table.");
            string jobId = Guid.NewGuid().ToString();

            // create queue if not exists
            await _queueClient.CreateIfNotExistsAsync();

            // create message
            await _queueClient.SendMessageAsync(jobId);
            _logger.LogInformation("Message sent to queue: " + queueName + " with message: " + jobId);

            // create table if not exists
            await _tableClient.CreateIfNotExistsAsync();

            var jobStatus = new JobStatus { RowKey = jobId, StartTime = DateTimeOffset.UtcNow, Status = "Processing" };
            _logger.LogInformation("Adding entity to table: " + jobStatus.RowKey);

            await _tableClient.AddEntityAsync(jobStatus);
            return jobId;


        }

    }
}
