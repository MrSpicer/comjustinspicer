using Microsoft.EntityFrameworkCore;
using comjustinspice.Data.Models.Blog;

namespace comjustinspice.Data
{
	public class BlogContext : DbContext
	{
		private string _dbPath = string.Empty;

		public DbSet<Post> Posts {get; set;}

		public BlogContext(){
			var folder = Environment.SpecialFolder.LocalApplicationData;
			var path = Environment.GetFolderPath(folder);
			_dbPath = System.IO.Path.Join(path, "blogging.db");

			Console.WriteLine($"Database path: {_dbPath}.");
		}

		protected override void OnConfiguring(DbContextOptionsBuilder options){
			options.UseSqlite($"Data Source={_dbPath}");
		}
	}
}