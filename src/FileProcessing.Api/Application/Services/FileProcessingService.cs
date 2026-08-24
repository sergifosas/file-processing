using FileProcessing.Api.Domain.Files;

namespace FileProcessing.Api.Application.Services;

public sealed record DownloadResult(
    Stream Content,
    string ContentType,
    string OriginalName);

public class FileProcessingService
{
    private readonly IFileStorage _storage;
    private readonly IStorage _s3Storage;
    private readonly IFileRepository _repository;

    public FileProcessingService(
        IFileStorage storage,
        IStorage s3Storage,
        IFileRepository repository)
    {
        _storage = storage;
        _s3Storage = s3Storage;
        _repository = repository;
    }

    public async Task<string> ProcessAsync(
        Stream file,
        string originalName,
        string contentType,
        long size)
    {
        if (file is null)
            throw new ArgumentNullException(nameof(file));

        if (size == 0)
            throw new ArgumentException("El archivo está vacío.", nameof(size));

        var storedName =
            $"{Guid.NewGuid()}{Path.GetExtension(originalName)}";

        var path = await _storage.SaveAsync(
            file,
            storedName
        );

        var storedFile = new StoredFile
        {
            OriginalName = originalName,
            StoredName = storedName,
            ContentType = contentType,
            Size = size,
            Path = path,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(storedFile);

        return storedName;
    }

    public async Task<string> ProcessS3Async(
        Stream file,
        string originalName,
        string contentType,
        long size)
    {
        if (file is null)
            throw new ArgumentNullException(nameof(file));

        if (size <= 0)
            throw new ArgumentException(
                "El archivo está vacío.",
                nameof(size));

        var storedName =
            $"{Guid.NewGuid()}{Path.GetExtension(originalName)}";

        var path = await _s3Storage.SaveAsync(
            file,
            storedName,
            contentType);

        var storedFile = new StoredFile
        {
            OriginalName = originalName,
            StoredName = storedName,
            ContentType = contentType,
            Size = size,
            Path = path,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(storedFile);

        return storedName;
    }

    public async Task<DownloadResult?> DownloadS3Async(string storedName)
    {
        if (string.IsNullOrWhiteSpace(storedName))
            throw new ArgumentException(
                "El nombre almacenado no es válido.",
                nameof(storedName));

        var storedFile = await _repository.GetAsync(storedName);

        if (storedFile is null)
            return null;

        var content = await _s3Storage.GetAsync(storedName);

        return new DownloadResult(
            content,
            storedFile.ContentType,
            storedFile.OriginalName);
    }
}