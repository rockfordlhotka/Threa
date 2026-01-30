using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameMechanics.Effects.Behaviors;
using System;
using System.Collections.Generic;

namespace GameMechanics.Test;

[TestClass]
public class ConcentrationStateSerializationTests
{
    [TestMethod]
    public void ConcentrationState_CastingTime_SerializesAndDeserializes()
    {
        // Arrange
        var state = new ConcentrationState
        {
            ConcentrationType = "MagazineReload",
            TotalRequired = 3,
            CurrentProgress = 1,
            DeferredActionType = "MagazineReload",
            DeferredActionPayload = new MagazineReloadPayload
            {
                WeaponItemId = Guid.NewGuid(),
                MagazineItemId = Guid.NewGuid(),
                RoundsToLoad = 30
            }.Serialize(),
            CompletionMessage = "Magazine reloaded!",
            InterruptionMessage = "Reload interrupted!"
        };

        // Act
        var json = state.Serialize();
        var deserialized = ConcentrationState.FromJson(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual("MagazineReload", deserialized.ConcentrationType);
        Assert.AreEqual(3, deserialized.TotalRequired);
        Assert.AreEqual(1, deserialized.CurrentProgress);
        Assert.AreEqual("MagazineReload", deserialized.DeferredActionType);
        Assert.IsNotNull(deserialized.DeferredActionPayload);
        Assert.AreEqual("Magazine reloaded!", deserialized.CompletionMessage);
        Assert.AreEqual("Reload interrupted!", deserialized.InterruptionMessage);
    }

    [TestMethod]
    public void ConcentrationState_Sustained_SerializesAndDeserializes()
    {
        // Arrange
        var state = new ConcentrationState
        {
            ConcentrationType = "SustainedSpell",
            SpellName = "Invisibility",
            LinkedEffectIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            FatDrainPerRound = 1,
            VitDrainPerRound = 0,
            SourceCasterId = Guid.NewGuid()
        };

        // Act
        var json = state.Serialize();
        var deserialized = ConcentrationState.FromJson(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual("SustainedSpell", deserialized.ConcentrationType);
        Assert.AreEqual("Invisibility", deserialized.SpellName);
        Assert.AreEqual(2, deserialized.LinkedEffectIds?.Count);
        Assert.AreEqual(1, deserialized.FatDrainPerRound);
        Assert.AreEqual(0, deserialized.VitDrainPerRound);
        Assert.IsNotNull(deserialized.SourceCasterId);
    }

    [TestMethod]
    public void MagazineReloadPayload_SerializesAndDeserializes()
    {
        // Arrange
        var weaponId = Guid.NewGuid();
        var magazineId = Guid.NewGuid();
        var payload = new MagazineReloadPayload
        {
            WeaponItemId = weaponId,
            MagazineItemId = magazineId,
            RoundsToLoad = 30
        };

        // Act
        var json = payload.Serialize();
        var deserialized = MagazineReloadPayload.FromJson(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(weaponId, deserialized.WeaponItemId);
        Assert.AreEqual(magazineId, deserialized.MagazineItemId);
        Assert.AreEqual(30, deserialized.RoundsToLoad);
    }

    [TestMethod]
    public void SpellCastPayload_SerializesAndDeserializes()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var payload = new SpellCastPayload
        {
            SpellId = 42,
            TargetId = targetId,
            Parameters = "{\"power\":10}"
        };

        // Act
        var json = payload.Serialize();
        var deserialized = SpellCastPayload.FromJson(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(42, deserialized.SpellId);
        Assert.AreEqual(targetId, deserialized.TargetId);
        Assert.AreEqual("{\"power\":10}", deserialized.Parameters);
    }

    [TestMethod]
    public void ConcentrationState_WithNestedPayload_RoundTrips()
    {
        // Arrange
        var weaponId = Guid.NewGuid();
        var magazineId = Guid.NewGuid();

        var payload = new MagazineReloadPayload
        {
            WeaponItemId = weaponId,
            MagazineItemId = magazineId,
            RoundsToLoad = 30
        };

        var state = new ConcentrationState
        {
            ConcentrationType = "MagazineReload",
            TotalRequired = 3,
            CurrentProgress = 1,
            DeferredActionType = "MagazineReload",
            DeferredActionPayload = payload.Serialize(),
            CompletionMessage = "Reload complete"
        };

        // Act - serialize state, deserialize it, then deserialize nested payload
        var stateJson = state.Serialize();
        var deserializedState = ConcentrationState.FromJson(stateJson);
        Assert.IsNotNull(deserializedState);

        var deserializedPayload = MagazineReloadPayload.FromJson(deserializedState.DeferredActionPayload);

        // Assert
        Assert.IsNotNull(deserializedPayload);
        Assert.AreEqual(weaponId, deserializedPayload.WeaponItemId);
        Assert.AreEqual(magazineId, deserializedPayload.MagazineItemId);
        Assert.AreEqual(30, deserializedPayload.RoundsToLoad);
    }

    [TestMethod]
    public void ConcentrationState_NullProperties_SerializesProperly()
    {
        // Arrange
        var state = new ConcentrationState
        {
            ConcentrationType = "MagazineReload",
            TotalRequired = 3,
            // All other properties null/default
        };

        // Act
        var json = state.Serialize();
        var deserialized = ConcentrationState.FromJson(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual("MagazineReload", deserialized.ConcentrationType);
        Assert.AreEqual(3, deserialized.TotalRequired);
        Assert.IsNull(deserialized.DeferredActionPayload);
        Assert.IsNull(deserialized.SpellName);
        Assert.IsNull(deserialized.LinkedEffectIds);
    }

    [TestMethod]
    public void MagazineReloadPayload_FromNullJson_ReturnsNull()
    {
        // Act
        var result1 = MagazineReloadPayload.FromJson(null);
        var result2 = MagazineReloadPayload.FromJson("");
        var result3 = MagazineReloadPayload.FromJson("   ");

        // Assert
        Assert.IsNull(result1);
        Assert.IsNull(result2);
        Assert.IsNull(result3);
    }

    [TestMethod]
    public void ConcentrationState_FromNullJson_ReturnsNull()
    {
        // Act
        var result1 = ConcentrationState.FromJson(null);
        var result2 = ConcentrationState.FromJson("");
        var result3 = ConcentrationState.FromJson("   ");

        // Assert
        Assert.IsNull(result1);
        Assert.IsNull(result2);
        Assert.IsNull(result3);
    }

    [TestMethod]
    public void ConcentrationState_BackwardCompatibility_LoadsOldFormat()
    {
        // Arrange - simulate old format JSON (without new properties)
        var oldJson = @"{
            ""type"": ""MagazineReload"",
            ""totalRequired"": 30,
            ""currentProgress"": 10,
            ""roundsPerTick"": 3,
            ""targetItemId"": ""00000000-0000-0000-0000-000000000001""
        }";

        // Act
        var deserialized = ConcentrationState.FromJson(oldJson);

        // Assert - should deserialize successfully with new properties as null/default
        Assert.IsNotNull(deserialized);
        Assert.AreEqual("MagazineReload", deserialized.ConcentrationType);
        Assert.AreEqual(30, deserialized.TotalRequired);
        Assert.AreEqual(10, deserialized.CurrentProgress);
        Assert.AreEqual(3, deserialized.RoundsPerTick);
        Assert.IsNull(deserialized.DeferredActionType);
        Assert.IsNull(deserialized.CompletionMessage);
        Assert.IsNull(deserialized.SpellName);
        Assert.AreEqual(0, deserialized.FatDrainPerRound);
    }
}
