using Microsoft.EntityFrameworkCore;
using Comjustinspicer.CMS.Data.Models;

namespace Comjustinspicer.CMS.Data.DbContexts;

public class ContentZoneContext : DbContext
{
    public ContentZoneContext(DbContextOptions<ContentZoneContext> options) : base(options) { }

    public DbSet<ContentZoneDTO> ContentZones { get; set; } = null!;
    public DbSet<ContentZoneItemDTO> ContentZoneItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ContentZoneDTO>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.ToTable("ContentZones");

            entity.HasMany(e => e.Items)
                  .WithOne(i => i.ContentZone)
                  .HasForeignKey(i => i.ContentZoneId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Store CustomFields as JSON
            entity.OwnsMany(e => e.CustomFields, cf =>
            {
                cf.ToJson();
            });
        });

        modelBuilder.Entity<ContentZoneItemDTO>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ComponentName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.ComponentPropertiesJson).HasMaxLength(4000);
            entity.ToTable("ContentZoneItems");

            entity.HasIndex(e => new { e.ContentZoneId, e.Ordinal });

            // Store CustomFields as JSON
            entity.OwnsMany(e => e.CustomFields, cf =>
            {
                cf.ToJson();
            });
        });
    }
}
