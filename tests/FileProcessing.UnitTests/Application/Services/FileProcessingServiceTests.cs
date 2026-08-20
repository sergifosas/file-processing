using FileProcessing.Api.Application.Services;
using Microsoft.AspNetCore.Http;

namespace FileProcessing.UnitTests.Application.Services;

public class FileProcessingServiceTests
{
    private readonly FileProcessingService _service = new();

    [Fact]
    public async Task ProcessAsync_NullFile_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ProcessAsync(null!));
    }

    [Fact]
    public async Task ProcessAsync_EmptyFile_ThrowsArgumentException()
    {
        var emptyFile = new FormFile(new MemoryStream(), 0, 0, "file", "empty.txt");

        await Assert.ThrowsAsync<ArgumentException>(() => _service.ProcessAsync(emptyFile));
    }

    [Fact]
    public async Task ProcessAsync_ValidFile_StoresFileAndReturnsGeneratedName()
    {
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("contenido de prueba"));
        var file = new FormFile(stream, 0, stream.Length, "file", "test.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        var storedName = await _service.ProcessAsync(file);

        Assert.False(string.IsNullOrWhiteSpace(storedName));
        Assert.EndsWith(".txt", storedName);

        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", storedName);
        Assert.True(File.Exists(uploadsPath));

        File.Delete(uploadsPath);
    }
}