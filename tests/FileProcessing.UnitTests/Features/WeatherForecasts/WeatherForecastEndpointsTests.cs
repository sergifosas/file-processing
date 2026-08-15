using System.Net;
using System.Net.Http.Json;
using FileProcessing.Api.Features.WeatherForecasts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;

namespace FileProcessing.UnitTests.Features.WeatherForecasts;

public class WeatherForecastEndpointsTests : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        _app = builder.Build();
        _app.MapWeatherForecastEndpoints();

        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }

    [Fact]
    public async Task MapWeatherForecastEndpoints_ServesTwentyForecastsOnGet()
    {
        var response = await _client.GetAsync("/weatherforecast");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();
        Assert.NotNull(forecasts);
        Assert.Equal(20, forecasts.Length);
    }

    [Fact]
    public async Task MapWeatherForecastEndpoints_ReturnsNotFoundForUnknownRoute()
    {
        var response = await _client.GetAsync("/unknown");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}