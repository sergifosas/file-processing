using FileProcessing.Api.Application.Services;
using FileProcessing.Api.Features.Storing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileProcessing.UnitTests.Features.Storing;

public class FileStoringControllerTests
{
    private static FileStoringController CreateController() => new(new FileProcessingService());

    private static FormFile CreateFile(string content = "contenido de prueba")
    {
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        return new FormFile(stream, 0, stream.Length, "file", "test.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };
    }

    [Fact]
    public async Task Process_NullFile_ReturnsBadRequest()
    {
        var controller = CreateController();

        var result = await controller.Process(null!);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("File is required.", badRequest.Value);
    }

    [Fact]
    public async Task Process_EmptyFile_ReturnsBadRequest()
    {
        var controller = CreateController();
        var emptyFile = new FormFile(new MemoryStream(), 0, 0, "file", "empty.txt");

        var result = await controller.Process(emptyFile);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("File is required.", badRequest.Value);
    }

    [Fact]
    public async Task Process_ValidFile_ReturnsOk()
    {
        var controller = CreateController();

        var result = await controller.Process(CreateFile());

        Assert.IsType<OkResult>(result);
    }
}