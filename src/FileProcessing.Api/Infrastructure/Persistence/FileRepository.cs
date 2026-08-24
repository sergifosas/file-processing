using FileProcessing.Api.Application.Services;
using FileProcessing.Api.Domain.Files;
using Microsoft.EntityFrameworkCore;

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

    public async Task<StoredFile?> GetAsync(string storedName)
    {
        return await _context.Files
            .FirstOrDefaultAsync(x => x.StoredName == storedName);
    }
}