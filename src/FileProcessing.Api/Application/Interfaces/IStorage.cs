namespace FileProcessing.Api.Application.Services;

public interface IStorage
{
    Task<string> SaveAsync(
        Stream file,
        string storedName,
        string contentType);

    Task<Stream> GetAsync(string storedName);
}