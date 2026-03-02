using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Comjustinspicer.CMS.Data.DbContexts;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;

namespace Comjustinspicer.Tests;

[TestFixture]
public class ContentServiceParentChildTests
{
    private DbContextOptions<ContentBlockContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ContentBlockContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Test]
    public async Task GetChildrenAsync_WithMatchingParentMasterId_ReturnsOnlyLatestVersionChildren()
    {
        var options = CreateOptions();
        var parentId = Guid.NewGuid();

        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);

            var child = await svc.CreateAsync(new ContentBlockDTO
            {
                Title = "Child",
                Content = "child content",
                ParentMasterId = parentId
            });
        }

        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            var children = await svc.GetChildrenAsync(parentId);

            Assert.That(children.Count, Is.EqualTo(1));
            Assert.That(children[0].ParentMasterId, Is.EqualTo(parentId));
        }
    }

    [Test]
    public async Task GetChildrenAsync_NoChildren_ReturnsEmptyList()
    {
        var options = CreateOptions();

        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            await svc.CreateAsync(new ContentBlockDTO { Title = "Root", Content = "root" });
        }

        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            var children = await svc.GetChildrenAsync(Guid.NewGuid());

            Assert.That(children, Is.Empty);
        }
    }

    [Test]
    public async Task GetChildrenAsync_MultipleVersionsOfChild_ReturnsOnlyLatestVersion()
    {
        var options = CreateOptions();
        var parentId = Guid.NewGuid();
        ContentBlockDTO child;

        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            child = await svc.CreateAsync(new ContentBlockDTO
            {
                Title = "Child v1",
                Content = "v1",
                ParentMasterId = parentId
            });
        }

        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            child.Content = "v2";
            await svc.UpdateAsync(child);
        }

        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            var children = await svc.GetChildrenAsync(parentId);

            Assert.That(children.Count, Is.EqualTo(1));
            Assert.That(children[0].Content, Is.EqualTo("v2"));
            Assert.That(children[0].Version, Is.EqualTo(1));
        }
    }

    [Test]
    public async Task GetRootsAsync_ReturnsOnlyItemsWithNullParentMasterId()
    {
        var options = CreateOptions();
        var parentId = Guid.NewGuid();

        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            await svc.CreateAsync(new ContentBlockDTO { Title = "Root A", Content = "root a" });
            await svc.CreateAsync(new ContentBlockDTO { Title = "Root B", Content = "root b" });
            await svc.CreateAsync(new ContentBlockDTO
            {
                Title = "Child",
                Content = "child",
                ParentMasterId = parentId
            });
        }

        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            var roots = await svc.GetRootsAsync();

            Assert.That(roots.Count, Is.EqualTo(2));
            Assert.That(roots.All(r => r.ParentMasterId == null), Is.True);
        }
    }

    [Test]
    public async Task UpdateAsync_PreservesParentMasterId_AcrossVersions()
    {
        var options = CreateOptions();
        var parentId = Guid.NewGuid();
        ContentBlockDTO child;

        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            child = await svc.CreateAsync(new ContentBlockDTO
            {
                Title = "Child",
                Content = "original",
                ParentMasterId = parentId
            });
        }

        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            child.Content = "updated";
            await svc.UpdateAsync(child);
        }

        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            var updated = await svc.GetByMasterIdAsync(child.MasterId);

            Assert.That(updated, Is.Not.Null);
            Assert.That(updated!.ParentMasterId, Is.EqualTo(parentId));
            Assert.That(updated.Version, Is.EqualTo(1));
        }
    }

    [Test]
    public async Task CreateAsync_SetsParentMasterId_WhenProvided()
    {
        var options = CreateOptions();
        var parentId = Guid.NewGuid();

        using (var ctx = new ContentBlockContext(options))
        {
            IContentService<ContentBlockDTO> svc = new ContentService<ContentBlockDTO>(ctx);
            var child = await svc.CreateAsync(new ContentBlockDTO
            {
                Title = "Child",
                Content = "content",
                ParentMasterId = parentId
            });

            Assert.That(child.ParentMasterId, Is.EqualTo(parentId));
            Assert.That(child.Id, Is.Not.EqualTo(Guid.Empty));
        }
    }
}
