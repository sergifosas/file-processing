using FileProcessing.Api.Features.WeatherForecasts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Se omite la redirección HTTPS durante los tests de integración (TestServer no expone un puerto HTTPS).
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.MapWeatherForecastEndpoints();

app.Run();

public partial class Program { }
