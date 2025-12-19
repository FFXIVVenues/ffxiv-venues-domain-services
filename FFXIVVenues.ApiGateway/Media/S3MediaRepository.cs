using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using FFXIVVenues.ApiGateway.Helpers;
using FFXIVVenues.DomainData.Helpers;
using Microsoft.Extensions.Configuration;

namespace FFXIVVenues.ApiGateway.Media;

public class S3MediaRepository : IMediaRepository
{
    private readonly IConfiguration _config;
    private readonly IAmazonS3 _s3Client;

    public bool IsMetered => true;

    public S3MediaRepository(IConfiguration config)
    {
        _config = config;
        _s3Client = CreateS3Client();
    }

    public async Task<(Stream Stream, string ContentType)> Download(string venueId, string key, CancellationToken cancellationToken)
    {
        var bucketName = _config.GetValue<string>("MediaStorage:S3:BucketName");
        if (string.IsNullOrEmpty(bucketName))
            throw new Exception("No bucket name configured for media storage.");

        var path = $"{venueId}/{key}";
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = path
        };

        var response = await _s3Client.GetObjectAsync(request, cancellationToken);
        return (response.ResponseStream, response.Headers.ContentType);
    }

    public async Task<string> Upload(string venueId, string contentType, long contentLength, Stream stream, CancellationToken cancellationToken)
    {
        var bucketName = _config.GetValue<string>("MediaStorage:S3:BucketName");
        if (string.IsNullOrEmpty(bucketName))
            throw new Exception("No bucket name configured for media storage.");

        var key = IdHelper.GenerateId();
        var path = $"{venueId}/{key}";
        var request = new PutObjectRequest
        {
            
            BucketName = bucketName,
            Key = path,
            InputStream = stream,
            ContentType = contentType,
            Headers = { ContentLength = contentLength },
            
            // Cloudflare R2 does not support Streaming SigV4
            DisablePayloadSigning = true,
            DisableDefaultChecksumValidation = true
        };
        
        await _s3Client.PutObjectAsync(request, cancellationToken);
        return key;
    }

    public async Task Delete(string venueId, string key)
    {
        var bucketName = _config.GetValue<string>("MediaStorage:S3:BucketName");
        if (string.IsNullOrEmpty(bucketName))
            throw new Exception("No bucket name configured for media storage.");

        var path = $"{venueId}/{key}";
        var request = new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = path
        };

        await _s3Client.DeleteObjectAsync(request);
    }

    private IAmazonS3 CreateS3Client()
    {
        var endpoint = _config.GetValue<string>("MediaStorage:S3:Endpoint");
        var accessKey = _config.GetValue<string>("MediaStorage:S3:AccessKey");
        var secretKey = _config.GetValue<string>("MediaStorage:S3:SecretKey");

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
            throw new Exception("S3 endpoint, access key, and secret key must be configured for media storage.");

        var config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true // Required for Cloudflare R2 and some S3-compatible services
        };

        return new AmazonS3Client(accessKey, secretKey, config);
    }
}