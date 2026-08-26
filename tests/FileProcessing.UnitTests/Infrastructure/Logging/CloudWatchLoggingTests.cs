using Amazon;
using FileProcessing.Api.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Parsing;
using Serilog.Sinks.AwsCloudWatch;

namespace FileProcessing.UnitTests.Infrastructure.Logging;

public class CloudWatchLoggingTests
{
    [Theory]
    [InlineData("Development", true)]
    [InlineData("Production", true)]
    [InlineData("Staging", true)]
    [InlineData("Testing", false)]
    [InlineData(null, true)]
    [InlineData("", true)]
    public void CanWriteToCloudWatch_CombinesEnvironmentAndLogGroup(
        string? environment,
        bool expected)
    {
        var options = new CloudWatchOptions
        {
            LogGroupName = "file-processing-logs"
        };

        var result = CloudWatchLoggingExtensions.CanWriteToCloudWatch(
            environment,
            options);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CanWriteToCloudWatch_WithoutLogGroup_ReturnsFalse(string? logGroup)
    {
        var options = new CloudWatchOptions { LogGroupName = logGroup };

        var result = CloudWatchLoggingExtensions.CanWriteToCloudWatch(
            "Development",
            options);

        Assert.False(result);
    }

    [Fact]
    public void CreateSinkOptions_UsesConfiguredLogGroupFormatterAndStreamProvider()
    {
        var options = CloudWatchLoggingExtensions.CreateSinkOptions(
            new CloudWatchOptions { LogGroupName = "my-group" });

        Assert.Equal("my-group", options.LogGroupName);
        Assert.IsType<CompactJsonFormatter>(options.TextFormatter);
        Assert.IsType<DefaultLogStreamProvider>(options.LogStreamNameProvider);
    }

    [Theory]
    [InlineData("CanonicalLog Event={Event} Outcome={Outcome} FileName={FileName}")]
    [InlineData("CanonicalLog Event={Event} Component={Component} Count={Count} Storage={Storage}")]
    [InlineData("CanonicalLog Event={Event} Component={Component} Outcome={Outcome} DurationMs={DurationMs}")]
    [InlineData("CanonicalLog")]
    public void IsCanonicalLogEvent_MatchesTemplatesStartingWithCanonicalLog(string template)
    {
        var logEvent = CreateLogEvent(template);

        Assert.True(CloudWatchLoggingExtensions.IsCanonicalLogEvent(logEvent));
    }

    [Theory]
    [InlineData("Request finished {Method} {Path}")]
    [InlineData("Application started. Press Ctrl+C to shut down.")]
    [InlineData("Now listening on: {address}")]
    [InlineData("Hosting environment: {EnvName}")]
    [InlineData("")]
    public void IsCanonicalLogEvent_DoesNotMatchOtherTemplates(string template)
    {
        var logEvent = CreateLogEvent(template);

        Assert.False(CloudWatchLoggingExtensions.IsCanonicalLogEvent(logEvent));
    }

    private static LogEvent CreateLogEvent(string template)
    {
        var messageTemplate = new MessageTemplateParser().Parse(template);
        return new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            exception: null,
            messageTemplate,
            Enumerable.Empty<LogEventProperty>());
    }

    [Theory]
    [InlineData("eu-west-1")]
    [InlineData("us-east-1")]
    [InlineData("us-west-2")]
    public void ResolveRegion_ResolvesKnownRegion(string systemName)
    {
        var region = CloudWatchLoggingExtensions.ResolveRegion(systemName);

        Assert.Equal(systemName, region.SystemName);
    }

    [Fact]
    public void ResolveRegion_FromNullOrBlank_DefaultsToEuWest1()
    {
        Assert.Equal(
            RegionEndpoint.EUWest1.SystemName,
            CloudWatchLoggingExtensions.ResolveRegion(null).SystemName);
        Assert.Equal(
            RegionEndpoint.EUWest1.SystemName,
            CloudWatchLoggingExtensions.ResolveRegion(" ").SystemName);
    }

    [Fact]
    public void CreateCloudWatchClient_WithServiceUrl_UsesLocalStackEndpoint()
    {
        var client = CloudWatchLoggingExtensions.CreateCloudWatchClient(
            new AwsOptions
            {
                Region = "eu-west-1",
                AccessKey = "test",
                SecretKey = "test",
                ServiceUrl = "http://localhost:4566"
            });

        // Al fijar ServiceURL (LocalStack), el SDK usa esa URL y no expone
        // RegionEndpoint en la configuración; la región se valida en los demás tests.
        Assert.Equal("http://localhost:4566/", client.Config.ServiceURL);
    }

    [Fact]
    public void CreateCloudWatchClient_WithoutServiceUrl_UsesAwsRegionalEndpoint()
    {
        var client = CloudWatchLoggingExtensions.CreateCloudWatchClient(
            new AwsOptions
            {
                Region = "eu-west-1",
                AccessKey = "test",
                SecretKey = "test"
            });

        Assert.Equal("eu-west-1", client.Config.RegionEndpoint.SystemName);
    }

    [Fact]
    public void CreateCloudWatchClient_WithoutCredentials_UsesDefaultChain()
    {
        // Sin claves estáticas debe poder construirse igualmente para usar la
        // cadena de credenciales por defecto (rol IAM / variables de entorno).
        var client = CloudWatchLoggingExtensions.CreateCloudWatchClient(
            new AwsOptions
            {
                Region = "eu-west-1"
            });

        Assert.NotNull(client);
        Assert.Equal("eu-west-1", client.Config.RegionEndpoint.SystemName);
    }

    [Fact]
    public void Options_BindFromConfigurationSections()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AWS:Region"] = "eu-west-1",
                ["AWS:AccessKey"] = "access-key",
                ["AWS:SecretKey"] = "secret-key",
                ["AWS:ServiceUrl"] = "http://localhost:4566",
                ["AWS:CloudWatch:LogGroupName"] = "file-processing-logs"
            })
            .Build();

        var aws = config.GetSection("AWS").Get<AwsOptions>();
        var cloudWatch = config.GetSection("AWS:CloudWatch").Get<CloudWatchOptions>();

        Assert.NotNull(aws);
        Assert.Equal("eu-west-1", aws!.Region);
        Assert.Equal("access-key", aws.AccessKey);
        Assert.Equal("secret-key", aws.SecretKey);
        Assert.Equal("http://localhost:4566", aws.ServiceUrl);

        Assert.NotNull(cloudWatch);
        Assert.Equal("file-processing-logs", cloudWatch!.LogGroupName);
    }
}