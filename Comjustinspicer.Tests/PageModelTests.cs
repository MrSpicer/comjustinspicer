using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Comjustinspicer.CMS.Data;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;
using Comjustinspicer.CMS.Models.Page;

namespace Comjustinspicer.Tests;

[TestFixture]
public class PageModelTests
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

    private static PageDTO CreateDto(Guid? id = null) => new PageDTO
    {
        Id = id ?? Guid.NewGuid(),
        Title = "Test Page",
        Route = "/test",
        ControllerName = "TestController",
        ConfigurationJson = "{}",
        IsPublished = true,
        CreationDate = DateTime.UtcNow.AddMinutes(-10),
        ModificationDate = DateTime.UtcNow.AddMinutes(-5)
    };

    private static PageUpsertViewModel CreateViewModel(Guid? id = null) => new PageUpsertViewModel
    {
        Id = id,
        Title = "Test Page",
        Route = "/test",
        ControllerName = "TestController",
        ConfigurationJson = "{}"
    };

    [Test]
    public async Task GetPageIndexAsync_ReturnsIndexWithTree()
    {
        var pages = new List<PageDTO>
        {
            CreateDto(),
            new PageDTO { Id = Guid.NewGuid(), Title = "Child", Route = "/test/child", ControllerName = "ChildController", IsPublished = false }
        };
        var svc = new Mock<IPageService>();
        svc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(pages);
        var model = new PageModel(svc.Object, _mapper);

        var result = await model.GetPageIndexAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Pages, Has.Count.EqualTo(1));
        Assert.That(result.Pages[0].Children, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetPageUpsertAsync_NullId_ReturnsNewViewModel()
    {
        var svc = new Mock<IPageService>();
        var model = new PageModel(svc.Object, _mapper);

        var result = await model.GetPageUpsertAsync(null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.Null);
    }

    [Test]
    public async Task GetPageUpsertAsync_Found_ReturnsViewModel()
    {
        var dto = CreateDto();
        var svc = new Mock<IPageService>();
        svc.Setup(s => s.GetByIdAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var model = new PageModel(svc.Object, _mapper);

        var result = await model.GetPageUpsertAsync(dto.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(dto.Id));
        Assert.That(result.Route, Is.EqualTo("/test"));
        Assert.That(result.ControllerName, Is.EqualTo("TestController"));
    }

    [Test]
    public async Task GetPageUpsertAsync_NotFound_ReturnsNull()
    {
        var svc = new Mock<IPageService>();
        svc.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as PageDTO);
        var model = new PageModel(svc.Object, _mapper);

        var result = await model.GetPageUpsertAsync(Guid.NewGuid());

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task SavePageUpsertAsync_Create_Success()
    {
        var svc = new Mock<IPageService>();
        svc.Setup(s => s.CreateAsync(It.IsAny<PageDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PageDTO());
        var model = new PageModel(svc.Object, _mapper);
        var vm = CreateViewModel();

        var (success, err) = await model.SavePageUpsertAsync(vm);

        Assert.That(success, Is.True);
        Assert.That(err, Is.Null);
        svc.Verify(s => s.CreateAsync(It.IsAny<PageDTO>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SavePageUpsertAsync_Update_Success()
    {
        var svc = new Mock<IPageService>();
        svc.Setup(s => s.UpdateAsync(It.IsAny<PageDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var model = new PageModel(svc.Object, _mapper);
        var vm = CreateViewModel(Guid.NewGuid());

        var (success, err) = await model.SavePageUpsertAsync(vm);

        Assert.That(success, Is.True);
        Assert.That(err, Is.Null);
        svc.Verify(s => s.UpdateAsync(It.IsAny<PageDTO>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SavePageUpsertAsync_UpdateFails_ReturnsError()
    {
        var svc = new Mock<IPageService>();
        svc.Setup(s => s.UpdateAsync(It.IsAny<PageDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var model = new PageModel(svc.Object, _mapper);
        var vm = CreateViewModel(Guid.NewGuid());

        var (success, err) = await model.SavePageUpsertAsync(vm);

        Assert.That(success, Is.False);
        Assert.That(err, Is.Not.Null);
    }

    [Test]
    public void SavePageUpsertAsync_Null_Throws()
    {
        var svc = new Mock<IPageService>();
        var model = new PageModel(svc.Object, _mapper);

        Assert.ThrowsAsync<ArgumentNullException>(async () => await model.SavePageUpsertAsync(null!));
    }

    [Test]
    public async Task DeletePageAsync_Delegates()
    {
        var svc = new Mock<IPageService>();
        svc.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var model = new PageModel(svc.Object, _mapper);

        var ok = await model.DeletePageAsync(Guid.NewGuid());

        Assert.That(ok, Is.True);
    }

    [Test]
    public async Task IsRouteAvailableAsync_Delegates()
    {
        var svc = new Mock<IPageService>();
        svc.Setup(s => s.IsRouteAvailableAsync("/test", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var model = new PageModel(svc.Object, _mapper);

        var available = await model.IsRouteAvailableAsync("/test");

        Assert.That(available, Is.True);
    }
}
