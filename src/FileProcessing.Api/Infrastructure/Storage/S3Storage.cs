using Amazon.S3;
using Amazon.S3.Model;
using FileProcessing.Api.Application.Services;

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
}