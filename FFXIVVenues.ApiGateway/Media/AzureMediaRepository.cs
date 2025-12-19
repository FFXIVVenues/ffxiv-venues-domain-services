using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FFXIVVenues.ApiGateway.Helpers;
using FFXIVVenues.DomainData.Helpers;
using Microsoft.Extensions.Configuration;
using Uri = System.Uri;

namespace FFXIVVenues.ApiGateway.Media;

public class AzureMediaRepository : IMediaRepository
{
    private readonly IConfiguration _config;

    public bool IsMetered => true;
    
    public AzureMediaRepository(IConfiguration config) =>
        _config = config;

    public async Task<(Stream Stream, string ContentType)> Download(string venueId, string key, CancellationToken cancellationToken)
    {
        var container = GetBlobContainerClient();
        var blob = container.GetBlobClient(key);
        var response = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
            
        return (response.Value.Content, response.Value.Details.ContentType);
    }

    public async Task<string> Upload(string venueId, string contentType, long contentLength, Stream stream, CancellationToken cancellationToken)
    {
        var key = IdHelper.GenerateId();
        var container = GetBlobContainerClient();
        var blob = container.GetBlobClient(key);
        var result = await blob.UploadAsync(stream,
            httpHeaders: new BlobHttpHeaders { ContentType = contentType },
            transferOptions: new StorageTransferOptions { MaximumTransferSize = 10_048_576 },
            cancellationToken: cancellationToken);
        return key;
    }

    public async Task Delete(string venueId, string key)
    {
        var container = GetBlobContainerClient();
        var blob = container.GetBlobClient(key);
        _ = await blob.DeleteIfExistsAsync();
    }

    private BlobContainerClient GetBlobContainerClient()
    {
        var connectionString = _config.GetValue<string>("MediaStorage:Azure:ConnectionString");
        if (connectionString != null)
        {
            var containerName = _config.GetValue<string>("MediaStorage:Azure:ContainerName");
            if (containerName == null)
                throw new Exception("No connection string configured for media storage.");

            return new BlobContainerClient(connectionString, containerName);
        }
            
        var containerUri = _config.GetValue<string>("MediaStorage:Azure:ContainerUri");
        if (containerUri != null)
            return new BlobContainerClient(new Uri(containerUri), new DefaultAzureCredential());

        throw new Exception("No connection string or container uri configured for media storage.");
    }

}