using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Add BlobServiceClient using connection string
        string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        if (string.IsNullOrEmpty(storageConnectionString))
        {
            throw new InvalidOperationException("AzureWebJobsStorage environment variable is not set.");
        }

        services.AddSingleton(new BlobServiceClient(storageConnectionString));

        // Add logging
        services.AddLogging();
    })
    .Build();

host.Run();
