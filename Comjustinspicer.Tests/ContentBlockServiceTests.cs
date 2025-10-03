using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Comjustinspicer.Data;
using Comjustinspicer.Data.ContentBlock;
using Comjustinspicer.Data.ContentBlock.Models;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Comjustinspicer.Tests;

[TestFixture]
public class ContentBlockServiceTests
{

    // private IMapper _mapper;
    // private MapperConfiguration _config;

    private DbContextOptions<ContentBlockContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<ContentBlockContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    // [SetUp]
    // public void Setup()
    // {
    //     // Configure AutoMapper here. 
    //     // You can add individual profiles or scan for profiles in an assembly.
    //     //todo: maybe a better way to do the logfactory
    //     _config = new MapperConfiguration(cfg =>
    //     {
    //         // Example: Adding a specific profile
    //         cfg.AddProfile<MappingProfile>();

    //         // Example: Scanning an assembly for all profiles
    //         // cfg.AddMaps(typeof(MyApplicationProfile).Assembly); 
    //     }, LoggerFactory.Create(builder => builder.AddConsole()));

    //     _mapper = _config.CreateMapper();
    // }

    [Test]
    public async Task Upsert_Create_Read_Update_Delete_Flow()
    {
        var options = CreateNewContextOptions();

        // Create (Upsert with empty Id)
        ContentBlockDTO created;
        using (var ctx = new ContentBlockContext(options))
        {
            var svc = new ContentBlockService(ctx);
            var cb = new ContentBlockDTO
            {
                Title = "Test Block",
                Content = "This is content"
            };

            var ok = await svc.UpsertAsync(cb);
            Assert.That(ok, Is.True);
            created = cb;
            Assert.That(created.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(created.CreationDate, Is.Not.EqualTo(default(DateTime)));
            Assert.That(created.ModificationDate, Is.Not.EqualTo(default(DateTime)));
        }

        // Read All / ById
        using (var ctx = new ContentBlockContext(options))
        {
            var svc = new ContentBlockService(ctx);
            var all = await svc.GetAllAsync();
            Assert.That(all.Count, Is.EqualTo(1));

            var byId = await svc.GetByIdAsync(created.Id);
            Assert.That(byId, Is.Not.Null);
            Assert.That(byId!.Title, Is.EqualTo("Test Block"));
        }

        // Update via Upsert (existing Id)
        using (var ctx = new ContentBlockContext(options))
        {
            var svc = new ContentBlockService(ctx);
            created.Content = "Updated content";
            var ok = await svc.UpsertAsync(created);
            Assert.That(ok, Is.True);
        }

        using (var ctx = new ContentBlockContext(options))
        {
            var svc = new ContentBlockService(ctx);
            var byId = await svc.GetByIdAsync(created.Id);
            Assert.That(byId, Is.Not.Null);
            Assert.That(byId!.Content, Is.EqualTo("Updated content"));
        }

        // Delete
        using (var ctx = new ContentBlockContext(options))
        {
            var svc = new ContentBlockService(ctx);
            var ok = await svc.DeleteAsync(created.Id);
            Assert.That(ok, Is.True);
        }

        using (var ctx = new ContentBlockContext(options))
        {
            var svc = new ContentBlockService(ctx);
            var all = await svc.GetAllAsync();
            Assert.That(all.Count, Is.EqualTo(0));

            var byId = await svc.GetByIdAsync(created.Id);
            Assert.That(byId, Is.Null);
        }
    }

    [Test]
    public void Upsert_Null_Throws()
    {
        var options = CreateNewContextOptions();

        using (var ctx = new ContentBlockContext(options))
        {
            var svc = new ContentBlockService(ctx);
            Assert.ThrowsAsync<ArgumentNullException>(async () => await svc.UpsertAsync(null!));
        }
    }

    [Test]
    public async Task Delete_Nonexistent_ReturnsFalse()
    {
        var options = CreateNewContextOptions();

        using (var ctx = new ContentBlockContext(options))
        {
            var svc = new ContentBlockService(ctx);
            var ok = await svc.DeleteAsync(Guid.NewGuid());
            Assert.That(ok, Is.False);
        }
    }
}
