using Files.Models;
using Microsoft.EntityFrameworkCore;
namespace Files.Repositories;

public class FilesDbContext : DbContext   
{
    public FilesDbContext(DbContextOptions<FilesDbContext> options) : base(options)
    {
    }
    public DbSet<Chunk> Chunks => Set<Chunk>();
    public DbSet<Models.File> Files => Set<Models.File>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Chunk>()
            .HasKey(chunk => chunk.Id);
        builder.Entity<Models.File>()
            .HasKey(file => file.Id);
        /* builder.Entity<Models.File>(); */
        //We create a compounded key elements with same Number and FileId musnt exists  
        builder.Entity<Chunk>()
        .HasIndex(e => new { e.FileId, e.Number })
        .IsUnique();
/*         builder.Entity<Chunk>()
            .HasKey(e => new {e.FileId, e.Number});
        builder.Entity<Chunk>()
            .Property(e => e.FileId)
            .IsRequired();
        builder.Entity<Chunk>()
            .Property(e => e.Number)
            .IsRequired(); */
    
    }
}