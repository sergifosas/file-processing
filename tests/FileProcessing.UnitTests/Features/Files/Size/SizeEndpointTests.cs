using FileProcessing.Api.Application.Services;
using FileProcessing.Api.Features.Files.Size;
using FileProcessing.UnitTests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace FileProcessing.UnitTests.Features.Files.Size;

public class SizeEndpointTests
{
    private static SizeEndpoint CreateEndpoint() =>
        new(new FileProcessingService(
            new FakeFileStorage(),
            new FakeS3Storage(),
            new FakeFileRepository(),
            NullLogger<FileProcessingService>.Instance));

    private static FormFile CreateFile(string content = "contenido de prueba")
    {
        var stream = new MemoryStream(
            System.Text.Encoding.UTF8.GetBytes(content));

        return new FormFile(stream, 0, stream.Length, "file", "test.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };
    }

    [Fact]
    public async Task GetSize_NullFile_ReturnsBadRequest()
    {
        var endpoint = CreateEndpoint();

        var result = await endpoint.GetSize(null!);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("File is required.", badRequest.Value);
    }

    [Fact]
    public async Task GetSize_EmptyFile_ReturnsBadRequest()
    {
        var endpoint = CreateEndpoint();
        var emptyFile = new FormFile(new MemoryStream(), 0, 0, "file", "empty.txt");

        var result = await endpoint.GetSize(emptyFile);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("File is required.", badRequest.Value);
    }

    [Fact]
    public async Task GetSize_ValidFile_ReturnsOkWithStoredNameAndCalculatedSizes()
    {
        var storage = new FakeFileStorage();
        var repository = new FakeFileRepository();
        var endpoint = new SizeEndpoint(
            new FileProcessingService(
                storage,
                new FakeS3Storage(),
                repository,
                NullLogger<FileProcessingService>.Instance));
        var file = CreateFile();

        var result = await endpoint.GetSize(file);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var payload = okResult.Value!;
        var storedName = GetPropertyValue(payload, "StoredName") as string;
        var sizeInBytes = (GetPropertyValue(payload, "FileSizeInBytes") as long?)!;
        var sizeInMb = (GetPropertyValue(payload, "FileSizeInMb") as double?)!;

        Assert.Equal(file.Length, sizeInBytes);
        Assert.Equal(
            Math.Round(file.Length / (1024.0 * 1024.0), 2),
            sizeInMb);
        Assert.Equal(storage.SavedFileName, storedName);
        Assert.EndsWith(".txt", storedName!);
    }

    private static object? GetPropertyValue(object obj, string propertyName)
    {
        foreach (var prop in obj.GetType().GetProperties())
        {
            if (prop.Name == propertyName)
                return prop.GetValue(obj);
        }

        return null;
    }
}