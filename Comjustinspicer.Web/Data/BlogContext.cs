using Microsoft.EntityFrameworkCore;
using comjustinspicer.Data.Blog.Models;

namespace comjustinspicer.Data;

public class BlogContext : DbContext
{
	public BlogContext(DbContextOptions<BlogContext> options)
		: base(options)
	{
	}

	public DbSet<PostDTO> Posts { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<PostDTO>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Title)
				.IsRequired()
				.HasMaxLength(20000);
			entity.ToTable("Posts");
		});
	}
}