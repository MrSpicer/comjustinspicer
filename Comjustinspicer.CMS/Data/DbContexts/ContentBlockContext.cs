using Microsoft.EntityFrameworkCore;
using Comjustinspicer.CMS.Data.ContentBlock.Models;

namespace Comjustinspicer.CMS.Data.DbContexts;

public class ContentBlockContext : DbContext
{
    public ContentBlockContext(DbContextOptions<ContentBlockContext> options) : base(options) { }

    public DbSet<ContentBlockDTO> ContentBlocks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ContentBlockDTO>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(10000);
            entity.ToTable("ContentBlocks");
        });
    }
}