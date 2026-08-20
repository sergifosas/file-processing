using FileProcessing.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileProcessing.Api.Features.Storing;

[ApiController]
[Route("files")]
public class FileStoringController : ControllerBase
{
    private readonly FileProcessingService _fileProcessingService;

    public FileStoringController(FileProcessingService fileProcessingService)
    {
        _fileProcessingService = fileProcessingService;
    }

    [HttpPost("process")]
    public async Task<IActionResult> Process(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("File is required.");

        await _fileProcessingService.ProcessAsync(file);

        return Ok();
    }
}