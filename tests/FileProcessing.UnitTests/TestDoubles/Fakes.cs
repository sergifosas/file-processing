using FileProcessing.Api.Application.Services;
using FileProcessing.Api.Domain.Files;

namespace FileProcessing.UnitTests.TestDoubles;

public sealed class FakeFileStorage : IFileStorage
{
    public Stream? SavedStream { get; private set; }

    public string? SavedFileName { get; private set; }

    public Task<string> SaveAsync(Stream stream, string fileName)
    {
        SavedStream = stream;
        SavedFileName = fileName;

        return Task.FromResult(Path.Combine("uploads", fileName));
    }
}

public sealed class FakeS3Storage : IStorage
{
    public Stream? SavedStream { get; private set; }

    public string? SavedStoredName { get; private set; }

    public string? SavedContentType { get; private set; }

    public Task<string> SaveAsync(
        Stream stream,
        string storedName,
        string contentType)
    {
        SavedStream = stream;
        SavedStoredName = storedName;
        SavedContentType = contentType;

        return Task.FromResult(storedName);
    }
}

public sealed class FakeFileRepository : IFileRepository
{
    public List<StoredFile> Files { get; } = [];

    public Task AddAsync(StoredFile file)
    {
        Files.Add(file);
        return Task.CompletedTask;
    }
}