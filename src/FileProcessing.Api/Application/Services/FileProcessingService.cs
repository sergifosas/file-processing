using System.Diagnostics;
using FileProcessing.Api.Domain.Files;
using FileProcessing.Api.Domain.Metadata;

namespace FileProcessing.Api.Application.Services;

public sealed record DownloadResult(
    Stream Content,
    string ContentType,
    string OriginalName);

public record MetadataResult(
    string OriginalName,
    string Extension,
    string ContentType,
    long Size,
    string ETag,
    DateTimeOffset LastModified,
    DateTime CreatedAt,
    Dictionary<string, string>? Metadata);

public class FileProcessingService
{
    private readonly IFileStorage _storage;
    private readonly IStorage _s3Storage;
    private readonly IFileRepository _repository;

    private readonly ILogger<FileProcessingService> _logger;

    public FileProcessingService(
        IFileStorage storage,
        IStorage s3Storage,
        IFileRepository repository,
        ILogger<FileProcessingService> logger)
    {
        _storage = storage;
        _s3Storage = s3Storage;
        _repository = repository;
        _logger = logger;
    }

    public async Task<string> ProcessAsync(
        Stream file,
        string originalName,
        string contentType,
        long size)
    {
        if (file is null)
        {
            _logger.LogWarning(
                "CanonicalLog Event={Event} Outcome={Outcome} FileName={FileName}",
                "Process",
                "NullStream",
                originalName);

            throw new ArgumentNullException(nameof(file));
        }

        if (size == 0)
        {
            _logger.LogWarning(
                "CanonicalLog Event={Event} Outcome={Outcome} FileName={FileName}",
                "Process",
                "EmptyFile",
                originalName);

            throw new ArgumentException("El archivo está vacío.", nameof(size));
        }

        var timer = Stopwatch.StartNew();

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

        timer.Stop();

        _logger.LogInformation(
            "CanonicalLog Event={Event} Component={Component} Count={Count} " +
            "Storage={Storage} FileName={FileName} FileSizeBytes={FileSize} " +
            "ContentType={ContentType} StoredName={StoredName} " +
            "DurationMs={DurationMs}",
            "Process",
            "FileProcessing.Api",
            1,
            "Local",
            originalName,
            size,
            contentType,
            storedName,
            timer.Elapsed.TotalMilliseconds);

        return storedName;
    }

    public async Task<string> ProcessS3Async(
        Stream file,
        string originalName,
        string contentType,
        long size)
    {
        if (file is null)
        {
            _logger.LogWarning(
                "CanonicalLog Event={Event} Outcome={Outcome} FileName={FileName}",
                "ProcessS3",
                "NullStream",
                originalName);

            throw new ArgumentNullException(nameof(file));
        }

        if (size <= 0)
        {
            _logger.LogWarning(
                "CanonicalLog Event={Event} Outcome={Outcome} FileName={FileName}",
                "ProcessS3",
                "EmptyFile",
                originalName);

            throw new ArgumentException(
                "El archivo está vacío.",
                nameof(size));
        }

        var timer = Stopwatch.StartNew();

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

        timer.Stop();

        _logger.LogInformation(
            "CanonicalLog Event={Event} Component={Component} Count={Count} " +
            "Storage={Storage} FileName={FileName} FileSizeBytes={FileSize} " +
            "ContentType={ContentType} StoredName={StoredName} " +
            "DurationMs={DurationMs}",
            "ProcessS3",
            "FileProcessing.Api",
            1,
            "S3",
            originalName,
            size,
            contentType,
            storedName,
            timer.Elapsed.TotalMilliseconds);

        return storedName;
    }

