using FileProcessing.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileProcessing.Api.Features.Files.Download;

[ApiController]
[Route("files")]
public class DownloadS3Endpoint : ControllerBase
{
    private readonly FileProcessingService _fileProcessingService;

    public DownloadS3Endpoint(FileProcessingService fileProcessingService)
    {
        _fileProcessingService = fileProcessingService;
    }

    [HttpGet("downloadS3/{storedName}")]
    public async Task<IActionResult> Download(string storedName)
    {
        var result = await _fileProcessingService.DownloadS3Async(storedName);

        if (result is null)
            return NotFound(
                $"No se encontró ningún archivo con el nombre '{storedName}'.");

        return File(result.Content, result.ContentType, result.OriginalName);
    }
}