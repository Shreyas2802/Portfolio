using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Identity;

namespace Portfolio.Infrastructure.Persistence;

/// <summary>
/// The single EF Core DbContext for the application. Grows one DbSet
/// per aggregate as later phases introduce Photo, Album, Project, etc.
/// (spec section 6). Entities never cross the API boundary directly -
/// DTOs handle that (spec principle #9) - so this stays purely an
/// Infrastructure concern.
///
/// Inherits IdentityDbContext&lt;ApplicationUser&gt; as of Phase 6 -
/// this adds ASP.NET Core Identity's own tables (AspNetUsers,
/// AspNetRoles, etc.) alongside everything already here. A new
/// migration is needed to create them; existing tables are untouched.
/// </summary>
public class PortfolioDbContext : IdentityDbContext<ApplicationUser>
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options)
        : base(options)
    {
    }

    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SiteSettings>(entity =>
        {
            entity.Property(e => e.SiteTitle).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SiteDescription).HasMaxLength(1000);
        });

        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(320).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(4000).IsRequired();
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<Album>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Albums)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Photo>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Caption).HasMaxLength(1000);
            entity.Property(e => e.ThumbnailPath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.MediumPath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.LargePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.CameraMake).HasMaxLength(100);
            entity.Property(e => e.CameraModel).HasMaxLength(100);
            entity.Property(e => e.LensModel).HasMaxLength(200);
            entity.Property(e => e.Aperture).HasColumnType("decimal(4,1)");
            entity.Property(e => e.FocalLengthMm).HasColumnType("decimal(6,1)");
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasOne(e => e.Album)
                .WithMany(a => a.Photos)
                .HasForeignKey(e => e.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Tags)
                .WithMany(t => t.Photos)
                .UsingEntity(j => j.ToTable("PhotoTags"));
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Tagline).HasMaxLength(500);
            entity.Property(e => e.Role).HasMaxLength(300);
            entity.Property(e => e.Status).HasMaxLength(300);
            entity.Property(e => e.Overview).HasMaxLength(4000);
            entity.Property(e => e.ContributionsText).HasMaxLength(4000);
            entity.Property(e => e.Technologies).HasMaxLength(500);
            entity.Property(e => e.GithubUrl).HasMaxLength(500);
            entity.Property(e => e.LiveUrl).HasMaxLength(500);
            entity.Property(e => e.CoverImageUrl).HasMaxLength(500);
            entity.HasIndex(e => e.Slug).IsUnique();
        });
    }
}
