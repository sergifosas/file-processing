using FileProcessing.Api.Application.Services;
using FileProcessing.Api.Domain.Files;
using FileProcessing.Api.Features.Files.Download;
using FileProcessing.UnitTests.TestDoubles;
using Microsoft.AspNetCore.Mvc;

namespace FileProcessing.UnitTests.Features.Files.Download;

public class DownloadS3EndpointTests
{
    [Fact]
    public async Task Download_ExistingFile_ReturnsFileStreamResult()
    {
        var repository = new FakeFileRepository();
        var s3Storage = new FakeS3Storage();
        var endpoint = new DownloadS3Endpoint(
            new FileProcessingService(
                new FakeFileStorage(),
                s3Storage,
                repository));

        const string storedName = "abc.txt";
        repository.Files.Add(new StoredFile
        {
            StoredName = storedName,
            OriginalName = "original.txt",
            ContentType = "text/plain",
            Size = 5,
            Path = storedName,
            CreatedAt = DateTime.UtcNow
        });

        s3Storage.Objects[storedName] =
            System.Text.Encoding.UTF8.GetBytes("hola");

        var result = await endpoint.Download(storedName);

        var fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("text/plain", fileResult.ContentType);
        Assert.Equal("original.txt", fileResult.FileDownloadName);

        using var reader = new StreamReader(fileResult.FileStream);
        Assert.Equal("hola", await reader.ReadToEndAsync());
        Assert.Equal(storedName, s3Storage.LastRequestedKey);
    }

    [Fact]
    public async Task Download_MissingFile_ReturnsNotFound()
    {
        var endpoint = new DownloadS3Endpoint(
            new FileProcessingService(
                new FakeFileStorage(),
                new FakeS3Storage(),
                new FakeFileRepository()));

        var result = await endpoint.Download("no-existe.txt");

        Assert.IsType<NotFoundObjectResult>(result);
    }
}