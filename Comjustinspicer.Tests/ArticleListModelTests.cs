using System;
using System.Collections.Generic;
using System.Linq;
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
public class ArticleListModelTests
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

    private static ArticleListDTO CreateArticleList(Guid? id = null, string slug = "test-list")
    {
        var resolvedId = id ?? Guid.NewGuid();
        return new ArticleListDTO
        {
            Id = resolvedId,
            MasterId = resolvedId,
            Title = "Test List",
            Slug = slug,
            PublicationDate = DateTime.UtcNow,
            CreationDate = DateTime.UtcNow.AddMinutes(-10),
            ModificationDate = DateTime.UtcNow.AddMinutes(-5)
        };
    }

    private static ArticleDTO CreatePost(Guid articleListMasterId, Guid? id = null, bool isPublished = true) => new ArticleDTO
    {
        Id = id ?? Guid.NewGuid(),
        Title = "T",
        Body = "B",
        AuthorName = "A",
        ArticleListMasterId = articleListMasterId,
        IsPublished = isPublished,
        PublicationDate = DateTime.UtcNow.AddMinutes(-1),
        CreationDate = DateTime.UtcNow.AddMinutes(-10),
        ModificationDate = DateTime.UtcNow.AddMinutes(-5)
    };

    [Test]
    public async Task GetArticleListIndexAsync_ReturnsAllListsWithCounts()
    {
        var list1 = CreateArticleList();
        var list2 = CreateArticleList(slug: "list-2");
        var post1 = CreatePost(list1.MasterId);
        var post2 = CreatePost(list1.MasterId);
        var post3 = CreatePost(list2.MasterId);

        var listSvc = new Mock<IContentService<ArticleListDTO>>();
        listSvc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ArticleListDTO> { list1, list2 });

        var postSvc = new Mock<IContentService<ArticleDTO>>();
        postSvc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ArticleDTO> { post1, post2, post3 });

        var model = new ArticleListModel(listSvc.Object, postSvc.Object, _mapper);
        var vm = await model.GetArticleListIndexAsync();

        Assert.That(vm.ArticleLists, Has.Count.EqualTo(2));
        Assert.That(vm.ArticleLists.First(l => l.Id == list1.Id).ArticleCount, Is.EqualTo(2));
        Assert.That(vm.ArticleLists.First(l => l.Id == list2.Id).ArticleCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetArticlesForListAsync_FiltersPostsByArticleListMasterId()
    {
        var list = CreateArticleList();
        var otherListMasterId = Guid.NewGuid();
        var post1 = CreatePost(list.MasterId);
        var post2 = CreatePost(otherListMasterId);

        var listSvc = new Mock<IContentService<ArticleListDTO>>();
        listSvc.Setup(s => s.GetByMasterIdAsync(list.MasterId, It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var postSvc = new Mock<IContentService<ArticleDTO>>();
        postSvc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ArticleDTO> { post1, post2 });

        var model = new ArticleListModel(listSvc.Object, postSvc.Object, _mapper);
        var vm = await model.GetArticlesForListAsync(list.MasterId);

        Assert.That(vm, Is.Not.Null);
        Assert.That(vm!.Articles, Has.Count.EqualTo(1));
        Assert.That(vm.ArticleListId, Is.EqualTo(list.MasterId));
    }

    [Test]
    public async Task GetArticlesForListAsync_ListNotFound_ReturnsNull()
    {
        var listSvc = new Mock<IContentService<ArticleListDTO>>();
        listSvc.Setup(s => s.GetByMasterIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(null as ArticleListDTO);

        var postSvc = new Mock<IContentService<ArticleDTO>>();
        var model = new ArticleListModel(listSvc.Object, postSvc.Object, _mapper);
        var vm = await model.GetArticlesForListAsync(Guid.NewGuid());

        Assert.That(vm, Is.Null);
    }

    [Test]
    public async Task GetArticlesForListAsync_ExcludesUnpublishedArticles()
    {
        var list = CreateArticleList();
        var published = CreatePost(list.MasterId, isPublished: true);
        var unpublished = CreatePost(list.MasterId, isPublished: false);

        var listSvc = new Mock<IContentService<ArticleListDTO>>();
        listSvc.Setup(s => s.GetByMasterIdAsync(list.MasterId, It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var postSvc = new Mock<IContentService<ArticleDTO>>();
        postSvc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ArticleDTO> { published, unpublished });

        var model = new ArticleListModel(listSvc.Object, postSvc.Object, _mapper);
        var vm = await model.GetArticlesForListAsync(list.MasterId);

        Assert.That(vm, Is.Not.Null);
        Assert.That(vm!.Articles, Has.Count.EqualTo(1));
        Assert.That(vm.Articles[0].Id, Is.EqualTo(published.Id));
    }

    [Test]
    public async Task GetArticlesForListBySlugAsync_Found_ReturnsArticles()
    {
        var list = CreateArticleList(slug: "my-list");
        var post = CreatePost(list.MasterId);

        var listSvc = new Mock<IContentService<ArticleListDTO>>();
        listSvc.Setup(s => s.GetBySlugAsync("my-list", It.IsAny<CancellationToken>())).ReturnsAsync(list);
        listSvc.Setup(s => s.GetByMasterIdAsync(list.MasterId, It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var postSvc = new Mock<IContentService<ArticleDTO>>();
        postSvc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ArticleDTO> { post });

        var model = new ArticleListModel(listSvc.Object, postSvc.Object, _mapper);
        var vm = await model.GetArticlesForListBySlugAsync("my-list");

        Assert.That(vm, Is.Not.Null);
        Assert.That(vm!.ArticleListSlug, Is.EqualTo("my-list"));
    }

    [Test]
    public async Task GetArticlesForListBySlugAsync_NotFound_ReturnsNull()
    {
        var listSvc = new Mock<IContentService<ArticleListDTO>>();
        listSvc.Setup(s => s.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(null as ArticleListDTO);

        var postSvc = new Mock<IContentService<ArticleDTO>>();
        var model = new ArticleListModel(listSvc.Object, postSvc.Object, _mapper);
        var vm = await model.GetArticlesForListBySlugAsync("nonexistent");

        Assert.That(vm, Is.Null);
    }

    [Test]
    public async Task SaveArticleListUpsertAsync_Create_Path()
    {
        var listSvc = new Mock<IContentService<ArticleListDTO>>();
        listSvc.Setup(s => s.CreateAsync(It.IsAny<ArticleListDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArticleListDTO l, CancellationToken _) => l);

        var postSvc = new Mock<IContentService<ArticleDTO>>();
        var model = new ArticleListModel(listSvc.Object, postSvc.Object, _mapper);

        var vm = new ArticleListUpsertViewModel { Title = "New List", PublicationDate = DateTime.UtcNow };
        var (success, err) = await model.SaveArticleListUpsertAsync(vm);

        Assert.That(success, Is.True);
        Assert.That(err, Is.Null);
        listSvc.Verify(s => s.CreateAsync(It.IsAny<ArticleListDTO>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteArticleListAsync_CallsService()
    {
        var list = CreateArticleList();

        var listSvc = new Mock<IContentService<ArticleListDTO>>();
        listSvc.Setup(s => s.GetByIdAsync(list.Id, It.IsAny<CancellationToken>())).ReturnsAsync(list);
        listSvc.Setup(s => s.DeleteAsync(list.Id, false, true, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var postSvc = new Mock<IContentService<ArticleDTO>>();
        postSvc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<ArticleDTO>());

        var model = new ArticleListModel(listSvc.Object, postSvc.Object, _mapper);

        var result = await model.DeleteArticleListAsync(list.Id);
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task GetArticleListUpsertAsync_NullId_ReturnsNewModel()
    {
        var listSvc = new Mock<IContentService<ArticleListDTO>>();
        var postSvc = new Mock<IContentService<ArticleDTO>>();
        var model = new ArticleListModel(listSvc.Object, postSvc.Object, _mapper);

        var vm = await model.GetArticleListUpsertAsync(null);
        Assert.That(vm, Is.Not.Null);
        Assert.That(vm!.Id, Is.Null);
    }

    [Test]
    public async Task GetArticleListUpsertAsync_NotFound_ReturnsNull()
    {
        var listSvc = new Mock<IContentService<ArticleListDTO>>();
        listSvc.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(null as ArticleListDTO);

        var postSvc = new Mock<IContentService<ArticleDTO>>();
        var model = new ArticleListModel(listSvc.Object, postSvc.Object, _mapper);

        var vm = await model.GetArticleListUpsertAsync(Guid.NewGuid());
        Assert.That(vm, Is.Null);
    }
}
