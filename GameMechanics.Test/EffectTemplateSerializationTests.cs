using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Csla;
using GameMechanics;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class EffectTemplateSerializationTests
{
    [TestMethod]
    public void EffectTemplateDto_ImplementsIMobileObject()
    {
        // Arrange
        var dto = new EffectTemplateDto();

        // Assert - Check that it's an IMobileObject by trying to use its methods
        var info = new Csla.Serialization.Mobile.SerializationInfo();

        // This will throw if IMobileObject is not properly implemented
        dto.GetState(info);

        // Success - the DTO implements IMobileObject
        Assert.IsTrue(true, "EffectTemplateDto properly implements IMobileObject");
    }

    [TestMethod]
    public void EffectTemplateDto_CanSerializeAndDeserialize()
    {
        // Arrange
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

        // Act - Manually serialize and deserialize
        var info = new Csla.Serialization.Mobile.SerializationInfo();
        original.GetState(info);

        var deserialized = new EffectTemplateDto();
        deserialized.SetState(info);

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

        Console.WriteLine("SUCCESS: All properties correctly serialized and deserialized!");
    }
}
