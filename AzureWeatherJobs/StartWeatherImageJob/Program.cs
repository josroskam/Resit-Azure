using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using StartWeatherImageJob.Services;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureServices(services =>
    {
        services.AddLogging();  // Registers default logging services
        services.AddSingleton<JobService>();  // Register JobService for DI
    })
    .Build();

host.Run();
