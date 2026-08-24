using FileProcessing.Api.Domain.Files;

namespace FileProcessing.Api.Application.Services;

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
}