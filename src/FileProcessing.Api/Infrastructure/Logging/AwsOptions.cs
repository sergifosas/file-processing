namespace FileProcessing.Api.Infrastructure.Logging;

/// <summary>
/// Configuración de AWS, enlazada desde la sección "AWS" del appsettings.
/// </summary>
public sealed class AwsOptions
{
    public string? Region { get; set; }

    public string? AccessKey { get; set; }

    public string? SecretKey { get; set; }

    /// <summary>Endpoint opcional, útil para desarrollos locales contra LocalStack.</summary>
    public string? ServiceUrl { get; set; }
}
