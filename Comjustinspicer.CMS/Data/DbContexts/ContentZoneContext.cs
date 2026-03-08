using Microsoft.EntityFrameworkCore;
using Comjustinspicer.CMS.Data.Models;

namespace Comjustinspicer.CMS.Data.DbContexts;

public class ContentZoneContext : DbContext
{
    public ContentZoneContext(DbContextOptions<ContentZoneContext> options) : base(options) { }

    public DbSet<ContentZoneDTO> ContentZones { get; set; } = null!;
    public DbSet<ContentZoneItemDTO> ContentZoneItems { get; set; } = null!;
    public DbSet<ContentZoneAssignmentDTO> ContentZoneAssignments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ContentZoneAssignmentDTO>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SlotName).IsRequired().HasMaxLength(256);
            entity.ToTable("ContentZoneAssignments",
                t => t.HasCheckConstraint("CK_ContentZoneAssignments_OneParent",
                    "(\"ParentPageMasterId\" IS NOT NULL AND \"ParentZoneId\" IS NULL) OR " +
                    "(\"ParentPageMasterId\" IS NULL AND \"ParentZoneId\" IS NOT NULL)"));

            entity.HasOne(e => e.ContentZone)
                  .WithMany()
                  .HasForeignKey(e => e.ContentZoneId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentZone)
                  .WithMany()
                  .HasForeignKey(e => e.ParentZoneId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.ParentPageMasterId, e.SlotName })
                  .IsUnique()
                  .HasFilter("\"ParentPageMasterId\" IS NOT NULL")
                  .HasDatabaseName("IX_ContentZoneAssignments_PageSlot");

            entity.HasIndex(e => new { e.ParentZoneId, e.SlotName })
                  .IsUnique()
                  .HasFilter("\"ParentZoneId\" IS NOT NULL")
                  .HasDatabaseName("IX_ContentZoneAssignments_ZoneSlot");
        });

        modelBuilder.Entity<ContentZoneDTO>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.ToTable("ContentZones");

            entity.HasMany(e => e.Items)
                  .WithOne(i => i.ContentZone)
                  .HasForeignKey(i => i.ContentZoneId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.ParentMasterId)
                .HasDatabaseName("IX_ContentZones_ParentMasterId");

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
            entity.HasIndex(e => e.ParentMasterId)
                .HasDatabaseName("IX_ContentZoneItems_ParentMasterId");

            // Store CustomFields as JSON
            entity.OwnsMany(e => e.CustomFields, cf =>
            {
                cf.ToJson();
            });
        });
    }
}
