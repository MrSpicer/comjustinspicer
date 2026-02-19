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
public class PageServiceTests
{
    private DbContextOptions<PageContext> CreateOptions()
        => new DbContextOptionsBuilder<PageContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static PageDTO MakePage(string route = "/test", string title = "Test Page")
        => new PageDTO
        {
            Title = title,
            Route = route,
            ControllerName = "GenericPage",
            IsPublished = true,
        };

    [Test]
    public async Task CreateAsync_SetsMasterIdEqualsId()
    {
        var options = CreateOptions();

        using var ctx = new PageContext(options);
        var svc = new PageService(ctx);
        var page = MakePage();

        await svc.CreateAsync(page);

        Assert.That(page.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(page.MasterId, Is.EqualTo(page.Id));
        Assert.That(page.Version, Is.EqualTo(0));
    }

    [Test]
    public async Task UpdateAsync_InsertsNewVersionRow()
    {
        var options = CreateOptions();

        PageDTO created;
        using (var ctx = new PageContext(options))
        {
            var svc = new PageService(ctx);
            created = MakePage();
            await svc.CreateAsync(created);
        }

        var originalId = created.Id;
        var masterId = created.MasterId;

        using (var ctx = new PageContext(options))
        {
            var svc = new PageService(ctx);
            created.Title = "Updated Title";
            var ok = await svc.UpdateAsync(created);
            Assert.That(ok, Is.True);
        }

        using (var ctx = new PageContext(options))
        {
            var allRows = await ctx.Pages.AsNoTracking().ToListAsync();
            Assert.That(allRows.Count, Is.EqualTo(2));

            var v0 = allRows.Single(p => p.Id == originalId);
            var v1 = allRows.Single(p => p.Id != originalId);

            Assert.That(v0.Version, Is.EqualTo(0));
            Assert.That(v0.MasterId, Is.EqualTo(masterId));

            Assert.That(v1.Version, Is.EqualTo(1));
            Assert.That(v1.MasterId, Is.EqualTo(masterId));
            Assert.That(v1.Title, Is.EqualTo("Updated Title"));
        }
    }

    [Test]
    public async Task GetAllAsync_ReturnsOnlyLatestVersion()
    {
        var options = CreateOptions();

        PageDTO created;
        using (var ctx = new PageContext(options))
        {
            var svc = new PageService(ctx);
            created = MakePage();
            await svc.CreateAsync(created);
        }

        using (var ctx = new PageContext(options))
        {
            var svc = new PageService(ctx);
            created.Title = "Version 1";
            await svc.UpdateAsync(created);
        }

        using (var ctx = new PageContext(options))
        {
            var svc = new PageService(ctx);
            var all = await svc.GetAllAsync();
            Assert.That(all.Count, Is.EqualTo(1));
            Assert.That(all[0].Version, Is.EqualTo(1));
            Assert.That(all[0].Title, Is.EqualTo("Version 1"));
        }
    }

    [Test]
    public async Task GetByRouteAsync_ReturnsLatestPublishedVersion()
    {
        var options = CreateOptions();

        PageDTO created;
        using (var ctx = new PageContext(options))
        {
            var svc = new PageService(ctx);
            created = MakePage("/blog", "Original");
            await svc.CreateAsync(created);
        }

        using (var ctx = new PageContext(options))
        {
            var svc = new PageService(ctx);
            created.Title = "Revised";
            await svc.UpdateAsync(created);
        }

        using (var ctx = new PageContext(options))
        {
            var svc = new PageService(ctx);
            var result = await svc.GetByRouteAsync("/blog");
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Title, Is.EqualTo("Revised"));
            Assert.That(result.Version, Is.EqualTo(1));
        }
    }

    [Test]
    public async Task IsRouteAvailableAsync_ExcludesByMasterId()
    {
        var options = CreateOptions();

        PageDTO created;
        using (var ctx = new PageContext(options))
        {
            var svc = new PageService(ctx);
            created = MakePage("/exclusive");
            await svc.CreateAsync(created);
        }

        using (var ctx = new PageContext(options))
        {
            var svc = new PageService(ctx);

            // Without exclusion: route is taken
            var withoutExclusion = await svc.IsRouteAvailableAsync("/exclusive");
            Assert.That(withoutExclusion, Is.False);

            // With masterId exclusion: route is available to the same logical page
            var withExclusion = await svc.IsRouteAvailableAsync("/exclusive", created.MasterId);
            Assert.That(withExclusion, Is.True);
        }
    }

    [Test]
    public async Task DeleteAsync_DeletesAllVersions()
    {
        var options = CreateOptions();

        PageDTO created;
        using (var ctx = new PageContext(options))
        {
            var svc = new PageService(ctx);
            created = MakePage("/delete-me");
            await svc.CreateAsync(created);
        }

        using (var ctx = new PageContext(options))
        {
            var svc = new PageService(ctx);
            created.Title = "Version 1";
            await svc.UpdateAsync(created);
        }

        using (var ctx = new PageContext(options))
        {
            var svc = new PageService(ctx);
            // Delete by original row id
            var ok = await svc.DeleteAsync(created.Id);
            Assert.That(ok, Is.True);
        }

        using (var ctx = new PageContext(options))
        {
            var remaining = await ctx.Pages.AsNoTracking().ToListAsync();
            Assert.That(remaining.Count, Is.EqualTo(0));
        }
    }

    [Test]
    public async Task DeleteAsync_Nonexistent_ReturnsFalse()
    {
        var options = CreateOptions();

        using var ctx = new PageContext(options);
        var svc = new PageService(ctx);
        var ok = await svc.DeleteAsync(Guid.NewGuid());
        Assert.That(ok, Is.False);
    }

    [Test]
    public async Task UpdateAsync_Nonexistent_ReturnsFalse()
    {
        var options = CreateOptions();

        using var ctx = new PageContext(options);
        var svc = new PageService(ctx);
        var page = MakePage();
        page.Id = Guid.NewGuid(); // doesn't exist in DB

        var ok = await svc.UpdateAsync(page);
        Assert.That(ok, Is.False);
    }
}
