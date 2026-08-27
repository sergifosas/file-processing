namespace FileProcessing.Api.Domain.Metadata;

public class FileMetadata
{
    public string StoredName { get; set; } = string.Empty;

    public string Extension { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long Size { get; set; }

    public string ETag { get; set; } = string.Empty;

    public DateTimeOffset LastModified { get; set; }

    public string? ContentEncoding { get; set; }

    public string? ContentLanguage { get; set; }

    public string? CacheControl { get; set; }

    public string? StorageClass { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }
}