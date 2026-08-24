using FileProcessing.Api.Domain.Files;

namespace FileProcessing.Api.Application.Services;

public interface IFileRepository
{
    Task AddAsync(StoredFile file);
}