    public async Task<DownloadResult?> DownloadS3Async(string storedName)
    {
        if (string.IsNullOrWhiteSpace(storedName))
        {
            _logger.LogWarning(
                "CanonicalLog Event={Event} Outcome={Outcome} StoredName={StoredName}",
                "DownloadS3",
                "InvalidName",
                storedName);

            throw new ArgumentException(
                "El nombre almacenado no es válido.",
                nameof(storedName));
        }

        var timer = Stopwatch.StartNew();

        var storedFile = await _repository.GetAsync(storedName);

        if (storedFile is null)
        {
            timer.Stop();

            _logger.LogInformation(
                "CanonicalLog Event={Event} Component={Component} Outcome={Outcome} " +
                "Storage={Storage} StoredName={StoredName} DurationMs={DurationMs}",
                "DownloadS3",
                "FileProcessing.Api",
                "NotFound",
                "S3",
                storedName,
                timer.Elapsed.TotalMilliseconds);

            return null;
        }

        var content = await _s3Storage.GetAsync(storedName);

        timer.Stop();

        _logger.LogInformation(
            "CanonicalLog Event={Event} Component={Component} Count={Count} " +
            "Outcome={Outcome} Storage={Storage} StoredName={StoredName} " +
            "OriginalName={OriginalName} ContentType={ContentType} " +
            "DurationMs={DurationMs}",
            "DownloadS3",
            "FileProcessing.Api",
            1,
            "Success",
            "S3",
            storedName,
            storedFile.OriginalName,
            storedFile.ContentType,
            timer.Elapsed.TotalMilliseconds);

        return new DownloadResult(
            content,
            storedFile.ContentType,
            storedFile.OriginalName);
    }



    public async Task<MetadataResult?> ObtainMetadataAsync(string storedName)
    {
        if (string.IsNullOrWhiteSpace(storedName))
        {
            _logger.LogWarning(
                "CanonicalLog Event={Event} Outcome={Outcome} StoredName={StoredName}",
                "ObtainMetadata",
                "InvalidName",
                storedName);

            throw new ArgumentException(
                "El nombre almacenado no es válido.",
                nameof(storedName));
        }

        var timer = Stopwatch.StartNew();

        var storedFile = await _repository.GetAsync(storedName);

        if (storedFile is null)
        {
            timer.Stop();

            _logger.LogInformation(
                "CanonicalLog Event={Event} Component={Component} Outcome={Outcome} " +
                "Storage={Storage} StoredName={StoredName} DurationMs={DurationMs}",
                "ObtainMetadata",
                "FileProcessing.Api",
                "NotFound",
                "S3",
                storedName,
                timer.Elapsed.TotalMilliseconds);

            return null;
        }

        var s3Metadata = await _s3Storage.GetMetadataAsync(storedName);

        if (s3Metadata is null)
        {
            timer.Stop();

            _logger.LogInformation(
                "CanonicalLog Event={Event} Component={Component} Outcome={Outcome} " +
                "Storage={Storage} StoredName={StoredName} DurationMs={DurationMs}",
                "ObtainMetadata",
                "FileProcessing.Api",
                "NotFound",
                "S3",
                storedName,
                timer.Elapsed.TotalMilliseconds);

            return null;
        }

        timer.Stop();

        _logger.LogInformation(
            "CanonicalLog Event={Event} Component={Component} Count={Count} " +
            "Outcome={Outcome} Storage={Storage} StoredName={StoredName} " +
            "OriginalName={OriginalName} ContentType={ContentType} " +
            "DurationMs={DurationMs}",
            "ObtainMetadata",
            "FileProcessing.Api",
            1,
            "Success",
            "S3",
            storedName,
            storedFile.OriginalName,
            storedFile.ContentType,
            timer.Elapsed.TotalMilliseconds);

        return new MetadataResult(
            storedFile.OriginalName,
            Path.GetExtension(storedFile.OriginalName),
            s3Metadata.ContentType,
            s3Metadata.Size,
            s3Metadata.ETag,
            s3Metadata.LastModified,
            storedFile.CreatedAt,
            s3Metadata.Metadata);
    }
}