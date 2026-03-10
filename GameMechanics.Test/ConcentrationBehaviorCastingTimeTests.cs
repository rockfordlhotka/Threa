using Csla;
using Csla.Configuration;
using GameMechanics.Effects;
using GameMechanics.Effects.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class ConcentrationBehaviorCastingTimeTests : TestBase
{

    #region OnTick Progress Tests

    [TestMethod]
    public void OnTick_CastingTime_IncrementsProgress()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var stateJson = ConcentrationBehavior.CreateMagazineReloadState(
            weaponItemId: Guid.NewGuid(),
            magazineItemId: Guid.NewGuid(),
            roundsToLoad: 30,
            totalRounds: 3);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            ConcentrationBehavior.MagazineReloadName,
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Act - first tick
        c.Effects.EndOfRound(0);

        // Assert - effect should still exist with progress = 1
        var concentrationEffect = c.Effects.FirstOrDefault(e => e.EffectType == EffectType.Concentration);
        Assert.IsNotNull(concentrationEffect, "Effect should still exist after first tick");

        var state = ConcentrationState.FromJson(concentrationEffect.BehaviorState);
        Assert.IsNotNull(state);
        Assert.AreEqual(1, state.CurrentProgress, "Progress should be 1 after first tick");
    }

    [TestMethod]
    public void OnTick_CastingTime_ExpiresEarlyWhenComplete()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var stateJson = ConcentrationBehavior.CreateMagazineReloadState(
            weaponItemId: Guid.NewGuid(),
            magazineItemId: Guid.NewGuid(),
            roundsToLoad: 30,
            totalRounds: 3);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            ConcentrationBehavior.MagazineReloadName,
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Act - run 3 ticks
        c.Effects.EndOfRound(0);
        c.Effects.EndOfRound(0);
        c.Effects.EndOfRound(0);

        // Assert - effect should be removed after completion
        Assert.AreEqual(0, c.Effects.Count, "Effect should be removed after completion");
    }

    [TestMethod]
    public void OnTick_CastingTime_UpdatesDescription()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var stateJson = ConcentrationBehavior.CreateMagazineReloadState(
            weaponItemId: Guid.NewGuid(),
            magazineItemId: Guid.NewGuid(),
            roundsToLoad: 30,
            totalRounds: 5);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            ConcentrationBehavior.MagazineReloadName,
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Act - first tick
        c.Effects.EndOfRound(0);

        // Assert - description should show progress
        var concentrationEffect = c.Effects.FirstOrDefault(e => e.EffectType == EffectType.Concentration);
        Assert.IsNotNull(concentrationEffect);
        Assert.IsTrue(concentrationEffect.Description?.Contains("1/5"),
            $"Description should contain progress '1/5'. Actual: {concentrationEffect.Description}");
    }

    #endregion

    #region OnExpire Tests

    [TestMethod]
    public void OnExpire_MagazineReload_SetsCompletionResult()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var weaponId = Guid.NewGuid();
        var magazineId = Guid.NewGuid();
        var stateJson = ConcentrationBehavior.CreateMagazineReloadState(
            weaponItemId: weaponId,
            magazineItemId: magazineId,
            roundsToLoad: 30,
            totalRounds: 2);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            ConcentrationBehavior.MagazineReloadName,
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Act - run to completion
        c.Effects.EndOfRound(0);
        c.Effects.EndOfRound(0);

        // Assert
        Assert.IsNotNull(c.LastConcentrationResult, "Should have completion result");
        Assert.AreEqual("MagazineReload", c.LastConcentrationResult.ActionType);
        Assert.IsTrue(c.LastConcentrationResult.Success, "Success should be true for completion");
        Assert.IsNotNull(c.LastConcentrationResult.Payload);
        Assert.IsTrue(c.LastConcentrationResult.Message.Contains("30 rounds"));

        // Verify payload can be deserialized
        var payload = MagazineReloadPayload.FromJson(c.LastConcentrationResult.Payload);
        Assert.IsNotNull(payload);
        Assert.AreEqual(weaponId, payload.WeaponItemId);
        Assert.AreEqual(magazineId, payload.MagazineItemId);
        Assert.AreEqual(30, payload.RoundsToLoad);
    }

    [TestMethod]
    public void OnExpire_SpellCast_SetsCompletionResult()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var targetId = Guid.NewGuid();
        var stateJson = ConcentrationBehavior.CreateSpellCastingState(
            spellId: 42,
            targetId: targetId,
            castingRounds: 2,
            spellName: "Fireball");

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Casting Fireball",
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Act - run to completion
        c.Effects.EndOfRound(0);
        c.Effects.EndOfRound(0);

        // Assert
        Assert.IsNotNull(c.LastConcentrationResult);
        Assert.AreEqual("SpellCast", c.LastConcentrationResult.ActionType);
        Assert.IsTrue(c.LastConcentrationResult.Success);
        Assert.IsTrue(c.LastConcentrationResult.Message.Contains("Fireball"));

        // Verify payload
        var payload = SpellCastPayload.FromJson(c.LastConcentrationResult.Payload);
        Assert.IsNotNull(payload);
        Assert.AreEqual(42, payload.SpellId);
        Assert.AreEqual(targetId, payload.TargetId);
    }

    #endregion

    #region OnRemove Tests

    [TestMethod]
    public void OnRemove_CastingTime_SetsInterruptionResult()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var stateJson = ConcentrationBehavior.CreateMagazineReloadState(
            weaponItemId: Guid.NewGuid(),
            magazineItemId: Guid.NewGuid(),
            roundsToLoad: 30,
            totalRounds: 5);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            ConcentrationBehavior.MagazineReloadName,
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Run one tick then interrupt
        c.Effects.EndOfRound(0);

        // Act - manually remove (simulating interruption)
        ConcentrationBehavior.BreakConcentration(c);

        // Assert
        Assert.IsNotNull(c.LastConcentrationResult);
        Assert.AreEqual("MagazineReload", c.LastConcentrationResult.ActionType);
        Assert.IsFalse(c.LastConcentrationResult.Success, "Success should be false for interruption");
        Assert.IsTrue(c.LastConcentrationResult.Message.Contains("interrupted"));
    }

    [TestMethod]
    public void OnRemove_CastingTime_DoesNotExecuteDeferredAction()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var stateJson = ConcentrationBehavior.CreateMagazineReloadState(
            weaponItemId: Guid.NewGuid(),
            magazineItemId: Guid.NewGuid(),
            roundsToLoad: 30,
            totalRounds: 5);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            ConcentrationBehavior.MagazineReloadName,
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Run one tick
        c.Effects.EndOfRound(0);

        // Clear any existing result
        c.ClearConcentrationResult();

        // Act - interrupt the concentration
        ConcentrationBehavior.BreakConcentration(c);

        // Assert - result should show failure, not success
        Assert.IsNotNull(c.LastConcentrationResult);
        Assert.IsFalse(c.LastConcentrationResult.Success,
            "Interrupted concentration should not have Success=true (deferred action not executed)");
    }

    #endregion

    #region Helper Method Tests

    [TestMethod]
    public void CreateMagazineReloadState_CreatesValidState()
    {
        // Arrange
        var weaponId = Guid.NewGuid();
        var magazineId = Guid.NewGuid();

        // Act
        var stateJson = ConcentrationBehavior.CreateMagazineReloadState(
            weaponItemId: weaponId,
            magazineItemId: magazineId,
            roundsToLoad: 30,
            totalRounds: 3);

        var state = ConcentrationState.FromJson(stateJson);

        // Assert
        Assert.IsNotNull(state);
        Assert.AreEqual("MagazineReload", state.ConcentrationType);
        Assert.AreEqual(3, state.TotalRequired);
        Assert.AreEqual(0, state.CurrentProgress);
        Assert.AreEqual(1, state.RoundsPerTick);
        Assert.AreEqual(magazineId, state.TargetItemId);
        Assert.AreEqual(weaponId, state.SourceItemId);
        Assert.AreEqual("MagazineReload", state.DeferredActionType);
        Assert.IsNotNull(state.DeferredActionPayload);
        Assert.IsNotNull(state.CompletionMessage);
        Assert.IsNotNull(state.InterruptionMessage);

        // Verify nested payload
        var payload = MagazineReloadPayload.FromJson(state.DeferredActionPayload);
        Assert.IsNotNull(payload);
        Assert.AreEqual(weaponId, payload.WeaponItemId);
        Assert.AreEqual(magazineId, payload.MagazineItemId);
        Assert.AreEqual(30, payload.RoundsToLoad);
    }

    [TestMethod]
    public void CreateWeaponReloadState_Magazine_AlwaysTakesOneRound()
    {
        // Arrange - magazine with 30 rounds
        var weaponId = Guid.NewGuid();
        var magazineId = Guid.NewGuid();

        // Act
        var stateJson = ConcentrationBehavior.CreateWeaponReloadState(
            weaponItemId: weaponId,
            ammoSourceItemId: magazineId,
            roundsToLoad: 30,
            isLooseAmmo: false);

        var state = ConcentrationState.FromJson(stateJson);

        // Assert - magazine reload should always take exactly 1 round regardless of capacity
        Assert.IsNotNull(state);
        Assert.AreEqual(1, state.TotalRequired, "Magazine reload must always take exactly 1 round");
    }

    [TestMethod]
    public void CreateWeaponReloadState_LooseAmmo_TakesRoundsBasedOnCount()
    {
        // Arrange - loading 9 loose rounds
        var weaponId = Guid.NewGuid();
        var ammoId = Guid.NewGuid();

        // Act
        var stateJson = ConcentrationBehavior.CreateWeaponReloadState(
            weaponItemId: weaponId,
            ammoSourceItemId: ammoId,
            roundsToLoad: 9,
            isLooseAmmo: true);

        var state = ConcentrationState.FromJson(stateJson);

        // Assert - 9 loose rounds at 3 per game round = 3 game rounds
        Assert.IsNotNull(state);
        Assert.AreEqual(3, state.TotalRequired, "9 loose rounds should take 3 game rounds (3 rounds per game round)");
    }

    [TestMethod]
    public void CreateSpellCastingState_CreatesValidState()
    {
        // Arrange
        var targetId = Guid.NewGuid();

        // Act
        var stateJson = ConcentrationBehavior.CreateSpellCastingState(
            spellId: 99,
            targetId: targetId,
            castingRounds: 4,
            spellName: "Lightning Bolt");

        var state = ConcentrationState.FromJson(stateJson);

        // Assert
        Assert.IsNotNull(state);
        Assert.AreEqual("SpellCasting", state.ConcentrationType);
        Assert.AreEqual(4, state.TotalRequired);
        Assert.AreEqual(0, state.CurrentProgress);
        Assert.AreEqual(1, state.RoundsPerTick);
        Assert.AreEqual("SpellCast", state.DeferredActionType);
        Assert.IsNotNull(state.DeferredActionPayload);
        Assert.IsTrue(state.CompletionMessage?.Contains("Lightning Bolt"));
        Assert.IsTrue(state.InterruptionMessage?.Contains("Lightning Bolt"));

        // Verify nested payload
        var payload = SpellCastPayload.FromJson(state.DeferredActionPayload);
        Assert.IsNotNull(payload);
        Assert.AreEqual(99, payload.SpellId);
        Assert.AreEqual(targetId, payload.TargetId);
    }

    #endregion

    #region CharacterEdit Integration Tests

    [TestMethod]
    public void LastConcentrationResult_ClearConcentrationResult_ClearsResult()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var c = dp.Create(42);

        c.LastConcentrationResult = new ConcentrationCompletionResult
        {
            ActionType = "Test",
            Message = "Test message",
            Success = true
        };

        Assert.IsNotNull(c.LastConcentrationResult);

        // Act
        c.ClearConcentrationResult();

        // Assert
        Assert.IsNull(c.LastConcentrationResult);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void OnTick_InvalidBehaviorState_ExpireEarly()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        // Create effect with invalid JSON
        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            ConcentrationBehavior.MagazineReloadName,
            null,
            null,
            "invalid json");

        c.Effects.AddEffect(effect);

        // Act
        c.Effects.EndOfRound(0);

        // Assert - effect should be removed due to invalid state
        Assert.AreEqual(0, c.Effects.Count, "Effect with invalid state should be removed");
    }

    [TestMethod]
    public void OnTick_SpellCasting_TracksProgress()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var stateJson = ConcentrationBehavior.CreateSpellCastingState(
            spellId: 1,
            targetId: null,
            castingRounds: 4,
            spellName: "Heal");

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Casting Heal",
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Act - run 2 ticks
        c.Effects.EndOfRound(0);
        c.Effects.EndOfRound(0);

        // Assert - should still be concentrating with progress = 2
        var concentrationEffect = c.Effects.FirstOrDefault(e => e.EffectType == EffectType.Concentration);
        Assert.IsNotNull(concentrationEffect);

        var state = ConcentrationState.FromJson(concentrationEffect.BehaviorState);
        Assert.IsNotNull(state);
        Assert.AreEqual(2, state.CurrentProgress);

        // Description should show spell casting progress
        Assert.IsTrue(concentrationEffect.Description?.Contains("Casting spell"));
        Assert.IsTrue(concentrationEffect.Description?.Contains("2/4"));
    }

    #endregion

    #region Medical Healing Tests

    [TestMethod]
    public void CreateMedicalHealingState_CreatesValidState()
    {
        // Arrange & Act
        var stateJson = ConcentrationBehavior.CreateMedicalHealingState(
            targetCharacterId: 1,
            targetName: "TestPatient",
            healerCharacterId: 2,
            healerName: "TestHealer",
            skillName: "First-Aid",
            successValue: 3,
            concentrationRounds: 2);

        var state = ConcentrationState.FromJson(stateJson);

        // Assert
        Assert.IsNotNull(state);
        Assert.AreEqual("MedicalHealing", state.ConcentrationType);
        Assert.AreEqual(2, state.TotalRequired);
        Assert.AreEqual(0, state.CurrentProgress);
        Assert.AreEqual(1, state.RoundsPerTick);
        Assert.AreEqual("MedicalHealing", state.DeferredActionType);
        Assert.IsNotNull(state.DeferredActionPayload);
        Assert.IsTrue(state.CompletionMessage?.Contains("TestHealer"));
        Assert.IsTrue(state.CompletionMessage?.Contains("First-Aid"));
        Assert.IsTrue(state.InterruptionMessage?.Contains("First-Aid"));

        // Verify nested payload
        var payload = MedicalHealingPayload.FromJson(state.DeferredActionPayload);
        Assert.IsNotNull(payload);
        Assert.AreEqual(1, payload.TargetCharacterId);
        Assert.AreEqual("TestPatient", payload.TargetName);
        Assert.AreEqual(2, payload.HealerCharacterId);
        Assert.AreEqual("TestHealer", payload.HealerName);
        Assert.AreEqual("First-Aid", payload.SkillName);
        Assert.AreEqual(3, payload.SuccessValue);
    }

    [TestMethod]
    public void OnTick_MedicalHealing_TracksProgress()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var stateJson = ConcentrationBehavior.CreateMedicalHealingState(
            targetCharacterId: c.Id,
            targetName: c.Name,
            healerCharacterId: c.Id,
            healerName: c.Name,
            skillName: "First-Aid",
            successValue: 3,
            concentrationRounds: 2);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Treating: Test",
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Act - first tick
        c.Effects.EndOfRound(0);

        // Assert - should still be concentrating with progress = 1
        var concentrationEffect = c.Effects.FirstOrDefault(e => e.EffectType == EffectType.Concentration);
        Assert.IsNotNull(concentrationEffect);

        var state = ConcentrationState.FromJson(concentrationEffect.BehaviorState);
        Assert.IsNotNull(state);
        Assert.AreEqual(1, state.CurrentProgress);

        // Description should show medical progress
        Assert.IsTrue(concentrationEffect.Description?.Contains("Treating patient"));
        Assert.IsTrue(concentrationEffect.Description?.Contains("1/2"));
    }

    [TestMethod]
    public void OnExpire_MedicalHealing_SetsCompletionResult()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var stateJson = ConcentrationBehavior.CreateMedicalHealingState(
            targetCharacterId: c.Id,
            targetName: c.Name,
            healerCharacterId: c.Id,
            healerName: c.Name,
            skillName: "First-Aid",
            successValue: 3,  // SV 3 = 2 healing
            concentrationRounds: 2);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Treating: Test",
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Act - run to completion
        c.Effects.EndOfRound(0);
        c.Effects.EndOfRound(0);

        // Assert
        Assert.AreEqual(0, c.Effects.Count, "Effect should be removed after completion");
        Assert.IsNotNull(c.LastConcentrationResult);
        Assert.AreEqual("MedicalHealing", c.LastConcentrationResult.ActionType);
        Assert.IsTrue(c.LastConcentrationResult.Success);
        Assert.IsNotNull(c.LastConcentrationResult.Payload);
        Assert.IsTrue(c.LastConcentrationResult.Message.Contains("heals"));

        // Verify payload can be deserialized
        var payload = MedicalHealingPayload.FromJson(c.LastConcentrationResult.Payload);
        Assert.IsNotNull(payload);
        Assert.AreEqual(3, payload.SuccessValue);
    }

    [TestMethod]
    public void OnRemove_MedicalHealing_SetsInterruptionResult()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var stateJson = ConcentrationBehavior.CreateMedicalHealingState(
            targetCharacterId: c.Id,
            targetName: c.Name,
            healerCharacterId: c.Id,
            healerName: c.Name,
            skillName: "Doctor",
            successValue: 5,
            concentrationRounds: 4);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Treating: Test",
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Run one tick then interrupt
        c.Effects.EndOfRound(0);

        // Act - manually remove (simulating interruption)
        ConcentrationBehavior.BreakConcentration(c);

        // Assert
        Assert.IsNotNull(c.LastConcentrationResult);
        Assert.AreEqual("MedicalHealing", c.LastConcentrationResult.ActionType);
        Assert.IsFalse(c.LastConcentrationResult.Success, "Success should be false for interruption");
        Assert.IsTrue(c.LastConcentrationResult.Message.Contains("interrupted"));
    }

    [TestMethod]
    public void MedicalHealing_FailedCheck_CompletesWithNoHealing()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        // Negative SV = failed check, but concentration still happens
        var stateJson = ConcentrationBehavior.CreateMedicalHealingState(
            targetCharacterId: c.Id,
            targetName: c.Name,
            healerCharacterId: c.Id,
            healerName: c.Name,
            skillName: "First-Aid",
            successValue: -2,  // Failed check
            concentrationRounds: 2);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Treating: Test",
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Act - run to completion
        c.Effects.EndOfRound(0);
        c.Effects.EndOfRound(0);

        // Assert - concentration completes but message indicates no healing
        Assert.IsNotNull(c.LastConcentrationResult);
        Assert.IsTrue(c.LastConcentrationResult.Success, "Concentration completes regardless of check result");
        Assert.IsTrue(c.LastConcentrationResult.Message.Contains("no healing") ||
                      c.LastConcentrationResult.Message.Contains("failed"),
            $"Message should indicate no healing. Actual: {c.LastConcentrationResult.Message}");
    }

    #endregion
}
