using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using System.Net;

namespace FetchGeneratedImages
{
    public class FetchGeneratedImages
    {
        private readonly ILogger<FetchGeneratedImages> _logger;
        private readonly BlobServiceClient _blobServiceClient;

        public FetchGeneratedImages(ILogger<FetchGeneratedImages> logger, BlobServiceClient blobServiceClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
        }

        [Function("FetchImages")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "generated-images/{jobId}")] HttpRequestData req,
            string jobId)
        {
            _logger.LogInformation($"Fetching results for JobId: {jobId}");

            // attempt to retrieve the job entity from Table Storage
            try
            {
                // generate list of URLs for the generated images
                var containerClient = _blobServiceClient.GetBlobContainerClient("weatherimagescontainer");
                var urls = new List<string>();

                // first get all images from the folder with the job id
                var blobItems = containerClient.GetBlobs(prefix: jobId);

                // generate a SAS token for each image
                foreach (var blobItem in blobItems)
                {
                    // generate a SAS token for each image
                    var blobClient = containerClient.GetBlobClient(blobItem.Name);
                    var sasBuilder = new BlobSasBuilder
                    {
                        BlobContainerName = blobClient.BlobContainerName,
                        BlobName = blobClient.Name,
                        Resource = "b",
                        StartsOn = DateTimeOffset.UtcNow,
                        ExpiresOn = DateTimeOffset.UtcNow.AddHours(4)
                    };

                    sasBuilder.SetPermissions(BlobSasPermissions.Read);

                    var sasToken = sasBuilder.ToSasQueryParameters(
                        new StorageSharedKeyCredential(
                            Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_NAME"),
                            Environment.GetEnvironmentVariable("WEB_JOBS_STORAGE_KEY"))
                        );

                    urls.Add($"{blobClient.Uri}?{sasToken}");
                }

                var numberOfImages = urls.Count;

                // create the response with the list of URLs
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new { jobId, numberOfImages, urls });
                return response;
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {
                _logger.LogError($"JobId: {jobId} not found");
                var response = req.CreateResponse(HttpStatusCode.NotFound);
                await response.WriteAsJsonAsync(new { Error = "Job not found" });
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error fetching images for JobId: {jobId}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(new { Error = e.Message });
                return response;
            }
        }
    }
}
