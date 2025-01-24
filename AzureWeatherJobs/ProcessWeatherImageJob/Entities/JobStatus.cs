﻿using Azure.Data.Tables;
using Azure;

namespace ProcessWeatherImageJob.Entities
{
    public class JobStatus : ITableEntity
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