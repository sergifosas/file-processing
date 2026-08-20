namespace FileProcessing.Api.Domain.Files;

public class StoredFile
{
    public long Id { get; set; }

    public required string OriginalName { get; set; }

    public required string StoredName { get; set; }

    public required string ContentType { get; set; }

    public long Size { get; set; }

    public required string Path { get; set; }

    public DateTime CreatedAt { get; set; }
}