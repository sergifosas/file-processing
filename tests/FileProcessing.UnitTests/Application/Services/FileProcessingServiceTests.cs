using FileProcessing.Api.Application.Services;
using FileProcessing.UnitTests.TestDoubles;

namespace FileProcessing.UnitTests.Application.Services;

public class FileProcessingServiceTests
{
    private readonly FakeFileStorage _storage;
    private readonly FakeS3Storage _s3Storage;
    private readonly FakeFileRepository _repository;
    private readonly FileProcessingService _service;

    public FileProcessingServiceTests()
    {
        _storage = new FakeFileStorage();
        _s3Storage = new FakeS3Storage();
        _repository = new FakeFileRepository();
        _service = new FileProcessingService(
            _storage,
            _s3Storage,
            _repository);
    }

    [Fact]
    public async Task ProcessAsync_NullStream_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.ProcessAsync(null!, "test.txt", "text/plain", 10));
    }

    [Fact]
    public async Task ProcessAsync_EmptyFile_ThrowsArgumentException()
    {
        using var stream = new MemoryStream();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ProcessAsync(stream, "empty.txt", "text/plain", 0));
    }

    [Fact]
    public async Task ProcessAsync_ValidFile_StoresMetadataAndReturnsGeneratedName()
    {
        using var stream = new MemoryStream(
            System.Text.Encoding.UTF8.GetBytes("contenido de prueba"));

        var storedName = await _service.ProcessAsync(
            stream,
            "test.txt",
            "text/plain",
            stream.Length);

        Assert.False(string.IsNullOrWhiteSpace(storedName));
        Assert.EndsWith(".txt", storedName);

        var storedFile = Assert.Single(_repository.Files);

        Assert.Equal("test.txt", storedFile.OriginalName);
        Assert.Equal("text/plain", storedFile.ContentType);
        Assert.Equal(stream.Length, storedFile.Size);
        Assert.Equal(storedName, storedFile.StoredName);
        Assert.Equal(storedName, _storage.SavedFileName);
        Assert.Same(stream, _storage.SavedStream);
    }

    [Fact]
    public async Task ProcessS3Async_NullStream_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.ProcessS3Async(null!, "test.txt", "text/plain", 10));
    }

    [Fact]
    public async Task ProcessS3Async_EmptyFile_ThrowsArgumentException()
    {
        using var stream = new MemoryStream();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ProcessS3Async(stream, "empty.txt", "text/plain", 0));
    }

    [Fact]
    public async Task ProcessS3Async_ValidFile_StoresMetadataAndReturnsGeneratedName()
    {
        using var stream = new MemoryStream(
            System.Text.Encoding.UTF8.GetBytes("contenido de prueba"));

        var storedName = await _service.ProcessS3Async(
            stream,
            "test.txt",
            "text/plain",
            stream.Length);

        Assert.False(string.IsNullOrWhiteSpace(storedName));
        Assert.EndsWith(".txt", storedName);

        var storedFile = Assert.Single(_repository.Files);

        Assert.Equal("test.txt", storedFile.OriginalName);
        Assert.Equal("text/plain", storedFile.ContentType);
        Assert.Equal(stream.Length, storedFile.Size);
        Assert.Equal(storedName, storedFile.StoredName);
        Assert.Equal(storedName, _s3Storage.SavedStoredName);
        Assert.Same(stream, _s3Storage.SavedStream);
        Assert.Equal("text/plain", _s3Storage.SavedContentType);
    }
}