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
        PostDTO created;
        using (var ctx = new ArticleContext(options))
        {
            IContentService<PostDTO> svc = new ContentService<PostDTO>(ctx);
            var post = new PostDTO
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
            IContentService<PostDTO> svc = new ContentService<PostDTO>(ctx);
            var all = await svc.GetAllAsync();
            Assert.That(all.Count, Is.EqualTo(1));

            var byId = await svc.GetByIdAsync(created.Id);
            Assert.That(byId, Is.Not.Null);
            Assert.That(byId!.Title, Is.EqualTo("Test Title"));
        }

        // Update
        using (var ctx = new ArticleContext(options))
        {
            IContentService<PostDTO> svc = new ContentService<PostDTO>(ctx);
            created.Title = "Updated Title";
            var ok = await svc.UpdateAsync(created);
            Assert.That(ok, Is.True);
        }

        using (var ctx = new ArticleContext(options))
        {
            IContentService<PostDTO> svc = new ContentService<PostDTO>(ctx);
            var byId = await svc.GetByIdAsync(created.Id);
            Assert.That(byId, Is.Not.Null);
            Assert.That(byId!.Title, Is.EqualTo("Updated Title"));
        }

        // Delete
        using (var ctx = new ArticleContext(options))
        {
            IContentService<PostDTO> svc = new ContentService<PostDTO>(ctx);
            var ok = await svc.DeleteAsync(created.Id);
            Assert.That(ok, Is.True);
        }

        using (var ctx = new ArticleContext(options))
        {
            IContentService<PostDTO> svc = new ContentService<PostDTO>(ctx);
            var all = await svc.GetAllAsync();
            Assert.That(all.Count, Is.EqualTo(0));

            var byId = await svc.GetByIdAsync(created.Id);
            Assert.That(byId, Is.Null);
        }
    }

    [Test]
    public async Task Update_Nonexistent_ReturnsFalse()
    {
        var options = CreateNewContextOptions();

        using (var ctx = new ArticleContext(options))
        {
            IContentService<PostDTO> svc = new ContentService<PostDTO>(ctx);
            var post = new PostDTO { Id = Guid.NewGuid(), Title = "x" };
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
            IContentService<PostDTO> svc = new ContentService<PostDTO>(ctx);
            var ok = await svc.DeleteAsync(Guid.NewGuid());
            Assert.That(ok, Is.False);
        }
    }
}
