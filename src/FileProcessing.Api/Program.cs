using FileProcessing.Api.Application.Services;
using FileProcessing.Api.Infrastructure.Logging;
using FileProcessing.Api.Infrastructure.Persistence;
using FileProcessing.Api.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Amazon;
using Amazon.S3;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Sobrecarga opcional con valores locales (credenciales de desarrollo).
// No debe commitearse (ver .gitignore: appsettings.*.Local.json).
builder.Configuration.AddJsonFile(
    "appsettings.Local.json",
    optional: true,
    reloadOnChange: true);

builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.AddCloudWatchLogging();
builder.Services.AddSwaggerGen();

builder.Services.Configure<LocalFileStorageOptions>(
    builder.Configuration.GetSection("Storage"));

builder.Services.Configure<AwsOptions>(
    builder.Configuration.GetSection("AWS"));

builder.Services.Configure<CloudWatchOptions>(
    builder.Configuration.GetSection("AWS:CloudWatch"));

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


var awsConfiguration = builder.Configuration.GetSection("AWS");

var s3Config = new AmazonS3Config
{
    ServiceURL = awsConfiguration["ServiceUrl"],
    ForcePathStyle = true
};

var s3Client = new AmazonS3Client(
    awsConfiguration["AccessKey"],
    awsConfiguration["SecretKey"],
    s3Config);

builder.Services.AddSingleton<IAmazonS3>(s3Client);

builder.Services.AddScoped<IStorage, S3Storage>();

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

app.UseSerilogRequestLogging();

app.MapControllers();

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}