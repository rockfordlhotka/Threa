using Csla;
using Csla.Configuration;
using GameMechanics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Threa.Dal;

namespace GameMechanics.Test;

/// <summary>
/// Tests to verify the round duration bug fix (3 seconds per round, not 6).
/// </summary>
[TestClass]
public class RoundDurationFixTests : TestBase
{

    [TestMethod]
    public async Task EffectDuration_5Rounds_ShouldBe15Seconds()
    {
        // Arrange
        using var services = InitServices();
        var characterPortal = services.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = services.GetRequiredService<IChildDataPortal<EffectRecord>>();

        var character = await characterPortal.CreateAsync(1);
        character.Name = "Test Character";
        character.CurrentGameTimeSeconds = 100;

        // Act - Create effect with epoch-based expiration (5 rounds = 15 seconds)
        var effect = await effectPortal.CreateChildAsync(
            Threa.Dal.Dto.EffectType.Buff,
            "Test Buff",
            (string?)null,
            15L, // 5 rounds × 3 seconds = 15 seconds duration
            null,
            100L // current game time
        );

        // Assert - 5 rounds × 3 seconds = 15 seconds
        Assert.AreEqual(100L, effect.CreatedAtEpochSeconds, "Effect should be created at current game time (100)");
        Assert.AreEqual(115L, effect.ExpiresAtEpochSeconds, "Effect should expire at 115 seconds (100 + 15 sec)");
    }

    [TestMethod]
    public async Task EffectExpiration_1Round_ShouldNotExpire()
    {
        // Arrange
        using var services = InitServices();
        var characterPortal = services.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = services.GetRequiredService<IChildDataPortal<EffectRecord>>();

        var character = await characterPortal.CreateAsync(1);
        character.Name = "Test Character";
        character.CurrentGameTimeSeconds = 100;

        var effect = await effectPortal.CreateChildAsync(
            Threa.Dal.Dto.EffectType.Buff,
            "Test Buff",
            (string?)null,
            15L, // 5 rounds × 3 seconds = 15 seconds
            null,
            100L // current game time
        );

        // Act - Advance time by 1 round (3 seconds)
        character.CurrentGameTimeSeconds += 3;

        // Assert - Effect should NOT be expired (at 103, expires at 115)
        Assert.IsFalse(effect.IsExpired(character.CurrentGameTimeSeconds),
            "Effect should not expire after 1 round (3 seconds) when duration is 5 rounds (15 seconds)");
    }

    [TestMethod]
    public async Task EffectExpiration_5Rounds_ShouldExpire()
    {
        // Arrange
        using var services = InitServices();
        var characterPortal = services.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = services.GetRequiredService<IChildDataPortal<EffectRecord>>();

        var character = await characterPortal.CreateAsync(1);
        character.Name = "Test Character";
        character.CurrentGameTimeSeconds = 100;

        var effect = await effectPortal.CreateChildAsync(
            Threa.Dal.Dto.EffectType.Buff,
            "Test Buff",
            (string?)null,
            15L, // 5 rounds × 3 seconds = 15 seconds
            null,
            100L // current game time
        );

        // Act - Advance time by 5 rounds (15 seconds) from 100 to 115
        character.CurrentGameTimeSeconds += 15;

        // Assert - Effect SHOULD be expired (at 115, expires at 115)
        Assert.IsTrue(effect.IsExpired(character.CurrentGameTimeSeconds),
            "Effect should expire after 5 rounds (15 seconds)");
    }

    [TestMethod]
    public async Task EndOfRound_ShouldAdvanceTimeBy3Seconds()
    {
        // Arrange
        using var services = InitServices();
        var characterPortal = services.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = services.GetRequiredService<IChildDataPortal<EffectRecord>>();

        var character = await characterPortal.CreateAsync(1);
        character.Name = "Test Character";
        character.CurrentGameTimeSeconds = 0;

        // Act
        character.EndOfRound(effectPortal);

        // Assert - 1 round = 3 seconds
        Assert.AreEqual(3L, character.CurrentGameTimeSeconds,
            "EndOfRound should advance time by 3 seconds (1 round)");
    }

    [TestMethod]
    public async Task EndOfRound_MultipleTimes_ShouldAdvanceCorrectly()
    {
        // Arrange
        using var services = InitServices();
        var characterPortal = services.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = services.GetRequiredService<IChildDataPortal<EffectRecord>>();

        var character = await characterPortal.CreateAsync(1);
        character.Name = "Test Character";
        character.CurrentGameTimeSeconds = 0;

        // Act - Advance 5 rounds
        for (int i = 0; i < 5; i++)
        {
            character.EndOfRound(effectPortal);
        }

        // Assert - 5 rounds × 3 seconds = 15 seconds
        Assert.AreEqual(15L, character.CurrentGameTimeSeconds,
            "5 rounds should advance time by 15 seconds (5 × 3 sec/round)");
    }
}
