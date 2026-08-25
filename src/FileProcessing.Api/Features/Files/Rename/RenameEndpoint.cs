using FileProcessing.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileProcessing.Api.Features.Files.Rename;

[ApiController]
[Route("files")]
public class RenameEndpoint : ControllerBase
{
    private readonly FileProcessingService _fileProcessingService;

    public RenameEndpoint(FileProcessingService fileProcessingService)
    {
        _fileProcessingService = fileProcessingService;
    }

    [HttpPost("rename")]
    public async Task<IActionResult> Rename(IFormFile file, string newFileName)
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

        return File(file.OpenReadStream(), file.ContentType, newFileName);
    }

}