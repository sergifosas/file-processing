using FileProcessing.Api.Domain.Files;
using Microsoft.EntityFrameworkCore;

namespace FileProcessing.Api.Infrastructure.Persistence;

public class FileProcessingDbContext : DbContext
{
    public FileProcessingDbContext(
        DbContextOptions<FileProcessingDbContext> options)
        : base(options)
    {
    }

    public DbSet<StoredFile> Files => Set<StoredFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FileProcessingDbContext).Assembly);
    }
}