using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Moq;
using NUnit.Framework;
using Comjustinspicer.CMS.Data;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;
using Comjustinspicer.CMS.Models.Article;

namespace Comjustinspicer.Tests;

[TestFixture]
public class ArticlePostModelTests
{
    private IMapper _mapper;

    [SetUp]
    public void Setup()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        }, LoggerFactory.Create(builder => builder.AddConsole()));

        _mapper = config.CreateMapper();
    }

    private static readonly Guid DefaultListId = Guid.NewGuid();

    private static ArticleDTO CreatePost(Guid? id = null) => new ArticleDTO
    {
        Id = id ?? Guid.NewGuid(),
        Title = "T",
        Body = "B",
        AuthorName = "A",
        ArticleListId = DefaultListId,
        PublicationDate = DateTime.UtcNow.AddMinutes(-1),
        CreationDate = DateTime.UtcNow.AddMinutes(-10),
        ModificationDate = DateTime.UtcNow.AddMinutes(-5)
    };

    [Test]
    public async Task GetPostViewModelAsync_NotFound_ReturnsNull()
    {
        var svc = new Mock<IContentService<ArticleDTO>>();
        svc.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as ArticleDTO);
        var model = new ArticleModel(svc.Object, _mapper);
        var result = await model.GetPostViewModelAsync(Guid.NewGuid());
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetPostViewModelAsync_Found_ReturnsViewModel()
    {
        var post = CreatePost();
        var svc = new Mock<IContentService<ArticleDTO>>();
        svc.Setup(s => s.GetByIdAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(post);
        var model = new ArticleModel(svc.Object, _mapper);
        var result = await model.GetPostViewModelAsync(post.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(post.Id));
    }

    [Test]
    public async Task GetUpsertViewModelAsync_NullId_ReturnsEmptyModel()
    {
        var svc = new Mock<IContentService<ArticleDTO>>();
        var model = new ArticleModel(svc.Object, _mapper);
        var vm = await model.GetUpsertViewModelAsync(null);
        Assert.That(vm, Is.Not.Null);
        Assert.That(vm!.Id, Is.Null);
    }

    [Test]
    public async Task GetUpsertViewModelAsync_NotFound_ReturnsNull()
    {
        var svc = new Mock<IContentService<ArticleDTO>>();
        svc.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as ArticleDTO);
        var model = new ArticleModel(svc.Object, _mapper);
        var vm = await model.GetUpsertViewModelAsync(Guid.NewGuid());
        Assert.That(vm, Is.Null);
    }

    [Test]
    public async Task GetUpsertViewModelAsync_Found_ReturnsMapped()
    {
        var post = CreatePost();
        var svc = new Mock<IContentService<ArticleDTO>>();
        svc.Setup(s => s.GetByIdAsync(post.Id, It.IsAny<CancellationToken>())).ReturnsAsync(post);
        var model = new ArticleModel(svc.Object, _mapper);
        var vm = await model.GetUpsertViewModelAsync(post.Id);
        Assert.That(vm, Is.Not.Null);
        Assert.That(vm!.Id, Is.EqualTo(post.Id));
    }

    [Test]
    public async Task SaveUpsertAsync_Create_Path()
    {
        var svc = new Mock<IContentService<ArticleDTO>>();
        svc.Setup(s => s.CreateAsync(It.IsAny<ArticleDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArticleDTO p, CancellationToken _) => p);
        var model = new ArticleModel(svc.Object, _mapper);
        var vm = new ArticleUpsertViewModel { Title = "T", Body = "B", AuthorName = "A", ArticleListId = DefaultListId, PublicationDate = DateTime.UtcNow };
        var (success, err) = await model.SaveUpsertAsync(vm);
        Assert.That(success, Is.True);
        Assert.That(err, Is.Null);
        svc.Verify(s => s.CreateAsync(It.IsAny<ArticleDTO>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SaveUpsertAsync_Update_Path_Success()
    {
        var svc = new Mock<IContentService<ArticleDTO>>();
        svc.Setup(s => s.UpdateAsync(It.IsAny<ArticleDTO>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var model = new ArticleModel(svc.Object, _mapper);
        var vm = new ArticleUpsertViewModel { Id = Guid.NewGuid(), Title = "T", Body = "B", AuthorName = "A", ArticleListId = DefaultListId, PublicationDate = DateTime.UtcNow };
        var (success, err) = await model.SaveUpsertAsync(vm);
        Assert.That(success, Is.True);
        Assert.That(err, Is.Null);
        svc.Verify(s => s.UpdateAsync(It.IsAny<ArticleDTO>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SaveUpsertAsync_Update_Path_Failure()
    {
        var svc = new Mock<IContentService<ArticleDTO>>();
        svc.Setup(s => s.UpdateAsync(It.IsAny<ArticleDTO>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var model = new ArticleModel(svc.Object, _mapper);
        var vm = new ArticleUpsertViewModel { Id = Guid.NewGuid(), Title = "T", Body = "B", AuthorName = "A", ArticleListId = DefaultListId, PublicationDate = DateTime.UtcNow };
        var (success, err) = await model.SaveUpsertAsync(vm);
        Assert.That(success, Is.False);
        Assert.That(err, Is.Not.Null);
    }

    [Test]
    public void SaveUpsertAsync_NullModel_Throws()
    {
        var svc = new Mock<IContentService<ArticleDTO>>();
        var model = new ArticleModel(svc.Object, _mapper);
        Assert.ThrowsAsync<ArgumentNullException>(async () => await model.SaveUpsertAsync(null!));
    }
}
