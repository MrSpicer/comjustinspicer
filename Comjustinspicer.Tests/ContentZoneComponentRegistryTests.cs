using System;
using System.Linq;
using NUnit.Framework;
using Comjustinspicer.CMS.Attributes;
using Comjustinspicer.CMS.ContentZones;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Comjustinspicer.Tests;

/// <summary>
/// Tests for the ContentZoneComponentRegistry and related attribute system.
/// </summary>
[TestFixture]
public class ContentZoneComponentRegistryTests
{
    #region Test ViewComponents and Configurations

    public class TestConfiguration
    {
        [ContentZoneProperty(Label = "Test Property", EditorType = EditorType.Text, IsRequired = true, Order = 1)]
        [Required]
        public string TestProperty { get; set; } = string.Empty;

        [ContentZoneProperty(Label = "Optional Number", EditorType = EditorType.Number, Min = 0, Max = 100, Order = 2)]
        public int OptionalNumber { get; set; }

        [ContentZoneProperty(Label = "Test Guid", EditorType = EditorType.Guid, EntityType = "TestEntity", Order = 3)]
        public Guid TestGuid { get; set; }
    }

    [ContentZoneComponent(
        DisplayName = "Test Component",
        Description = "A test component for unit testing.",
        Category = "Testing",
        ConfigurationType = typeof(TestConfiguration),
        IconClass = "fa-test",
        Order = 1
    )]
    public class TestViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string testProperty, int optionalNumber = 0)
        {
            return Content($"{testProperty} - {optionalNumber}");
        }
    }

    [ContentZoneComponent(
        DisplayName = "Another Test",
        Description = "Another test component.",
        Category = "Testing",
        Order = 2
    )]
    public class AnotherTestViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke() => Content("Another");
    }

    [ContentZoneComponent(Category = "Other")]
    public class MinimalViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke() => Content("Minimal");
    }

    // Not decorated - should not appear in registry
    public class UnregisteredViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke() => Content("Unregistered");
    }

    #endregion

    private IContentZoneComponentRegistry _registry = null!;

    [SetUp]
    public void Setup()
    {
        // Create registry scanning only this test assembly
        _registry = new ContentZoneComponentRegistry(new[] { typeof(ContentZoneComponentRegistryTests).Assembly });
    }

    #region GetAllComponents Tests

    [Test]
    public void GetAllComponents_ReturnsOnlyDecoratedComponents()
    {
        var components = _registry.GetAllComponents();

        Assert.That(components, Is.Not.Null);
        Assert.That(components.Count, Is.GreaterThanOrEqualTo(3)); // Test, AnotherTest, Minimal

        var names = components.Select(c => c.Name).ToList();
        Assert.That(names, Does.Contain("Test"));
        Assert.That(names, Does.Contain("AnotherTest"));
        Assert.That(names, Does.Contain("Minimal"));
        Assert.That(names, Does.Not.Contain("Unregistered"));
    }

    [Test]
    public void GetAllComponents_SortsByCategoryThenOrderThenName()
    {
        var components = _registry.GetAllComponents();

        // "Other" category should come BEFORE "Testing" alphabetically (O < T)
        var otherIndex = components.ToList().FindIndex(c => c.Category == "Other");
        var testingIndex = components.ToList().FindIndex(c => c.Category == "Testing");

        Assert.That(otherIndex, Is.LessThan(testingIndex));
    }

    #endregion

    #region GetByName Tests

    [Test]
    public void GetByName_ExistingComponent_ReturnsInfo()
    {
        var info = _registry.GetByName("Test");

        Assert.That(info, Is.Not.Null);
        Assert.That(info!.Name, Is.EqualTo("Test"));
        Assert.That(info.DisplayName, Is.EqualTo("Test Component"));
        Assert.That(info.Description, Is.EqualTo("A test component for unit testing."));
        Assert.That(info.Category, Is.EqualTo("Testing"));
        Assert.That(info.IconClass, Is.EqualTo("fa-test"));
        Assert.That(info.Order, Is.EqualTo(1));
    }

    [Test]
    public void GetByName_CaseInsensitive_ReturnsInfo()
    {
        var info1 = _registry.GetByName("TEST");
        var info2 = _registry.GetByName("test");
        var info3 = _registry.GetByName("Test");

        Assert.That(info1, Is.Not.Null);
        Assert.That(info2, Is.Not.Null);
        Assert.That(info3, Is.Not.Null);
        Assert.That(info1!.Name, Is.EqualTo(info2!.Name));
        Assert.That(info2.Name, Is.EqualTo(info3!.Name));
    }

    [Test]
    public void GetByName_NonExistentComponent_ReturnsNull()
    {
        var info = _registry.GetByName("NonExistent");

        Assert.That(info, Is.Null);
    }

    [Test]
    public void GetByName_MinimalAttribute_UsesDefaultDisplayName()
    {
        var info = _registry.GetByName("Minimal");

        Assert.That(info, Is.Not.Null);
        Assert.That(info!.DisplayName, Is.EqualTo("Minimal")); // Auto-generated from class name
    }

    #endregion

    #region GetCategories Tests

    [Test]
    public void GetCategories_ReturnsDistinctSortedCategories()
    {
        var categories = _registry.GetCategories();

        Assert.That(categories, Is.Not.Null);
        Assert.That(categories, Does.Contain("Testing"));
        Assert.That(categories, Does.Contain("Other"));

        // Verify sorted
        var sorted = categories.OrderBy(c => c).ToList();
        Assert.That(categories.SequenceEqual(sorted), Is.True);
    }

    #endregion

    #region GetByCategory Tests

    [Test]
    public void GetByCategory_ExistingCategory_ReturnsComponents()
    {
        var components = _registry.GetByCategory("Testing");

        Assert.That(components, Is.Not.Null);
        Assert.That(components.Count, Is.EqualTo(2)); // Test and AnotherTest
        Assert.That(components.All(c => c.Category == "Testing"), Is.True);
    }

    [Test]
    public void GetByCategory_NonExistentCategory_ReturnsEmpty()
    {
        var components = _registry.GetByCategory("NonExistent");

        Assert.That(components, Is.Not.Null);
        Assert.That(components, Is.Empty);
    }

    [Test]
    public void GetByCategory_CaseInsensitive()
    {
        var components1 = _registry.GetByCategory("TESTING");
        var components2 = _registry.GetByCategory("testing");

        Assert.That(components1.Count, Is.EqualTo(components2.Count));
    }

    #endregion

    #region Configuration Type Tests

    [Test]
    public void GetByName_WithConfiguration_HasProperties()
    {
        var info = _registry.GetByName("Test");

        Assert.That(info, Is.Not.Null);
        Assert.That(info!.ConfigurationType, Is.EqualTo(typeof(TestConfiguration)));
        Assert.That(info.HasConfiguration, Is.True);
        Assert.That(info.Properties, Is.Not.Null);
        Assert.That(info.Properties.Count, Is.EqualTo(3));
    }

    [Test]
    public void GetByName_WithoutConfiguration_HasNoProperties()
    {
        var info = _registry.GetByName("AnotherTest");

        Assert.That(info, Is.Not.Null);
        Assert.That(info!.ConfigurationType, Is.Null);
        Assert.That(info.HasConfiguration, Is.False);
        Assert.That(info.Properties, Is.Empty);
    }

    [Test]
    public void Properties_HaveCorrectMetadata()
    {
        var info = _registry.GetByName("Test");
        var testProperty = info!.Properties.First(p => p.Name == "TestProperty");

        Assert.That(testProperty.Label, Is.EqualTo("Test Property"));
        Assert.That(testProperty.EditorType, Is.EqualTo(EditorType.Text));
        Assert.That(testProperty.IsRequired, Is.True);
        Assert.That(testProperty.Order, Is.EqualTo(1));
    }

    [Test]
    public void Properties_NumericHaveMinMax()
    {
        var info = _registry.GetByName("Test");
        var numberProperty = info!.Properties.First(p => p.Name == "OptionalNumber");

        Assert.That(numberProperty.EditorType, Is.EqualTo(EditorType.Number));
        Assert.That(numberProperty.Min, Is.EqualTo(0));
        Assert.That(numberProperty.Max, Is.EqualTo(100));
    }

    [Test]
    public void Properties_GuidHaveEntityType()
    {
        var info = _registry.GetByName("Test");
        var guidProperty = info!.Properties.First(p => p.Name == "TestGuid");

        Assert.That(guidProperty.EditorType, Is.EqualTo(EditorType.Guid));
        Assert.That(guidProperty.EntityType, Is.EqualTo("TestEntity"));
    }

    [Test]
    public void Properties_AreSortedByOrder()
    {
        var info = _registry.GetByName("Test");
        var orders = info!.Properties.Select(p => p.Order).ToList();

        Assert.That(orders, Is.Ordered);
    }

    #endregion

    #region CreateDefaultConfiguration Tests

    [Test]
    public void CreateDefaultConfiguration_WithConfigurationType_ReturnsInstance()
    {
        var config = _registry.CreateDefaultConfiguration("Test");

        Assert.That(config, Is.Not.Null);
        Assert.That(config, Is.InstanceOf<TestConfiguration>());
    }

    [Test]
    public void CreateDefaultConfiguration_WithoutConfigurationType_ReturnsNull()
    {
        var config = _registry.CreateDefaultConfiguration("AnotherTest");

        Assert.That(config, Is.Null);
    }

    [Test]
    public void CreateDefaultConfiguration_NonExistentComponent_ReturnsNull()
    {
        var config = _registry.CreateDefaultConfiguration("NonExistent");

        Assert.That(config, Is.Null);
    }

    #endregion

    #region ValidateConfiguration Tests

    [Test]
    public void ValidateConfiguration_ValidConfig_ReturnsEmpty()
    {
        var config = new TestConfiguration
        {
            TestProperty = "Valid",
            OptionalNumber = 50,
            TestGuid = Guid.NewGuid()
        };

        var errors = _registry.ValidateConfiguration("Test", config);

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void ValidateConfiguration_MissingRequired_ReturnsError()
    {
        var config = new TestConfiguration
        {
            TestProperty = "", // Required but empty
            OptionalNumber = 50
        };

        var errors = _registry.ValidateConfiguration("Test", config);

        Assert.That(errors, Is.Not.Empty);
        Assert.That(errors.Any(e => e.Contains("Test Property")), Is.True);
    }

    [Test]
    public void ValidateConfiguration_NumberOutOfRange_ReturnsError()
    {
        var config = new TestConfiguration
        {
            TestProperty = "Valid",
            OptionalNumber = 150 // Max is 100
        };

        var errors = _registry.ValidateConfiguration("Test", config);

        Assert.That(errors, Is.Not.Empty);
        Assert.That(errors.Any(e => e.Contains("Optional Number")), Is.True);
    }

    [Test]
    public void ValidateConfiguration_UnknownComponent_ReturnsError()
    {
        var errors = _registry.ValidateConfiguration("NonExistent", new object());

        Assert.That(errors, Is.Not.Empty);
        Assert.That(errors.Any(e => e.Contains("Unknown component")), Is.True);
    }

    [Test]
    public void ValidateConfiguration_JsonString_ParsesAndValidates()
    {
        var json = "{\"testProperty\": \"Valid\", \"optionalNumber\": 50}";

        var errors = _registry.ValidateConfiguration("Test", json);

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void ValidateConfiguration_InvalidJson_ReturnsError()
    {
        var json = "{ invalid json }";

        var errors = _registry.ValidateConfiguration("Test", json);

        Assert.That(errors, Is.Not.Empty);
        Assert.That(errors.Any(e => e.Contains("Invalid JSON")), Is.True);
    }

    #endregion

    #region GetComponentsByCategory Tests

    [Test]
    public void GetComponentsByCategory_ReturnsGroupedDictionary()
    {
        var grouped = _registry.GetComponentsByCategory();

        Assert.That(grouped, Is.Not.Null);
        Assert.That(grouped.ContainsKey("Testing"), Is.True);
        Assert.That(grouped.ContainsKey("Other"), Is.True);
        Assert.That(grouped["Testing"].Count, Is.EqualTo(2));
        Assert.That(grouped["Other"].Count, Is.EqualTo(1));
    }

    #endregion
}
