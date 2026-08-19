using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence;

/// <summary>
/// The single EF Core DbContext for the application. Grows one DbSet
/// per aggregate as later phases introduce Photo, Album, Project, etc.
/// (spec section 6). Entities never cross the API boundary directly -
/// DTOs handle that (spec principle #9) - so this stays purely an
/// Infrastructure concern.
/// </summary>
public class PortfolioDbContext : DbContext
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options)
        : base(options)
    {
    }

    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SiteSettings>(entity =>
        {
            entity.Property(e => e.SiteTitle).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SiteDescription).HasMaxLength(1000);
        });
    }
}
