using Amazon;
using Amazon.CloudWatchLogs;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using Serilog.Sinks.AwsCloudWatch;

namespace FileProcessing.Api.Infrastructure.Logging;

public static class CloudWatchLoggingExtensions
{
    public static void AddCloudWatchLogging(
    this IHostApplicationBuilder builder)
    {
        var awsOptions = builder.Configuration
            .GetSection("AWS")
            .Get<AwsOptions>() ?? new AwsOptions();

        var cloudWatchOptions = builder.Configuration
            .GetSection("AWS:CloudWatch")
            .Get<CloudWatchOptions>() ?? new CloudWatchOptions();

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty(
                "Service",
                "FileProcessing.Api")
            .Enrich.WithProperty(
                "Environment",
                builder.Environment.EnvironmentName)
            .WriteTo.Console(
                new CompactJsonFormatter());

        if (CanWriteToCloudWatch(
                builder.Environment.EnvironmentName,
                cloudWatchOptions))
        {
            // Solo los "canonical logs" (cuyo mensaje empieza por "CanonicalLog")
            // se envían a CloudWatch. El resto (arranque del host, request logs,
            // etc.) queda únicamente en la consola.
            loggerConfiguration.WriteTo.Logger(
                lc => lc
                    .Filter.ByIncludingOnly(IsCanonicalLogEvent)
                    .WriteTo.AmazonCloudWatch(
                        CreateSinkOptions(cloudWatchOptions),
                        CreateCloudWatchClient(awsOptions)));
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        builder.Services.AddSerilog();
    }

    internal static bool CanWriteToCloudWatch(
        string? environmentName,
        CloudWatchOptions options) =>
        !string.Equals(
            environmentName,
            "Testing",
            StringComparison.OrdinalIgnoreCase) &&
        !string.IsNullOrWhiteSpace(
            options.LogGroupName);

    internal static bool IsCanonicalLogEvent(LogEvent logEvent) =>
        logEvent.MessageTemplate.Text.StartsWith(
            "CanonicalLog",
            StringComparison.Ordinal);

    internal static RegionEndpoint ResolveRegion(
        string? region) =>
        string.IsNullOrWhiteSpace(region)
            ? RegionEndpoint.EUWest1
            : RegionEndpoint.GetBySystemName(region);

    internal static CloudWatchSinkOptions CreateSinkOptions(
        CloudWatchOptions options) =>
        new()
        {
            LogGroupName = options.LogGroupName!,
            TextFormatter = new CompactJsonFormatter(),
            LogStreamNameProvider =
                new DefaultLogStreamProvider()
        };

    internal static AmazonCloudWatchLogsClient CreateCloudWatchClient(
        AwsOptions options)
    {
        var regionEndpoint =
            ResolveRegion(options.Region);

        var hasCredentials =
            !string.IsNullOrWhiteSpace(options.AccessKey) &&
            !string.IsNullOrWhiteSpace(options.SecretKey);

        if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
        {
            var config = new AmazonCloudWatchLogsConfig
            {
                RegionEndpoint = regionEndpoint,
                ServiceURL = options.ServiceUrl,
                // LocalStack/Amazon firman cada petición con la región de
                // autenticación. Si usamos ServiceURL (LocalStack) el SDK usa
                // "us-east-1" por defecto, y entonces los logs se almacenan en
                // un namespace distinto al que consulta la CLI (--region). Al
                // fijarla a la región configurada, ambos coinciden.
                AuthenticationRegion = regionEndpoint.SystemName
            };

            return hasCredentials
                ? new AmazonCloudWatchLogsClient(
                    options.AccessKey!,
                    options.SecretKey!,
                    config)
                : new AmazonCloudWatchLogsClient(config);
        }

        return hasCredentials
            ? new AmazonCloudWatchLogsClient(
                options.AccessKey!,
                options.SecretKey!,
                regionEndpoint)
            : new AmazonCloudWatchLogsClient(
                regionEndpoint);
    }
}