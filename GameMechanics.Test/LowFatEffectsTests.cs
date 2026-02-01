using Csla;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMechanics.Test;

/// <summary>
/// Tests for low FAT and low VIT effects that require Focus checks to stay conscious.
/// Per GAME_RULES_SPECIFICATION.md:
/// - FAT = 3: Focus check TV 5
/// - FAT = 2: Focus check TV 7
/// - FAT = 1: Focus check TV 12
/// - FAT = 0: Passes out automatically
/// - VIT = 3: Focus check TV 7
/// - VIT = 2: Focus check TV 12
/// - VIT = 1: Passes out automatically (cannot act)
/// </summary>
[TestClass]
public class LowFatEffectsTests : TestBase
{
    [TestMethod]
    public void SkillCheck_WithTargetValue_ComparesAgainstTV()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var character = dp.Create(42);

        // Get the character's Focus AS for reference
        var focusSkill = character.Skills.FirstOrDefault(s => s.Name == "Focus");
        Assert.IsNotNull(focusSkill, "Character should have Focus skill");
        var focusAS = focusSkill.AbilityScore;

        // Run multiple checks to verify the TV is being used
        // With the fix, success should be based on (AS + roll - TV) >= 0
        // Without the fix, it would be (AS + roll) >= 0 (ignoring TV)

        // A very high TV should make success harder
        int highTVSuccesses = 0;
        int lowTVSuccesses = 0;
        const int trials = 100;

        for (int i = 0; i < trials; i++)
        {
            var highTVResult = character.Skills.SkillCheck("Focus", 20);
            var lowTVResult = character.Skills.SkillCheck("Focus", 0);

            if (highTVResult.Success) highTVSuccesses++;
            if (lowTVResult.Success) lowTVSuccesses++;
        }

