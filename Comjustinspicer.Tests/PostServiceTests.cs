using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using comjustinspicer.Data;
using comjustinspicer.Data.Models.Blog;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Comjustinspicer.Tests;

[TestFixture]
public class PostServiceTests
{
    private DbContextOptions<BlogContext> CreateNewContextOptions()
    {
        // Each test gets its own in-memory database
        return new DbContextOptionsBuilder<BlogContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Test]
    public async Task Create_Read_Update_Delete_Flow()
    {
        var options = CreateNewContextOptions();

        // Create
        PostDTO created;
        using (var ctx = new BlogContext(options))
        {
            var svc = new PostService(ctx);
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
        using (var ctx = new BlogContext(options))
        {
            var svc = new PostService(ctx);
            var all = await svc.GetAllAsync();
            Assert.That(all.Count, Is.EqualTo(1));

            var byId = await svc.GetByIdAsync(created.Id);
            Assert.That(byId, Is.Not.Null);
            Assert.That(byId!.Title, Is.EqualTo("Test Title"));
        }

        // Update
        using (var ctx = new BlogContext(options))
        {
            var svc = new PostService(ctx);
            created.Title = "Updated Title";
            var ok = await svc.UpdateAsync(created);
            Assert.That(ok, Is.True);
        }

        using (var ctx = new BlogContext(options))
        {
            var svc = new PostService(ctx);
            var byId = await svc.GetByIdAsync(created.Id);
            Assert.That(byId, Is.Not.Null);
            Assert.That(byId!.Title, Is.EqualTo("Updated Title"));
        }

        // Delete
        using (var ctx = new BlogContext(options))
        {
            var svc = new PostService(ctx);
            var ok = await svc.DeleteAsync(created.Id);
            Assert.That(ok, Is.True);
        }

        using (var ctx = new BlogContext(options))
        {
            var svc = new PostService(ctx);
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

        using (var ctx = new BlogContext(options))
        {
            var svc = new PostService(ctx);
            var post = new PostDTO { Id = Guid.NewGuid(), Title = "x" };
            var ok = await svc.UpdateAsync(post);
            Assert.That(ok, Is.False);
        }
    }

    [Test]
    public async Task Delete_Nonexistent_ReturnsFalse()
    {
        var options = CreateNewContextOptions();

        using (var ctx = new BlogContext(options))
        {
            var svc = new PostService(ctx);
            var ok = await svc.DeleteAsync(Guid.NewGuid());
            Assert.That(ok, Is.False);
        }
    }
}
