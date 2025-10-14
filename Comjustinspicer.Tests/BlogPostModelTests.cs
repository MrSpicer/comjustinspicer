using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using System.Threading;
using System;
using Comjustinspicer.Models.Blog;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Comjustinspicer.Tests;

[TestFixture]
public class BlogPostModelTests
{
	private IMapper _mapper;
	private MapperConfiguration _config;

	[SetUp]
	public void Setup()
	{
		// Configure AutoMapper here. 
		// You can add individual profiles or scan for profiles in an assembly.
		//todo: maybe a better way to do the logfactory
		_config = new MapperConfiguration(cfg =>
		{
			// Example: Adding a specific profile
			cfg.AddProfile<MappingProfile>();

			// Example: Scanning an assembly for all profiles
			// cfg.AddMaps(typeof(MyApplicationProfile).Assembly); 
		}, LoggerFactory.Create(builder => builder.AddConsole()));

		_mapper = _config.CreateMapper();
	}

	private static PostDTO CreatePost(Guid? id = null) => new PostDTO
	{
		Id = id ?? Guid.NewGuid(),
		Title = "T",
		Body = "B",
		AuthorName = "A",
		PublicationDate = DateTime.UtcNow.AddMinutes(-1),
		CreationDate = DateTime.UtcNow.AddMinutes(-10),
		ModificationDate = DateTime.UtcNow.AddMinutes(-5)
	};

    [Test]
    public async Task GetPostViewModelAsync_NotFound_ReturnsNull()
    {
        var svc = new Mock<IContentService<PostDTO>>();
        svc.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as PostDTO);
        var model = new BlogPostModel(svc.Object, _mapper);
        var result = await model.GetPostViewModelAsync(Guid.NewGuid());
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetPostViewModelAsync_Found_ReturnsViewModel()
    {
        var post = CreatePost();
        var svc = new Mock<IContentService<PostDTO>>();
        svc.Setup(s => s.GetByIdAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(post);
        var model = new BlogPostModel(svc.Object, _mapper);
        var result = await model.GetPostViewModelAsync(post.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(post.Id));
    }

    [Test]
    public async Task GetUpsertViewModelAsync_NullId_ReturnsEmptyModel()
    {
        var svc = new Mock<IContentService<PostDTO>>();
        var model = new BlogPostModel(svc.Object, _mapper);
        var vm = await model.GetUpsertViewModelAsync(null);
        Assert.That(vm, Is.Not.Null);
        Assert.That(vm!.Id, Is.Null);
    }

    [Test]
    public async Task GetUpsertViewModelAsync_NotFound_ReturnsNull()
    {
        var svc = new Mock<IContentService<PostDTO>>();
        svc.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as PostDTO);
        var model = new BlogPostModel(svc.Object, _mapper);
        var vm = await model.GetUpsertViewModelAsync(Guid.NewGuid());
        Assert.That(vm, Is.Null);
    }

    [Test]
    public async Task GetUpsertViewModelAsync_Found_ReturnsMapped()
    {
        var post = CreatePost();
        var svc = new Mock<IContentService<PostDTO>>();
        svc.Setup(s => s.GetByIdAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(post);
        var model = new BlogPostModel(svc.Object, _mapper);
        var vm = await model.GetUpsertViewModelAsync(post.Id);
        Assert.That(vm, Is.Not.Null);
        Assert.That(vm!.Id, Is.EqualTo(post.Id));
    }

    [Test]
    public async Task SaveUpsertAsync_Create_Path()
    {
    var svc = new Mock<IContentService<PostDTO>>();
        svc.Setup(s => s.CreateAsync(It.IsAny<PostDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PostDTO p, CancellationToken _) => p);
        var model = new BlogPostModel(svc.Object, _mapper);
        var vm = new PostUpsertViewModel { Title = "T", Body = "B", AuthorName = "A", PublicationDate = DateTime.UtcNow };
        var (success, err) = await model.SaveUpsertAsync(vm);
        Assert.That(success, Is.True);
        Assert.That(err, Is.Null);
        svc.Verify(s => s.CreateAsync(It.IsAny<PostDTO>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SaveUpsertAsync_Update_Path_Success()
    {
    var svc = new Mock<IContentService<PostDTO>>();
        svc.Setup(s => s.UpdateAsync(It.IsAny<PostDTO>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var model = new BlogPostModel(svc.Object, _mapper);
        var vm = new PostUpsertViewModel { Id = Guid.NewGuid(), Title = "T", Body = "B", AuthorName = "A", PublicationDate = DateTime.UtcNow };
        var (success, err) = await model.SaveUpsertAsync(vm);
        Assert.That(success, Is.True);
        Assert.That(err, Is.Null);
        svc.Verify(s => s.UpdateAsync(It.IsAny<PostDTO>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SaveUpsertAsync_Update_Path_Failure()
    {
    var svc = new Mock<IContentService<PostDTO>>();
        svc.Setup(s => s.UpdateAsync(It.IsAny<PostDTO>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var model = new BlogPostModel(svc.Object, _mapper);
        var vm = new PostUpsertViewModel { Id = Guid.NewGuid(), Title = "T", Body = "B", AuthorName = "A", PublicationDate = DateTime.UtcNow };
        var (success, err) = await model.SaveUpsertAsync(vm);
        Assert.That(success, Is.False);
        Assert.That(err, Is.Not.Null);
    }

    [Test]
    public void SaveUpsertAsync_NullModel_Throws()
    {
    var svc = new Mock<IContentService<PostDTO>>();
        var model = new BlogPostModel(svc.Object, _mapper);
        Assert.ThrowsAsync<ArgumentNullException>(async () => await model.SaveUpsertAsync(null!));
    }
}
