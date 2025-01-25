using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;
using StartWeatherImageJob.Services;
using System.Threading.Tasks;

namespace StartWeatherImageJob
{
    public class StartWeatherImageJob
    {
        private readonly JobService _jobService;
        private readonly ILogger<StartWeatherImageJob> _logger;

        // Use Dependency Injection to inject JobService
        public StartWeatherImageJob(JobService jobService, ILogger<StartWeatherImageJob> logger)
        {
            _jobService = jobService;
            _logger = logger;
        }

        [Function("StartWeatherImageJob")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            string jobId = await _jobService.CreateJobAsync();
            return new OkObjectResult(jobId);
        }
    }
}