        // With TV properly applied:
        // - TV 0 should succeed almost always (AS + roll >= 0)
        // - TV 20 should succeed rarely (AS + roll >= 20)
        Assert.IsTrue(lowTVSuccesses > highTVSuccesses,
            $"Lower TV should yield more successes. TV 0: {lowTVSuccesses}, TV 20: {highTVSuccesses}");
    }

    [TestMethod]
    public void EndOfRound_FatZero_CharacterPassesOut()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var character = dp.Create(42);

        // Set FAT to 0
        character.Fatigue.Value = 0;
        character.IsPassedOut = false;

        // Act
        character.EndOfRound(effectPortal);

        // Assert - character should automatically pass out at FAT 0
        Assert.IsTrue(character.IsPassedOut, "Character should pass out when FAT = 0");
    }

    [TestMethod]
    public void EndOfRound_FatFourOrHigher_NoFocusCheckRequired()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var character = dp.Create(42);

        // Set FAT to 4 (no check required)
        character.Fatigue.Value = 4;
        character.Fatigue.PendingDamage = 0; // Prevent FAT changes
        character.Fatigue.PendingHealing = 0;
        character.IsPassedOut = false;

        // Act - run multiple rounds
        for (int i = 0; i < 10; i++)
        {
            character.EndOfRound(effectPortal);
            character.Fatigue.Value = 4; // Reset FAT after each round
        }

        // Assert - character should never pass out at FAT >= 4
        Assert.IsFalse(character.IsPassedOut, "Character should not pass out when FAT >= 4");
    }

    [TestMethod]
    public void EndOfRound_VitOne_CharacterPassesOut()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var character = dp.Create(42);

        // Set VIT to 1 (cannot act)
        character.Vitality.Value = 1;
        character.Vitality.PendingDamage = 0;
        character.Vitality.PendingHealing = 0;
        character.IsPassedOut = false;

        // Act
        character.EndOfRound(effectPortal);

        // Assert - character should automatically pass out at VIT = 1
        Assert.IsTrue(character.IsPassedOut, "Character should pass out when VIT = 1");
    }

    [TestMethod]
    public void EndOfRound_VitFourOrHigher_NoFocusCheckRequired()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var character = dp.Create(42);

        // Set VIT to 4 (no check required)
        character.Vitality.Value = 4;
        character.Vitality.PendingDamage = 0;
        character.Vitality.PendingHealing = 0;
        character.IsPassedOut = false;

        // Act - run multiple rounds
        for (int i = 0; i < 10; i++)
        {
            character.EndOfRound(effectPortal);
            character.Vitality.Value = 4; // Reset VIT after each round
        }

        // Assert - character should never pass out at VIT >= 4
        Assert.IsFalse(character.IsPassedOut, "Character should not pass out when VIT >= 4");
    }

    [TestMethod]
    public void EndOfRound_LowFat_MayFailFocusCheck()
    {
        // This is a probabilistic test - with low Focus AS and high TV,
        // the character should fail the Focus check and pass out eventually.
        // We test with FAT = 1 (TV 12) which is very hard to pass.

        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var character = dp.Create(42);

        // Get Focus skill AS
        var focusSkill = character.Skills.FirstOrDefault(s => s.Name == "Focus");
        Assert.IsNotNull(focusSkill, "Character should have Focus skill");
        var focusAS = focusSkill.AbilityScore;

        // Set FAT to 1 (requires TV 12 Focus check)
        int passedOutCount = 0;
        const int trials = 100;

        for (int i = 0; i < trials; i++)
        {
            character.Fatigue.Value = 1;
            character.Fatigue.PendingDamage = 0;
            character.Fatigue.PendingHealing = 0;
            character.IsPassedOut = false;

            character.EndOfRound(effectPortal);

            if (character.IsPassedOut)
                passedOutCount++;
        }

        // With Focus AS typically around 5-10 and TV 12,
        // character should fail more often than succeed
        // (needs to roll 12 - AS or higher, which is typically 2-7)
        // 4dF+ has a bell curve centered around 0, so rolling 2-7 is rare
        Assert.IsTrue(passedOutCount > 0,
            $"Character with FAT = 1 should sometimes fail Focus check TV 12 and pass out. " +
            $"Focus AS = {focusAS}, Passed out {passedOutCount}/{trials} times");
    }

    [TestMethod]
    public void EndOfRound_LowVit_MayFailFocusCheck()
    {
        // Similar test for VIT = 2 (TV 12)

        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var character = dp.Create(42);

        // Get Focus skill AS
        var focusSkill = character.Skills.FirstOrDefault(s => s.Name == "Focus");
        Assert.IsNotNull(focusSkill, "Character should have Focus skill");
        var focusAS = focusSkill.AbilityScore;

        // Set VIT to 2 (requires TV 12 Focus check)
        int passedOutCount = 0;
        const int trials = 100;

        for (int i = 0; i < trials; i++)
        {
            character.Vitality.Value = 2;
            character.Vitality.PendingDamage = 0;
            character.Vitality.PendingHealing = 0;
            character.IsPassedOut = false;

            character.EndOfRound(effectPortal);

            if (character.IsPassedOut)
                passedOutCount++;
        }

        Assert.IsTrue(passedOutCount > 0,
            $"Character with VIT = 2 should sometimes fail Focus check TV 12 and pass out. " +
            $"Focus AS = {focusAS}, Passed out {passedOutCount}/{trials} times");
    }

    [TestMethod]
    public void FocusCheck_SuccessRateDecreasesWithHigherTV()
    {
        // Verify that higher TV means lower success rate

        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var character = dp.Create(42);

        int tv5Successes = 0;
        int tv7Successes = 0;
        int tv12Successes = 0;
        const int trials = 200;

        for (int i = 0; i < trials; i++)
        {
            if (character.Skills.SkillCheck("Focus", 5).Success) tv5Successes++;
            if (character.Skills.SkillCheck("Focus", 7).Success) tv7Successes++;
            if (character.Skills.SkillCheck("Focus", 12).Success) tv12Successes++;
        }

        // Assert - success rate should decrease with higher TV
        Assert.IsTrue(tv5Successes >= tv7Successes,
            $"TV 5 ({tv5Successes}) should have >= success rate than TV 7 ({tv7Successes})");
        Assert.IsTrue(tv7Successes >= tv12Successes,
            $"TV 7 ({tv7Successes}) should have >= success rate than TV 12 ({tv12Successes})");
    }
}
