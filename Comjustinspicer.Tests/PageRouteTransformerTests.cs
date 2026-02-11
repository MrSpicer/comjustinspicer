using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;
using Comjustinspicer.CMS.Pages;
using Comjustinspicer.CMS.Routing;

namespace Comjustinspicer.Tests;

[TestFixture]
public class PageRouteTransformerTests
{
    private Mock<IPageService> _pageService;
    private Mock<IPageControllerRegistry> _registry;
    private PageRouteTransformer _transformer;

    [SetUp]
    public void Setup()
    {
        _pageService = new Mock<IPageService>();
        _registry = new Mock<IPageControllerRegistry>();
        _transformer = new PageRouteTransformer(_pageService.Object, _registry.Object);
    }

    private static PageDTO CreatePage(string route, string controllerName = "TestPage") => new PageDTO
    {
        Id = Guid.NewGuid(),
        Route = route,
        ControllerName = controllerName,
        Title = "Test"
    };

    private static PageControllerInfo CreateControllerInfo(string name = "TestPage") => new PageControllerInfo
    {
        Name = name,
        DisplayName = "Test Page"
    };

    private HttpContext CreateHttpContext(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        return context;
    }

    [Test]
    public async Task TransformAsync_ExactMatch_ReturnsRouteValues()
    {
        var page = CreatePage("/blog");
        var info = CreateControllerInfo();

        _pageService.Setup(s => s.GetByRouteAsync("/blog", It.IsAny<CancellationToken>())).ReturnsAsync(page);
        _registry.Setup(r => r.GetByName("TestPage")).Returns(info);

        var context = CreateHttpContext("/blog");
        var result = await _transformer.TransformAsync(context, new RouteValueDictionary());

        Assert.That(result, Is.Not.Null);
        Assert.That(result["controller"], Is.EqualTo("TestPage"));
        Assert.That(context.Items.ContainsKey("CMS:SubRoute"), Is.False);
    }

    [Test]
    public async Task TransformAsync_SubRouteMatch_SetsSubRouteItem()
    {
        var page = CreatePage("/blog");
        var info = CreateControllerInfo();

        _pageService.Setup(s => s.GetByRouteAsync("/blog/my-article", It.IsAny<CancellationToken>())).ReturnsAsync(null as PageDTO);
        _pageService.Setup(s => s.GetByRouteAsync("/blog", It.IsAny<CancellationToken>())).ReturnsAsync(page);
        _registry.Setup(r => r.GetByName("TestPage")).Returns(info);

        var context = CreateHttpContext("/blog/my-article");
        var result = await _transformer.TransformAsync(context, new RouteValueDictionary());

        Assert.That(result, Is.Not.Null);
        Assert.That(result["controller"], Is.EqualTo("TestPage"));
        Assert.That(context.Items["CMS:SubRoute"], Is.EqualTo("my-article"));
    }

    [Test]
    public async Task TransformAsync_NoMatch_ReturnsNull()
    {
        _pageService.Setup(s => s.GetByRouteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(null as PageDTO);

        var context = CreateHttpContext("/nonexistent");
        var result = await _transformer.TransformAsync(context, new RouteValueDictionary());

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task TransformAsync_DeepSubRoute_MatchesParent()
    {
        var page = CreatePage("/blog");
        var info = CreateControllerInfo();

        _pageService.Setup(s => s.GetByRouteAsync("/blog/category/my-article", It.IsAny<CancellationToken>())).ReturnsAsync(null as PageDTO);
        _pageService.Setup(s => s.GetByRouteAsync("/blog/category", It.IsAny<CancellationToken>())).ReturnsAsync(null as PageDTO);
        _pageService.Setup(s => s.GetByRouteAsync("/blog", It.IsAny<CancellationToken>())).ReturnsAsync(page);
        _registry.Setup(r => r.GetByName("TestPage")).Returns(info);

        var context = CreateHttpContext("/blog/category/my-article");
        var result = await _transformer.TransformAsync(context, new RouteValueDictionary());

        Assert.That(result, Is.Not.Null);
        Assert.That(context.Items["CMS:SubRoute"], Is.EqualTo("category/my-article"));
    }

    [Test]
    public async Task TransformAsync_RootPageSubRoute_MatchesRootAndSetsSubRoute()
    {
        var page = CreatePage("/");
        var info = CreateControllerInfo();

        _pageService.Setup(s => s.GetByRouteAsync("/second-test", It.IsAny<CancellationToken>())).ReturnsAsync(null as PageDTO);
        _pageService.Setup(s => s.GetByRouteAsync("/", It.IsAny<CancellationToken>())).ReturnsAsync(page);
        _registry.Setup(r => r.GetByName("TestPage")).Returns(info);

        var context = CreateHttpContext("/second-test");
        var result = await _transformer.TransformAsync(context, new RouteValueDictionary());

        Assert.That(result, Is.Not.Null);
        Assert.That(result["controller"], Is.EqualTo("TestPage"));
        Assert.That(context.Items["CMS:SubRoute"], Is.EqualTo("second-test"));
    }
}
