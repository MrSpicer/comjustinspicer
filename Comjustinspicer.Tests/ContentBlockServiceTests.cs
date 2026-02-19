using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Comjustinspicer.CMS.Data.DbContexts;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;

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

    [Test]
    public async Task Upsert_Create_Read_Update_Delete_Flow()
    {
        var options = CreateNewContextOptions();

        // Create (Upsert with empty Id)
        ContentBlockDTO created;
        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
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
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            var all = await svc.GetAllAsync();
            Assert.That(all.Count, Is.EqualTo(1));

            var byId = await svc.GetByIdAsync(created.Id);
            Assert.That(byId, Is.Not.Null);
            Assert.That(byId!.Title, Is.EqualTo("Test Block"));
        }

        // Update via Upsert (existing Id)
        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            created.Content = "Updated content";
            var ok = await svc.UpsertAsync(created);
            Assert.That(ok, Is.True);
        }

        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            var byId = await svc.GetByIdAsync(created.Id);
            Assert.That(byId, Is.Not.Null);
            Assert.That(byId!.Content, Is.EqualTo("Updated content"));
        }

        // Delete all versions
        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            var ok = await svc.DeleteAsync(created.Id, false, true);
            Assert.That(ok, Is.True);
        }

        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            var all = await svc.GetAllAsync();
            Assert.That(all.Count, Is.EqualTo(0));
        }
    }

    [Test]
    public void Upsert_Null_Throws()
    {
        var options = CreateNewContextOptions();

        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            Assert.ThrowsAsync<ArgumentNullException>(async () => await svc.UpsertAsync(null!));
        }
    }

    [Test]
    public async Task Delete_Nonexistent_ReturnsFalse()
    {
        var options = CreateNewContextOptions();

        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            var ok = await svc.DeleteAsync(Guid.NewGuid());
            Assert.That(ok, Is.False);
        }
    }
}
