using FileProcessing.Api.Application.Services;
using FileProcessing.Api.Features.Files.Rename;
using FileProcessing.UnitTests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace FileProcessing.UnitTests.Features.Files.Rename;

public class RenameEndpointTests
{
    private static RenameEndpoint CreateEndpoint() =>
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
    public async Task Rename_NullFile_ReturnsBadRequest()
    {
        var endpoint = CreateEndpoint();

        var result = await endpoint.Rename(null!, "renombrado.txt");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("File is required.", badRequest.Value);
    }

    [Fact]
    public async Task Rename_EmptyFile_ReturnsBadRequest()
    {
        var endpoint = CreateEndpoint();
        var emptyFile = new FormFile(new MemoryStream(), 0, 0, "file", "empty.txt");

        var result = await endpoint.Rename(emptyFile, "renombrado.txt");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("File is required.", badRequest.Value);
    }

    [Fact]
    public async Task Rename_ValidFile_ReturnsFileStreamWithNewNameAndPersistsMetadata()
    {
        var storage = new FakeFileStorage();
        var repository = new FakeFileRepository();
        var endpoint = new RenameEndpoint(
            new FileProcessingService(
                storage,
                new FakeS3Storage(),
                repository,
                NullLogger<FileProcessingService>.Instance));
        var file = CreateFile();

        var result = await endpoint.Rename(file, "renombrado.txt");

        var fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("text/plain", fileResult.ContentType);
        Assert.Equal("renombrado.txt", fileResult.FileDownloadName);

        using var reader = new StreamReader(fileResult.FileStream);
        Assert.Equal("contenido de prueba", await reader.ReadToEndAsync());

        var storedFile = Assert.Single(repository.Files);
        Assert.Equal("test.txt", storedFile.OriginalName);
        Assert.Equal("text/plain", storedFile.ContentType);
        Assert.Equal(file.Length, storedFile.Size);
        Assert.Equal(storage.SavedFileName, storedFile.StoredName);
        Assert.EndsWith(".txt", storage.SavedFileName!);
    }
}