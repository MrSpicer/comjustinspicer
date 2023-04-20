using Microsoft.EntityFrameworkCore;
using comjustinspicer.Data.Models.Blog;

namespace comjustinspicer.Data
{
	public class BlogContext : DbContext
	{
		private string _dbPath = string.Empty;

		public DbSet<Post> Posts {get; set;}

		public BlogContext(){
			var folder = Environment.SpecialFolder.LocalApplicationData;
			var path = Environment.GetFolderPath(folder);
			_dbPath = System.IO.Path.Join(path, "blogging.db"); //maybe this should change...

			Console.WriteLine($"Database path: {_dbPath}.");
		}

		protected override void OnConfiguring(DbContextOptionsBuilder options){
			options.UseSqlite($"Data Source={_dbPath}");
		}
	}
}