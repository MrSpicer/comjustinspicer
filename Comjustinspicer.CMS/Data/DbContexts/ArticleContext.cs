using Microsoft.EntityFrameworkCore;
using Comjustinspicer.CMS.Data.Models;

namespace Comjustinspicer.CMS.Data.DbContexts;

public class ArticleContext : DbContext
{
    public ArticleContext(DbContextOptions<ArticleContext> options) : base(options) { }

    public DbSet<ArticleListDTO> ArticleLists { get; set; } = null!;

    public DbSet<PostDTO> Articles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PostDTO>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(20000);
            entity.ToTable("Posts");
            
            // Store CustomFields as JSON
            entity.OwnsMany(e => e.CustomFields, cf =>
            {
                cf.ToJson();
            });
        });
        
        modelBuilder.Entity<ArticleListDTO>(entity =>
        {
            // Store CustomFields as JSON
            entity.OwnsMany(e => e.CustomFields, cf =>
            {
                cf.ToJson();
            });
        });
    }
}