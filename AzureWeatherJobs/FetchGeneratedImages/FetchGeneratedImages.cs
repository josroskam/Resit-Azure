using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace FetchGeneratedImages
{
    public class FetchGeneratedImages
    {
        private readonly ILogger<FetchGeneratedImages> _logger;
        private readonly BlobServiceClient _blobServiceClient;

        public FetchGeneratedImages(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FetchGeneratedImages>();
            _blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
        }
    }
}
