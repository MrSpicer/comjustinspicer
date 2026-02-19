using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Comjustinspicer.CMS.Data.DbContexts;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;

namespace Comjustinspicer.Tests;

[TestFixture]
public class PostServiceTests
{
    private DbContextOptions<ArticleContext> CreateNewContextOptions()
    {
        // Each test gets its own in-memory database
        return new DbContextOptionsBuilder<ArticleContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Test]
    public async Task Create_Read_Update_Delete_Flow()
    {
        var options = CreateNewContextOptions();

        // Create
        ArticleDTO created;
        using (var ctx = new ArticleContext(options))
        {
            IContentService<ArticleDTO> svc = new ContentService<ArticleDTO>(ctx);
            var post = new ArticleDTO
            {
                Title = "Test Title",
                Body = "Test Body",
                AuthorName = "Tester",
                PublicationDate = DateTime.UtcNow
            };

            created = await svc.CreateAsync(post);
            Assert.That(created.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(created.Title, Is.EqualTo("Test Title"));
        }

        // Read All / ById
        using (var ctx = new ArticleContext(options))
        {
            IContentService<ArticleDTO> svc = new ContentService<ArticleDTO>(ctx);
            var all = await svc.GetAllAsync();
            Assert.That(all.Count, Is.EqualTo(1));

            var byId = await svc.GetByIdAsync(created.Id);
            Assert.That(byId, Is.Not.Null);
            Assert.That(byId!.Title, Is.EqualTo("Test Title"));
        }

        // Update — inserts a new version row; fetch latest via GetAllAsync
        ArticleDTO updated;
        using (var ctx = new ArticleContext(options))
        {
            IContentService<ArticleDTO> svc = new ContentService<ArticleDTO>(ctx);
            created = created with { Title = "Updated Title" };
            var ok = await svc.UpdateAsync(created);
            Assert.That(ok, Is.True);
        }

        using (var ctx = new ArticleContext(options))
        {
            IContentService<ArticleDTO> svc = new ContentService<ArticleDTO>(ctx);
            var all = await svc.GetAllAsync();
            Assert.That(all.Count, Is.EqualTo(1));
            updated = all[0];
            Assert.That(updated.Title, Is.EqualTo("Updated Title"));
            Assert.That(updated.Version, Is.EqualTo(1));
        }

        // Delete by the latest version's Id
        using (var ctx = new ArticleContext(options))
        {
            IContentService<ArticleDTO> svc = new ContentService<ArticleDTO>(ctx);
            var ok = await svc.DeleteAsync(updated.Id, false, true);
            Assert.That(ok, Is.True);
        }

        using (var ctx = new ArticleContext(options))
        {
            IContentService<ArticleDTO> svc = new ContentService<ArticleDTO>(ctx);
            var all = await svc.GetAllAsync();
            Assert.That(all.Count, Is.EqualTo(0));
        }
    }

    [Test]
    public async Task Update_Nonexistent_ReturnsFalse()
    {
        var options = CreateNewContextOptions();

        using (var ctx = new ArticleContext(options))
        {
            IContentService<ArticleDTO> svc = new ContentService<ArticleDTO>(ctx);
            var post = new ArticleDTO { Id = Guid.NewGuid(), Title = "x" };
            var ok = await svc.UpdateAsync(post);
            Assert.That(ok, Is.False);
        }
    }

    [Test]
    public async Task Delete_Nonexistent_ReturnsFalse()
    {
        var options = CreateNewContextOptions();

        using (var ctx = new ArticleContext(options))
        {
            IContentService<ArticleDTO> svc = new ContentService<ArticleDTO>(ctx);
            var ok = await svc.DeleteAsync(Guid.NewGuid());
            Assert.That(ok, Is.False);
        }
    }

    [Test]
    public async Task DeleteAsync_DefaultArgs_DeletesOnlySingleRecord()
    {
        var options = CreateNewContextOptions();

        // Create and update to produce two version rows
        ArticleDTO created;
        using (var ctx = new ArticleContext(options))
        {
            IContentService<ArticleDTO> svc = new ContentService<ArticleDTO>(ctx);
            created = await svc.CreateAsync(new ArticleDTO { Title = "v1", PublicationDate = DateTime.UtcNow });
        }

        ArticleDTO latestVersion;
        using (var ctx = new ArticleContext(options))
        {
            IContentService<ArticleDTO> svc = new ContentService<ArticleDTO>(ctx);
            created = created with { Title = "v2" };
            await svc.UpdateAsync(created);
            var all = await svc.GetAllAsync();
            latestVersion = all[0];
        }

        // Delete only the latest version (deleteHistory=false)
        using (var ctx = new ArticleContext(options))
        {
            IContentService<ArticleDTO> svc = new ContentService<ArticleDTO>(ctx);
            var ok = await svc.DeleteAsync(latestVersion.Id, false, false);
            Assert.That(ok, Is.True);
        }

        // Old version (v1) should now be the latest in GetAllAsync
        using (var ctx = new ArticleContext(options))
        {
            IContentService<ArticleDTO> svc = new ContentService<ArticleDTO>(ctx);
            var all = await svc.GetAllAsync();
            Assert.That(all.Count, Is.EqualTo(1));
            Assert.That(all[0].Title, Is.EqualTo("v1"));
        }
    }

    [Test]
    public async Task DeleteAsync_WithHistory_DeletesAllVersions()
    {
        var options = CreateNewContextOptions();

        ArticleDTO created;
        using (var ctx = new ArticleContext(options))
        {
            IContentService<ArticleDTO> svc = new ContentService<ArticleDTO>(ctx);
            created = await svc.CreateAsync(new ArticleDTO { Title = "v1", PublicationDate = DateTime.UtcNow });
        }

        using (var ctx = new ArticleContext(options))
        {
            IContentService<ArticleDTO> svc = new ContentService<ArticleDTO>(ctx);
            created = created with { Title = "v2" };
            await svc.UpdateAsync(created);
        }

        // Delete all versions
        using (var ctx = new ArticleContext(options))
        {
            IContentService<ArticleDTO> svc = new ContentService<ArticleDTO>(ctx);
            var ok = await svc.DeleteAsync(created.Id, false, true);
            Assert.That(ok, Is.True);
        }

        using (var ctx = new ArticleContext(options))
        {
            IContentService<ArticleDTO> svc = new ContentService<ArticleDTO>(ctx);
            var all = await svc.GetAllAsync();
            Assert.That(all.Count, Is.EqualTo(0));
        }
    }
}
