using System.Net;
using System.Net.Http.Json;
using FileProcessing.Api.Features.WeatherForecasts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FileProcessing.IntegrationTests;

public class WeatherForecastEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WeatherForecastEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsOkWithJsonContentType()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/weatherforecast");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsTwentyForecasts()
    {
        var forecasts = await _factory.CreateClient().GetFromJsonAsync<WeatherForecast[]>("/weatherforecast");

        Assert.NotNull(forecasts);
        Assert.Equal(20, forecasts.Length);
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsValidForecastData()
    {
        var forecasts = await _factory.CreateClient().GetFromJsonAsync<WeatherForecast[]>("/weatherforecast");

        Assert.NotNull(forecasts);

        var today = DateOnly.FromDateTime(DateTime.Today);
        Assert.All(forecasts, forecast =>
        {
            // El endpoint genera fechas entre mañana (índice 1) y +20 días (índice 20).
            Assert.True(forecast.Date >= today);
            Assert.True(forecast.Date <= today.AddDays(20));

            // Random.Shared.Next(-20, 55) genera valores de -20 a 54 inclusive.
            Assert.InRange(forecast.TemperatureC, -20, 54);
            Assert.Equal(32 + (int)(forecast.TemperatureC / 0.5556), forecast.TemperatureF);
            Assert.False(string.IsNullOrWhiteSpace(forecast.Summary));
        });
    }

    [Fact]
    public async Task UnknownRoute_ReturnsNotFound()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}