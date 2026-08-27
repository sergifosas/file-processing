using Amazon.S3;
using Amazon.S3.Model;
using FileProcessing.Api.Application.Services;
using FileProcessing.Api.Domain.Metadata;

namespace FileProcessing.Api.Infrastructure.Storage;

public class S3Storage : IStorage
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;

    public S3Storage(
        IAmazonS3 s3,
        IConfiguration configuration)
    {
        _s3 = s3;

        _bucketName = configuration["AWS:S3:BucketName"]
            ?? throw new InvalidOperationException(
                "AWS:S3:BucketName no está configurado.");
    }

    public async Task<string> SaveAsync(
        Stream file,
        string key,
        string contentType)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = file,
            ContentType = contentType
        };

        await _s3.PutObjectAsync(request);

        return key;
    }

    public async Task<Stream> GetAsync(string key)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        var response = await _s3.GetObjectAsync(request);

        return response.ResponseStream;
    }

    public async Task<FileMetadata?> GetMetadataAsync(string key)
    {
        var request = new GetObjectMetadataRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        var response = await _s3.GetObjectMetadataAsync(request);

        return new FileMetadata
        {
            StoredName = key,
            ContentType = response.Headers.ContentType ?? string.Empty,
            Size = response.ContentLength,
            ETag = response.ETag ?? string.Empty,
            LastModified = response.LastModified ?? DateTime.MinValue,
            ContentEncoding = response.ContentEncoding,
            CacheControl = response.CacheControl,
            StorageClass = response.StorageClass,
            Metadata = response.Metadata.Count > 0
                ? response.Metadata.Keys.ToDictionary(
                    k => k,
                    k => response.Metadata[k])
                : null
        };
    }
}