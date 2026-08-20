using FileProcessing.Api.Application.Services;
using Microsoft.AspNetCore.Http;

namespace FileProcessing.UnitTests.Application.Services;

public class FileProcessingServiceTests
{
    private readonly FileProcessingService _service = new();

    [Fact]
    public async Task ProcessAsync_NullFile_Throws()
    {
        await Assert.ThrowsAsync<NullReferenceException>(() => _service.ProcessAsync(null!));
    }

    [Fact]
    public async Task ProcessAsync_ValidFile_Completes()
    {
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("contenido de prueba"));
        var file = new FormFile(stream, 0, stream.Length, "file", "test.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        // Se espera que el servicio procese el archivo sin lanzar excepciones.
        var exception = await Record.ExceptionAsync(() => _service.ProcessAsync(file));

        Assert.Null(exception);
    }
}