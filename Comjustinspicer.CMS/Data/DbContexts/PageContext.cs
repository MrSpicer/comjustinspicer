using Microsoft.EntityFrameworkCore;
using Comjustinspicer.CMS.Data.Models;

namespace Comjustinspicer.CMS.Data.DbContexts;

public class PageContext : DbContext
{
    public PageContext(DbContextOptions<PageContext> options) : base(options) { }

    public DbSet<PageDTO> Pages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PageDTO>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Route).IsRequired().HasMaxLength(512);
            entity.HasIndex(e => e.Route);
            entity.Property(e => e.ControllerName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.ConfigurationJson).HasMaxLength(4000);
            entity.ToTable("Pages");

            // Store CustomFields as JSON
            entity.OwnsMany(e => e.CustomFields, cf =>
            {
                cf.ToJson();
            });
        });
    }
}
