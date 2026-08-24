using FileProcessing.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileProcessing.Api.Features.Files.Upload;

[ApiController]
[Route("files")]
public class UploadS3Endpoint : ControllerBase
{
    private readonly FileProcessingService _fileProcessingService;

    public UploadS3Endpoint(FileProcessingService fileProcessingService)
    {
        _fileProcessingService = fileProcessingService;
    }

    [HttpPost("uploadS3")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("File is required.");

        await using var stream = file.OpenReadStream();

        var storedName = await _fileProcessingService.ProcessS3Async(
            stream,
            file.FileName,
            file.ContentType,
            file.Length
        );

        return Ok(storedName);
    }
}