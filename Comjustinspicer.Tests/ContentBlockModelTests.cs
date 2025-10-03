using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;
using AutoMapper;
using Comjustinspicer.Models.ContentBlock;
using Comjustinspicer.Data.ContentBlock;
using Comjustinspicer.Data.ContentBlock.Models;
using Comjustinspicer.Data;
using Microsoft.Extensions.Logging;

namespace Comjustinspicer.Tests;

[TestFixture]
public class ContentBlockModelTests
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
	
	private static ContentBlockDTO CreateDto(Guid? id = null) => new ContentBlockDTO
	{
		Id = id ?? Guid.NewGuid(),
		Title = "Title",
		Content = "Content",
		CreationDate = DateTime.UtcNow.AddMinutes(-10),
		ModificationDate = DateTime.UtcNow.AddMinutes(-5)
	};

    [Test]
    public void FromIdAsync_Empty_Throws()
    {
        var svc = new Mock<IContentBlockService>();
        var model = new ContentBlockModel(svc.Object);
        Assert.ThrowsAsync<ArgumentException>(async () => await model.FromIdAsync(Guid.Empty));
    }

    [Test]
    public async Task FromIdAsync_Found_ReturnsDto()
    {
        var dto = CreateDto();
        var svc = new Mock<IContentBlockService>();
        svc.Setup(s => s.GetByIdAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var model = new ContentBlockModel(svc.Object);
        var result = await model.FromIdAsync(dto.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(dto.Id));
    }

    [Test]
    public async Task GetAllAsync_ReturnsList()
    {
        var list = new List<ContentBlockDTO> { CreateDto() };
        var svc = new Mock<IContentBlockService>();
        svc.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);
        var model = new ContentBlockModel(svc.Object);
        var result = await model.GetAllAsync();
        Assert.That(result.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetUpsertModelAsync_NullId_ReturnsEmptyDto()
    {
        var svc = new Mock<IContentBlockService>();
        var model = new ContentBlockModel(svc.Object);
        var dto = await model.GetUpsertModelAsync(null);
        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.Id, Is.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task GetUpsertModelAsync_NotFound_ReturnsEmptyDto()
    {
        var svc = new Mock<IContentBlockService>();
        svc.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentBlockDTO?)null);
        var model = new ContentBlockModel(svc.Object);
        var dto = await model.GetUpsertModelAsync(Guid.NewGuid());
        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.Id, Is.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task GetUpsertModelAsync_Found_ReturnsDto()
    {
        var dto = CreateDto();
        var svc = new Mock<IContentBlockService>();
        svc.Setup(s => s.GetByIdAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var model = new ContentBlockModel(svc.Object);
        var result = await model.GetUpsertModelAsync(dto.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(dto.Id));
    }

    [Test]
    public async Task SaveUpsertAsync_Success()
    {
        var svc = new Mock<IContentBlockService>();
        svc.Setup(s => s.UpsertAsync(It.IsAny<ContentBlockDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var model = new ContentBlockModel(svc.Object);
        var dto = CreateDto(Guid.NewGuid());
        var (success, err) = await model.SaveUpsertAsync(dto);
        Assert.That(success, Is.True);
        Assert.That(err, Is.Null);
    }

    [Test]
    public async Task SaveUpsertAsync_Failure()
    {
        var svc = new Mock<IContentBlockService>();
        svc.Setup(s => s.UpsertAsync(It.IsAny<ContentBlockDTO>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var model = new ContentBlockModel(svc.Object);
        var dto = CreateDto(Guid.NewGuid());
        var (success, err) = await model.SaveUpsertAsync(dto);
        Assert.That(success, Is.False);
        Assert.That(err, Is.Not.Null);
    }

    [Test]
    public void SaveUpsertAsync_Null_Throws()
    {
        var svc = new Mock<IContentBlockService>();
        var model = new ContentBlockModel(svc.Object);
        Assert.ThrowsAsync<ArgumentNullException>(async () => await model.SaveUpsertAsync(null!));
    }

    [Test]
    public async Task DeleteAsync_Delegates()
    {
        var svc = new Mock<IContentBlockService>();
        svc.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var model = new ContentBlockModel(svc.Object);
        var ok = await model.DeleteAsync(Guid.NewGuid());
        Assert.That(ok, Is.True);
    }
}
