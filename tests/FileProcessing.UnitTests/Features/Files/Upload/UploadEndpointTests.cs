using FileProcessing.Api.Application.Services;
using FileProcessing.Api.Features.Files.Upload;
using FileProcessing.UnitTests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace FileProcessing.UnitTests.Features.Files.Upload;

public class UploadEndpointTests
{
    private static UploadEndpoint CreateEndpoint() =>
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
    public async Task Upload_NullFile_ReturnsBadRequest()
    {
        var endpoint = CreateEndpoint();

        var result = await endpoint.Upload(null!);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("File is required.", badRequest.Value);
    }

    [Fact]
    public async Task Upload_EmptyFile_ReturnsBadRequest()
    {
        var endpoint = CreateEndpoint();
        var emptyFile = new FormFile(new MemoryStream(), 0, 0, "file", "empty.txt");

        var result = await endpoint.Upload(emptyFile);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("File is required.", badRequest.Value);
    }

    [Fact]
    public async Task Upload_ValidFile_ReturnsOk()
    {
        var endpoint = CreateEndpoint();

        var result = await endpoint.Upload(CreateFile());

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.False(string.IsNullOrWhiteSpace(okResult.Value as string));
    }
}