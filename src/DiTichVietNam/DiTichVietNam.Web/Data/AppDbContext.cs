using DiTichVietNam.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DiTichVietNam.Web.Data;

public class AppDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Relic> Relics => Set<Relic>();
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<RelicType> RelicTypes => Set<RelicType>();
    public DbSet<RelicImage> RelicImages => Set<RelicImage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Province>(e =>
        {
            e.Property(p => p.Name).IsRequired().HasMaxLength(100);
            e.Property(p => p.Slug).IsRequired().HasMaxLength(120);
            e.Property(p => p.Region).IsRequired().HasMaxLength(10);
            e.HasIndex(p => p.Name).IsUnique();
            e.HasIndex(p => p.Slug).IsUnique();
        });

        builder.Entity<RelicType>(e =>
        {
            e.Property(t => t.Name).IsRequired().HasMaxLength(100);
            e.Property(t => t.Slug).IsRequired().HasMaxLength(120);
            e.HasIndex(t => t.Name).IsUnique();
            e.HasIndex(t => t.Slug).IsUnique();
        });

        builder.Entity<Relic>(e =>
        {
            e.Property(r => r.Name).IsRequired().HasMaxLength(200);
            e.Property(r => r.Slug).IsRequired().HasMaxLength(220);
            e.Property(r => r.NameNoAccent).IsRequired().HasMaxLength(200);
            e.Property(r => r.Address).IsRequired().HasMaxLength(300);
            e.Property(r => r.Description).IsRequired();
            e.Property(r => r.DescriptionNoAccent).IsRequired().HasDefaultValue(string.Empty);
            e.Property(r => r.SourceUrl).IsRequired();
            e.Property(r => r.ViewCount).HasDefaultValue(0);
            e.HasIndex(r => r.Slug).IsUnique();
            e.HasIndex(r => r.NameNoAccent);

            e.HasOne(r => r.Province)
                .WithMany(p => p.Relics)
                .HasForeignKey(r => r.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.RelicType)
                .WithMany(t => t.Relics)
                .HasForeignKey(r => r.RelicTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RelicImage>(e =>
        {
            e.Property(i => i.ImagePath).IsRequired();
            e.Property(i => i.ImageSource).IsRequired();
            e.Property(i => i.Caption).HasMaxLength(300);
            e.Property(i => i.IsThumbnail).HasDefaultValue(false);

            e.HasOne(i => i.Relic)
                .WithMany(r => r.Images)
                .HasForeignKey(i => i.RelicId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
