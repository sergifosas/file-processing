using FileProcessing.Api.Features.WeatherForecasts;

namespace FileProcessing.UnitTests.Features.WeatherForecasts;

public class WeatherForecastTests
{
    [Fact]
    public void Constructor_SetsDateTemperatureCAndSummary()
    {
        var date = new DateOnly(2026, 8, 15);

        var forecast = new WeatherForecast(date, 22, "Mild");

        Assert.Equal(date, forecast.Date);
        Assert.Equal(22, forecast.TemperatureC);
        Assert.Equal("Mild", forecast.Summary);
    }

    [Fact]
    public void Constructor_AllowsNullSummary()
    {
        var forecast = new WeatherForecast(DateOnly.MinValue, 0, null);

        Assert.Null(forecast.Summary);
    }

    [Fact]
    public void TemperatureF_ForZeroDegreesCelsius_Is32()
    {
        var forecast = new WeatherForecast(DateOnly.MinValue, 0, null);

        Assert.Equal(32, forecast.TemperatureF);
    }

    [Theory]
    [InlineData(-20, -3)]
    [InlineData(0, 32)]
    [InlineData(20, 67)]
    [InlineData(30, 85)]
    [InlineData(55, 130)]
    [InlineData(100, 211)]
    public void TemperatureF_ConvertsCelsiusToFahrenheit(int temperatureC, int expectedTemperatureF)
    {
        var forecast = new WeatherForecast(DateOnly.MinValue, temperatureC, null);

        Assert.Equal(expectedTemperatureF, forecast.TemperatureF);
    }
}