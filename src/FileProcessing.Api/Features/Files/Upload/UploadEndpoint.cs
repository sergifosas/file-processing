using FileProcessing.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileProcessing.Api.Features.Files.Upload;

[ApiController]
[Route("files")]
public class UploadEndpoint : ControllerBase
{
    private readonly FileProcessingService _fileProcessingService;

    public UploadEndpoint(FileProcessingService fileProcessingService)
    {
        _fileProcessingService = fileProcessingService;
    }

    [HttpPost("process")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("File is required.");

        await using var stream = file.OpenReadStream();

        var storedName = await _fileProcessingService.ProcessAsync(
            stream,
            file.FileName,
            file.ContentType,
            file.Length
        );

        return Ok(storedName);
    }
}