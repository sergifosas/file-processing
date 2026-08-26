using System.IO;
using System.Net.Sockets;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using FileProcessing.Api.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace FileProcessing.IntegrationTests.Logging;

/// <summary>
/// Tests funcionales del logging a CloudWatch. Ejercen el arranque real del host
/// (IHostApplicationBuilder) y el pipeline de Serilog configurado por
/// <see cref="CloudWatchLoggingExtensions.AddCloudWatchLogging"/>.
/// </summary>
public class CloudWatchLoggingFunctionalTests
{
    [Fact]
    public void Logging_WithCloudWatchEnabled_StillWritesToConsole()
    {
        using var output = new StringWriter();
        var previousLogger = Log.Logger;
        var previousOut = Console.Out;
        Console.SetOut(output);

        try
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Environment.EnvironmentName = "Development";
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AWS:AccessKey"] = "test",
                ["AWS:SecretKey"] = "test",
                ["AWS:Region"] = "eu-west-1",
                ["AWS:ServiceUrl"] = "http://localhost:4566",
                ["AWS:CloudWatch:LogGroupName"] = "file-processing-logs-functional"
            });

            builder.AddCloudWatchLogging();

            Assert.IsType<Serilog.Core.Logger>(Log.Logger);

            var marker = Guid.NewGuid();
            Log.Information("console-check-still-logs {Marker}", marker);

            Log.CloseAndFlush();

            var text = output.ToString();

            // Con CloudWatch activo la consola sigue mostrando los logs
            // (solo se filtra lo que llega a CloudWatch, no la consola).
            Assert.Contains(marker.ToString(), text, StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(previousOut);
            Log.CloseAndFlush();
            Log.Logger = previousLogger;
        }
    }

    [Fact]
    public void Logging_SkipsCloudWatch_AndStillConfiguresLogging_WhenTestingEnvironment()
    {
        var previousLogger = Log.Logger;

        try
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Environment.EnvironmentName = "Testing";
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AWS:AccessKey"] = "test",
                ["AWS:SecretKey"] = "test",
                ["AWS:Region"] = "eu-west-1",
                ["AWS:ServiceUrl"] = "http://localhost:4566",
                ["AWS:CloudWatch:LogGroupName"] = "file-processing-logs-functional"
            });

            // No debe lanzar y debe dejar Serilog configurado (fallback a consola).
            builder.AddCloudWatchLogging();

            Assert.IsType<Serilog.Core.Logger>(Log.Logger);

            Log.Information("testing-fallback {Marker}", Guid.NewGuid());
        }
        finally
        {
            Log.CloseAndFlush();
            Log.Logger = previousLogger;
        }
    }

    [Fact]
    public void Logging_WithoutLogGroup_StillConfiguresLogging()
    {
        var previousLogger = Log.Logger;

        try
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Environment.EnvironmentName = "Production";
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AWS:AccessKey"] = "test",
                ["AWS:SecretKey"] = "test",
                ["AWS:Region"] = "eu-west-1",
                ["AWS:ServiceUrl"] = "http://localhost:4566"
            });

            // Sin LogGroupName CloudWatch no debe activarse; Serilog sigue activo.
            builder.AddCloudWatchLogging();

            Assert.IsType<Serilog.Core.Logger>(Log.Logger);

            Log.Information("no-loggroup-context {Marker}", Guid.NewGuid());
        }
        finally
        {
            Log.CloseAndFlush();
            Log.Logger = previousLogger;
        }
    }

    [Fact]
    public async Task CloudWatch_ReceivesLogs_WhenLocalStackIsRunning()
    {
        if (!await IsLocalStackReachableAsync())
        {
            // LocalStack no está disponible: CI se mantiene verde y el test
            // solo ejercita el flujo completo cuando el endpoint existe.
            return;
        }

        var previousLogger = Log.Logger;
        var groupName = $"file-processing-functional-{Guid.NewGuid():N}";
        var marker = Guid.NewGuid().ToString("N");

        try
        {
            var builder = Host.CreateApplicationBuilder();
            builder.Environment.EnvironmentName = "Development";
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AWS:AccessKey"] = "test",
                ["AWS:SecretKey"] = "test",
                ["AWS:Region"] = "eu-west-1",
                ["AWS:ServiceUrl"] = "http://localhost:4566",
                ["AWS:CloudWatch:LogGroupName"] = groupName
            });

            builder.AddCloudWatchLogging();

            // Mensaje canónico (empieza por "CanonicalLog"): es el único tipo
            // que el filtro reenvía a CloudWatch.
            Log.Information(
                "CanonicalLog Event={Event} Outcome={Outcome} Marker={Marker}",
                "Functional",
                "Roundtrip",
                marker);
            Log.CloseAndFlush();

            using var client = new AmazonCloudWatchLogsClient(
                "test",
                "test",
                new AmazonCloudWatchLogsConfig
                {
                    RegionEndpoint = RegionEndpoint.EUWest1,
                    ServiceURL = "http://localhost:4566",
                    AuthenticationRegion = RegionEndpoint.EUWest1.SystemName
                });

            var found = await WaitForMarkerAsync(client, groupName, marker);

            Assert.True(
                found,
                $"No se encontró en CloudWatch el evento con marcador '{marker}'.");
        }
        finally
        {
            Log.CloseAndFlush();
            Log.Logger = previousLogger;
        }
    }

    private static async Task<bool> IsLocalStackReachableAsync()
    {
        try
        {
            using var tcpClient = new TcpClient();
            var connect = tcpClient.ConnectAsync("127.0.0.1", 4566);
            var completed = await Task.WhenAny(
                connect,
                Task.Delay(TimeSpan.FromSeconds(1)));

            if (completed != connect)
            {
                return false;
            }

            await connect;
            return tcpClient.Connected;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static async Task<bool> WaitForMarkerAsync(
        AmazonCloudWatchLogsClient client,
        string groupName,
        string marker,
        int seconds = 10)
    {
        var deadline = DateTime.UtcNow.AddSeconds(seconds);

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var streamsResponse = await client.DescribeLogStreamsAsync(
                    new DescribeLogStreamsRequest
                    {
                        LogGroupName = groupName,
                        OrderBy = "LastEventTime"
                    });

                foreach (var stream in streamsResponse.LogStreams)
                {
                    var eventsResponse = await client.GetLogEventsAsync(
                        new GetLogEventsRequest
                        {
                            LogGroupName = groupName,
                            LogStreamName = stream.LogStreamName
                        });

                    if (eventsResponse.Events.Any(e =>
                            e.Message != null &&
                            e.Message.Contains(marker, StringComparison.Ordinal)))
                    {
                        return true;
                    }
                }
            }
            catch (AmazonCloudWatchLogsException)
            {
                // El grupo/timeline aún se está creando en LocalStack; reintentamos.
            }

            await Task.Delay(TimeSpan.FromMilliseconds(750));
        }

        return false;
    }
}