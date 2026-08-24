namespace FileProcessing.Api.Application.Services;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream stream, string fileName);
}