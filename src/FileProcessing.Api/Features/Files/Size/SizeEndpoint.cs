using FileProcessing.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileProcessing.Api.Features.Files.Size;

[ApiController]
[Route("files")]
public class SizeEndpoint : ControllerBase
{
    private readonly FileProcessingService _fileProcessingService;

    public SizeEndpoint(FileProcessingService fileProcessingService)
    {
        _fileProcessingService = fileProcessingService;
    }

    [HttpPost("size")]
    public async Task<IActionResult> GetSize(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("File is required.");

        long sizeInBytes = file.Length;
        double sizeInMb = sizeInBytes / (1024.0 * 1024.0);

        await using var stream = file.OpenReadStream();

        var storedName = await _fileProcessingService.ProcessAsync(
            stream,
            file.FileName,
            file.ContentType,
            sizeInBytes
        );

        return Ok(new
        {
            StoredName = storedName,
            FileSizeInBytes = sizeInBytes,
            FileSizeInMb = Math.Round(sizeInMb, 2)
        });
    }

}