using System;
using Csla.Configuration;
using Csla.Serialization;
using Csla.Serialization.Mobile;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class EffectTemplateSerializationTests
{
    /// <summary>
    /// Builds a minimal CSLA service provider and returns the configured
    /// serialization formatter (MobileFormatter by default). CSLA 10 no longer
    /// allows constructing <see cref="SerializationInfo"/> directly, so tests
    /// must round-trip objects through the real formatter.
    /// </summary>
    private static ISerializationFormatter CreateFormatter(out ServiceProvider provider)
    {
        var services = new ServiceCollection();
        services.AddCsla();
        provider = services.BuildServiceProvider();
        return provider.GetRequiredService<ISerializationFormatter>();
    }

    [TestMethod]
    public void EffectTemplateDto_ImplementsIMobileObject()
    {
        var dto = new EffectTemplateDto();

        Assert.IsInstanceOfType<IMobileObject>(dto,
            "EffectTemplateDto must implement IMobileObject to be serialized by MobileFormatter.");
    }

    [TestMethod]
    public void EffectTemplateDto_CanSerializeAndDeserialize()
    {
        // Arrange
        var formatter = CreateFormatter(out var provider);
        using var _ = provider;

        var original = new EffectTemplateDto
        {
            Id = 123,
            Name = "Blessed",
            EffectType = EffectType.Buff,
            Description = "Blessed by the gods",
            IconName = "holy-icon",
            Color = "#ffff00",
            DefaultDurationValue = 10,
            DurationType = DurationType.Minutes,
            StateJson = "{\"asModifier\":2}",
            Tags = "buff,holy",
            IsSystem = true,
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 15, 14, 30, 0, DateTimeKind.Utc)
        };

        // Act - round-trip through the configured MobileFormatter
        var data = formatter.Serialize(original);
        var deserialized = (EffectTemplateDto)formatter.Deserialize(data)!;

        // Assert - All properties should match
        Assert.AreEqual(original.Id, deserialized.Id);
        Assert.AreEqual(original.Name, deserialized.Name);
        Assert.AreEqual(original.EffectType, deserialized.EffectType);
        Assert.AreEqual(original.Description, deserialized.Description);
        Assert.AreEqual(original.IconName, deserialized.IconName);
        Assert.AreEqual(original.Color, deserialized.Color);
        Assert.AreEqual(original.DefaultDurationValue, deserialized.DefaultDurationValue);
        Assert.AreEqual(original.DurationType, deserialized.DurationType);
        Assert.AreEqual(original.StateJson, deserialized.StateJson);
        Assert.AreEqual(original.Tags, deserialized.Tags);
        Assert.AreEqual(original.IsSystem, deserialized.IsSystem);
        Assert.AreEqual(original.IsActive, deserialized.IsActive);
        Assert.AreEqual(original.CreatedAt, deserialized.CreatedAt);
        Assert.AreEqual(original.UpdatedAt, deserialized.UpdatedAt);
    }
}
