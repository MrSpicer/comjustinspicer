using System;
using comjustinspicer.Data.Models.Blog;

namespace comjustinspicer.Models.Blog
{
	public class PostViewModel
	{
		public string Title {get; private set;}

		public PostViewModel(Post post){
			Title = post.Title ?? String.Empty;	
		}
	}
}