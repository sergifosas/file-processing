using FileProcessing.Api.Application.Services;
using FileProcessing.Api.Domain.Files;
using FileProcessing.Api.Features.Files.Metadata;
using FileProcessing.UnitTests.TestDoubles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace FileProcessing.UnitTests.Features.Files.Metadata;

public class MetadataEndpointTests
{
    private static MetadataEndpoint CreateEndpoint(
        FakeS3Storage? s3Storage = null,
        FakeFileRepository? repository = null)
    {
        return new MetadataEndpoint(
            new FileProcessingService(
                new FakeFileStorage(),
                s3Storage ?? new FakeS3Storage(),
                repository ?? new FakeFileRepository(),
                NullLogger<FileProcessingService>.Instance));
    }

    [Fact]
    public async Task GetMetadata_ExistingFile_ReturnsMetadata()
    {
        var s3Storage = new FakeS3Storage();
        var repository = new FakeFileRepository();
        var endpoint = CreateEndpoint(s3Storage, repository);

        const string storedName = "abc.txt";
        repository.Files.Add(new StoredFile
        {
            StoredName = storedName,
            OriginalName = "original.txt",
            ContentType = "text/plain",
            Size = 4,
            Path = storedName,
            CreatedAt = new DateTime(2026, 1, 1)
        });

        s3Storage.Objects[storedName] =
            System.Text.Encoding.UTF8.GetBytes("hola");
        s3Storage.ContentTypes[storedName] = "text/plain";

        var result = await endpoint.GetMetadata(storedName);

        var ok = Assert.IsType<OkObjectResult>(result);
        var metadata = Assert.IsType<MetadataResult>(ok.Value);

        Assert.Equal("original.txt", metadata.OriginalName);
        Assert.Equal(".txt", metadata.Extension);
        Assert.Equal("text/plain", metadata.ContentType);
        Assert.Equal(4, metadata.Size);
        Assert.Equal(storedName, s3Storage.LastRequestedKey);
    }

    [Fact]
    public async Task GetMetadata_MissingStoredFile_ReturnsNotFound()
    {
        var endpoint = CreateEndpoint();

        var result = await endpoint.GetMetadata("no-existe.txt");

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetMetadata_EmptyStoredName_ReturnsBadRequest()
    {
        var endpoint = CreateEndpoint();

        var result = await endpoint.GetMetadata(" ");

        Assert.IsType<BadRequestObjectResult>(result);
    }
}