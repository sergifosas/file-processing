namespace FileProcessing.Api.Infrastructure.Logging;

/// <summary>
/// Configuración del sink de CloudWatch, enlazada desde la sección "AWS:CloudWatch" del appsettings.
/// </summary>
public sealed class CloudWatchOptions
{
    public string? LogGroupName { get; set; }
}
