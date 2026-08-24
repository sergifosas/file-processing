using Amazon.S3;
using Amazon.S3.Model;
using FileProcessing.Api.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FileProcessing.UnitTests.Infrastructure.Storage;

public class S3StorageTests
{
    private readonly Mock<IAmazonS3> _s3 = new();

    private static Mock<IConfiguration> CreateConfiguration(string? bucketName)
    {
        var configuration = new Mock<IConfiguration>();

        configuration
            .Setup(c => c["AWS:S3:BucketName"])
            .Returns(bucketName);

        return configuration;
    }

    [Fact]
    public void Constructor_WithMissingBucketName_ThrowsInvalidOperationException()
    {
        var configuration = CreateConfiguration(null);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new S3Storage(_s3.Object, configuration.Object));

        Assert.Equal(
            "AWS:S3:BucketName no está configurado.",
            exception.Message);
    }

    [Fact]
    public async Task SaveAsync_SendsObjectWithExpectedRequestAndReturnsKey()
    {
        var storage = new S3Storage(
            _s3.Object,
            CreateConfiguration("my-bucket").Object);

        using var stream = new MemoryStream(
            System.Text.Encoding.UTF8.GetBytes("contenido de prueba"));

        _s3
            .Setup(x => x.PutObjectAsync(
                It.IsAny<PutObjectRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse());

        var key = await storage.SaveAsync(stream, "abc.txt", "text/plain");

        Assert.Equal("abc.txt", key);
        _s3.Verify(x => x.PutObjectAsync(
                It.Is<PutObjectRequest>(r =>
                    r.BucketName == "my-bucket" &&
                    r.Key == "abc.txt" &&
                    r.ContentType == "text/plain" &&
                    ReferenceEquals(r.InputStream, stream)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_SendsRequestForExpectedKeyAndReturnsResponseStream()
    {
        var storage = new S3Storage(
            _s3.Object,
            CreateConfiguration("my-bucket").Object);

        using var stream = new MemoryStream(
            System.Text.Encoding.UTF8.GetBytes("contenido de prueba"));

        _s3
            .Setup(x => x.GetObjectAsync(
                It.IsAny<GetObjectRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetObjectResponse { ResponseStream = stream });

        var result = await storage.GetAsync("abc.txt");

        Assert.Same(stream, result);
        _s3.Verify(x => x.GetObjectAsync(
                It.Is<GetObjectRequest>(r =>
                    r.BucketName == "my-bucket" &&
                    r.Key == "abc.txt"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}