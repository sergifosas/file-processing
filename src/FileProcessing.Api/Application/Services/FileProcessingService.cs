namespace FileProcessing.Api.Application.Services;

public class FileProcessingService
{
    public async Task ProcessAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
    }
}