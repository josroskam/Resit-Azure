using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Concurrent;
using System;
using Azure;

namespace AzureWeatherJobs.FetchGeneratedImages.Entities
{
    public class JobStatus : TableEntity
    {
        public string PartitionKey { get; set; } = "WeatherJob";
        public string RowKey { get; set; }
        public string Status { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public int TotalImages { get; set; }
        public int ImagesCompleted { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}