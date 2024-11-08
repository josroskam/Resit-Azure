using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Concurrent;
using System;

public class JobStatusEntity : TableEntity
{
    public JobStatusEntity(string jobId)
    {
        PartitionKey = "Job";
        RowKey = jobId;
    }

    public JobStatusEntity() { }

    public bool IsComplete { get; set; }
    public string[] ImageUrls { get; set; }  // URLs to generated images

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string StatusMessage { get; set; } = "In progress";
}
