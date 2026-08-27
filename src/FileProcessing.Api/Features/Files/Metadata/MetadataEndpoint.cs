using FileProcessing.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileProcessing.Api.Features.Files.Metadata;

[ApiController]
[Route("files")]
public class MetadataEndpoint : ControllerBase
{
    private readonly FileProcessingService _fileProcessingService;

    public MetadataEndpoint(FileProcessingService fileProcessingService)
    {
        _fileProcessingService = fileProcessingService;
    }

    [HttpGet("metadata/{storedName}")]
    public async Task<IActionResult> GetMetadata(string storedName)
    {
        if (string.IsNullOrWhiteSpace(storedName))
            return BadRequest("Stored name is required.");

        var metadata = await _fileProcessingService.ObtainMetadataAsync(storedName);

        if (metadata is null)
            return NotFound("File not found.");

        return Ok(metadata);
    }
}