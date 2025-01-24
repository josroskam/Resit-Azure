using AzureWeatherJobs.ImageGenerationJob.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using WeatherImageGenerator.ImageGenerationJob.Entities;


namespace ImageGenerationJob.Services
{
    public class TableService
    {
        private readonly ILogger<TableService> _logger;
        private readonly TableClient _tableClient;

        public TableService(string connectionString, string tableName, ILogger<TableService> logger)
        {
            _logger = logger;
            _tableClient = new TableClient(connectionString, tableName);
        }

        public async Task UpdateJobStatusAsync(WeatherStation weatherStation)
        {
            _logger.LogInformation("Updating status for JobId: {JobId}", weatherStation.JobId);

            try
            {
                var jobStatusResponse = await _tableClient.GetEntityAsync<JobStatus>("WeatherJob", weatherStation.JobId);
                var jobStatus = jobStatusResponse.Value;

                jobStatus.ImagesCompleted++;

                if (jobStatus.ImagesCompleted == jobStatus.TotalImages)
                {
                    jobStatus.Status = "Completed";
                }

                await _tableClient.UpdateEntityAsync(jobStatus, jobStatus.ETag);

                _logger.LogInformation("Successfully updated job progress in Table Storage.");
                return;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error updating status for JobId: {weatherStation.JobId}");
                return;
            }
        }
    }
}
