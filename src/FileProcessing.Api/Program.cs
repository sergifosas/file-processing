using FileProcessing.Api.Features.WeatherForecasts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Se omite la redirección HTTPS durante los tests de integración (TestServer no expone un puerto HTTPS).
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.MapWeatherForecastEndpoints();


app.MapControllers();

app.Run();

public partial class Program { }
