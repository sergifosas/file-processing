using FileProcessing.Api.Application.Services;
using Microsoft.Extensions.Options;

namespace FileProcessing.Api.Infrastructure.Storage;

public sealed class LocalFileStorageOptions
{
    public string UploadsPath { get; set; } = "uploads";
}

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _uploadsPath;

    public LocalFileStorage(IOptions<LocalFileStorageOptions> options)
    {
        _uploadsPath = Path.GetFullPath(options.Value.UploadsPath);
    }

    public async Task<string> SaveAsync(Stream stream, string fileName)
    {
        Directory.CreateDirectory(_uploadsPath);

        var filePath = Path.Combine(_uploadsPath, fileName);

        await using (var target = new FileStream(filePath, FileMode.Create))
        {
            await stream.CopyToAsync(target);
        }

        return filePath;
    }
}