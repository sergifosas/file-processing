namespace FileProcessing.Api.Application.Services;

public class FileProcessingService
{
    public async Task<string> ProcessAsync(IFormFile file)
    {
        if (file is null)
            throw new ArgumentNullException(nameof(file));

        if (file.Length == 0)
            throw new ArgumentException("El archivo está vacío.", nameof(file));

        var uploadsPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "uploads"
        );

        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsPath, fileName);

        await using var stream = new FileStream(
            filePath,
            FileMode.Create
        );

        await file.CopyToAsync(stream);

        return fileName;
    }
}