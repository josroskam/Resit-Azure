using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using StartWeatherImageJob.Services;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        // Register JobService with its dependencies
        services.AddSingleton<JobService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<JobService>>();
            var tableConnectionString = Environment.GetEnvironmentVariable("TableConnectionString");
            var queueConnectionString = Environment.GetEnvironmentVariable("QueueConnectionString");
            var queueName = Environment.GetEnvironmentVariable("QueueName") ?? "weather-image-jobs";
            var tableName = Environment.GetEnvironmentVariable("TableName") ?? "WeatherJobs";

            if (string.IsNullOrEmpty(tableConnectionString) || string.IsNullOrEmpty(queueConnectionString))
            {
                throw new InvalidOperationException("Required environment variables are not set.");
            }

            return new JobService(
                tableConnectionString,
                tableName,
                queueConnectionString,
                queueName,
                logger
            );
        });
    })
    .Build();

host.Run();
