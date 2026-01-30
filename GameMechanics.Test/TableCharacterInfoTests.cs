using Csla;
using Csla.Configuration;
using GameMechanics.GamePlay;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class TableCharacterInfoTests : TestBase
{

    [TestMethod]
    public void TableCharacterInfo_PopulatesPendingPools_FromCharacterDto()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IChildDataPortal<TableCharacterInfo>>();

        var tableChar = new TableCharacter
        {
            TableId = Guid.NewGuid(),
            CharacterId = 1,
            PlayerId = 42,
            JoinedAt = DateTime.UtcNow,
            ConnectionStatus = ConnectionStatus.Connected
        };

        var character = new Character
        {
            Id = 1,
            PlayerId = 42,
            Name = "Test Character",
            Species = "Human",
            FatValue = 8,
            FatBaseValue = 10,
            FatPendingDamage = 3,
            FatPendingHealing = 1,
            VitValue = 12,
            VitBaseValue = 15,
            VitPendingDamage = 5,
            VitPendingHealing = 2,
            ActionPointAvailable = 4,
            ActionPointMax = 6,
            Effects = []
        };

        // Act
        var info = portal.FetchChild(tableChar, character);

        // Assert
        Assert.AreEqual(3, info.FatPendingDamage, "FatPendingDamage should be 3");
        Assert.AreEqual(1, info.FatPendingHealing, "FatPendingHealing should be 1");
        Assert.AreEqual(5, info.VitPendingDamage, "VitPendingDamage should be 5");
        Assert.AreEqual(2, info.VitPendingHealing, "VitPendingHealing should be 2");
    }

    [TestMethod]
    public void TableCharacterInfo_CountsWoundsAndEffects_FromCharacterDto()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IChildDataPortal<TableCharacterInfo>>();

        var tableChar = new TableCharacter
        {
            TableId = Guid.NewGuid(),
            CharacterId = 1,
            PlayerId = 42,
            JoinedAt = DateTime.UtcNow,
            ConnectionStatus = ConnectionStatus.Connected
        };

        var character = new Character
        {
            Id = 1,
            PlayerId = 42,
            Name = "Test Character",
            Species = "Human",
            FatValue = 10,
            FatBaseValue = 10,
            VitValue = 15,
            VitBaseValue = 15,
            ActionPointAvailable = 6,
            ActionPointMax = 6,
            Effects = new List<CharacterEffect>
            {
                // 3 wounds
                new CharacterEffect { Id = Guid.NewGuid(), EffectType = EffectType.Wound, Name = "Light", IsActive = true },
                new CharacterEffect { Id = Guid.NewGuid(), EffectType = EffectType.Wound, Name = "Light", IsActive = true },
                new CharacterEffect { Id = Guid.NewGuid(), EffectType = EffectType.Wound, Name = "Serious", IsActive = true },
                // 2 non-wound effects
                new CharacterEffect { Id = Guid.NewGuid(), EffectType = EffectType.Buff, Name = "Shield Spell", IsActive = true, RoundsRemaining = 5 },
                new CharacterEffect { Id = Guid.NewGuid(), EffectType = EffectType.Poison, Name = "Poison", IsActive = true, RoundsRemaining = 3 }
            }
        };

        // Act
        var info = portal.FetchChild(tableChar, character);

        // Assert
        Assert.AreEqual(3, info.WoundCount, "WoundCount should be 3");
        Assert.AreEqual(2, info.EffectCount, "EffectCount should be 2");
    }

    [TestMethod]
    public void TableCharacterInfo_BuildsWoundSummary_WithGroupedCounts()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IChildDataPortal<TableCharacterInfo>>();

        var tableChar = new TableCharacter
        {
            TableId = Guid.NewGuid(),
            CharacterId = 1,
            PlayerId = 42,
            JoinedAt = DateTime.UtcNow,
            ConnectionStatus = ConnectionStatus.Connected
        };

        var character = new Character
        {
            Id = 1,
            PlayerId = 42,
            Name = "Test Character",
            Species = "Human",
            FatValue = 10,
            FatBaseValue = 10,
            VitValue = 15,
            VitBaseValue = 15,
            ActionPointAvailable = 6,
            ActionPointMax = 6,
            Effects = new List<CharacterEffect>
            {
                new CharacterEffect { Id = Guid.NewGuid(), EffectType = EffectType.Wound, Name = "Light", IsActive = true },
                new CharacterEffect { Id = Guid.NewGuid(), EffectType = EffectType.Wound, Name = "Light", IsActive = true },
                new CharacterEffect { Id = Guid.NewGuid(), EffectType = EffectType.Wound, Name = "Serious", IsActive = true }
            }
        };

        // Act
        var info = portal.FetchChild(tableChar, character);

        // Assert - should be "Light x2, Serious" or similar
        Assert.IsTrue(info.WoundSummary.Contains("Light x2"), $"WoundSummary should contain 'Light x2', got: {info.WoundSummary}");
        Assert.IsTrue(info.WoundSummary.Contains("Serious"), $"WoundSummary should contain 'Serious', got: {info.WoundSummary}");
    }

    [TestMethod]
    public void TableCharacterInfo_BuildsEffectSummary_WithDurations()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IChildDataPortal<TableCharacterInfo>>();

        var tableChar = new TableCharacter
        {
            TableId = Guid.NewGuid(),
            CharacterId = 1,
            PlayerId = 42,
            JoinedAt = DateTime.UtcNow,
            ConnectionStatus = ConnectionStatus.Connected
        };

        var character = new Character
        {
            Id = 1,
            PlayerId = 42,
            Name = "Test Character",
            Species = "Human",
            FatValue = 10,
            FatBaseValue = 10,
            VitValue = 15,
            VitBaseValue = 15,
            ActionPointAvailable = 6,
            ActionPointMax = 6,
            Effects = new List<CharacterEffect>
            {
                new CharacterEffect { Id = Guid.NewGuid(), EffectType = EffectType.Buff, Name = "Shield Spell", IsActive = true, RoundsRemaining = 5 },
                new CharacterEffect { Id = Guid.NewGuid(), EffectType = EffectType.Buff, Name = "Blessed", IsActive = true, RoundsRemaining = null },
                new CharacterEffect { Id = Guid.NewGuid(), EffectType = EffectType.Poison, Name = "Poison", IsActive = true, RoundsRemaining = 3 }
            }
        };

        // Act
        var info = portal.FetchChild(tableChar, character);

        // Assert
        Assert.IsTrue(info.EffectSummary.Contains("Shield Spell (5 rnd)"), $"EffectSummary should contain 'Shield Spell (5 rnd)', got: {info.EffectSummary}");
        Assert.IsTrue(info.EffectSummary.Contains("Blessed"), $"EffectSummary should contain 'Blessed', got: {info.EffectSummary}");
        Assert.IsTrue(info.EffectSummary.Contains("Poison (3 rnd)"), $"EffectSummary should contain 'Poison (3 rnd)', got: {info.EffectSummary}");
        // Blessed should NOT have " rnd" since it has no duration
        Assert.IsFalse(info.EffectSummary.Contains("Blessed ("), $"EffectSummary should not have duration for Blessed, got: {info.EffectSummary}");
    }

    [TestMethod]
    public void TableCharacterInfo_HandlesNullEffectsList_Gracefully()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IChildDataPortal<TableCharacterInfo>>();

        var tableChar = new TableCharacter
        {
            TableId = Guid.NewGuid(),
            CharacterId = 1,
            PlayerId = 42,
            JoinedAt = DateTime.UtcNow,
            ConnectionStatus = ConnectionStatus.Connected
        };

        var character = new Character
        {
            Id = 1,
            PlayerId = 42,
            Name = "Test Character",
            Species = "Human",
            FatValue = 10,
            FatBaseValue = 10,
            VitValue = 15,
            VitBaseValue = 15,
            ActionPointAvailable = 6,
            ActionPointMax = 6,
            Effects = null! // Explicitly null
        };

        // Act
        var info = portal.FetchChild(tableChar, character);

        // Assert - should handle null gracefully
        Assert.AreEqual(0, info.WoundCount, "WoundCount should be 0 for null effects");
        Assert.AreEqual(0, info.EffectCount, "EffectCount should be 0 for null effects");
        Assert.AreEqual("", info.WoundSummary, "WoundSummary should be empty for null effects");
        Assert.AreEqual("", info.EffectSummary, "EffectSummary should be empty for null effects");
    }
}
