using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class UrlDbContext : DbContext
{
    public UrlDbContext(DbContextOptions<UrlDbContext> options) : base(options) { }

    public DbSet<UrlMapping> UrlMappings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UrlMapping>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.ShortCode).IsUnique();
            b.Property(x => x.ShortCode).HasMaxLength(64).IsRequired();
            b.Property(x => x.LongUrl).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.HitCount).HasDefaultValue(0L);
        });
    }
}
