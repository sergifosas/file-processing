using FileProcessing.Api.Application.Services;
using FileProcessing.Api.Domain.Files;

namespace FileProcessing.Api.Infrastructure.Persistence;

public class FileRepository : IFileRepository
{
    private readonly FileProcessingDbContext _context;

    public FileRepository(FileProcessingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(StoredFile file)
    {
        _context.Files.Add(file);
        await _context.SaveChangesAsync();
    }
}