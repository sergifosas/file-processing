using FileProcessing.Api.Application.Services;
using FileProcessing.Api.Infrastructure.Persistence;
using FileProcessing.Api.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<LocalFileStorageOptions>(
    builder.Configuration.GetSection("Storage"));

builder.Services.AddScoped<FileProcessingService>();
builder.Services.AddScoped<IFileStorage, LocalFileStorage>();
builder.Services.AddScoped<IFileRepository, FileRepository>();

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<FileProcessingDbContext>(options =>
        options.UseInMemoryDatabase("FileProcessingTesting"));
}
else
{
    builder.Services.AddDbContext<FileProcessingDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection")));
}

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

app.MapControllers();

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}