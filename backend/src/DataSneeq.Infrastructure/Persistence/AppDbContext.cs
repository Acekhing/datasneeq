using DataSneeq.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataSneeq.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MappingTemplate> MappingTemplates => Set<MappingTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MappingTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TargetTable).IsRequired().HasMaxLength(200);
            entity.Property(e => e.MappingsJson).IsRequired();
            entity.Property(e => e.LookupRulesJson).IsRequired();
            entity.Property(e => e.PrimaryKeyGenerationStrategy)
            .HasDefaultValue(Domain.Enums.PrimaryKeyGenerationStrategy.Uuid)
            .HasSentinel((Domain.Enums.PrimaryKeyGenerationStrategy)(-1)); // Unused value so EF uses DB default when appropriate
        });
    }
}
